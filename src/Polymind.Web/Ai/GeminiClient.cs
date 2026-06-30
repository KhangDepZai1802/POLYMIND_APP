using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Polymind.Web.Ai;

/// <summary>
/// Client gọi Google Gemini (Generative Language API) bằng API key free.
/// Hỗ trợ: sinh văn bản, hội thoại nhiều lượt, và đọc ảnh/PDF (vision) để trích xuất hồ sơ.
/// Nếu chưa cấu hình key → trả AiResult.Fail (UI hiển thị hướng dẫn, không crash).
/// </summary>
public sealed class GeminiClient
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options, ILogger<GeminiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Đã có API key chưa? UI dùng để hiện cảnh báo "chưa cấu hình".</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    /// <summary>Sinh văn bản từ 1 prompt (kèm system instruction tùy chọn).</summary>
    public Task<AiResult> GenerateTextAsync(string prompt, string? system = null, CancellationToken ct = default)
    {
        var body = new
        {
            systemInstruction = system is null ? null : new { parts = new[] { new { text = system } } },
            contents = new object[] { new { role = "user", parts = new object[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.6, maxOutputTokens = 2048 },
        };
        return CallAsync(body, ct);
    }

    /// <summary>Hội thoại nhiều lượt cho chatbot.</summary>
    public Task<AiResult> ChatAsync(IReadOnlyList<AiChatMessage> history, string? system = null, CancellationToken ct = default)
    {
        var contents = history
            .Where(m => !string.IsNullOrWhiteSpace(m.Text))
            .Select(m => (object)new
            {
                role = m.FromUser ? "user" : "model",
                parts = new object[] { new { text = m.Text } },
            })
            .ToArray();

        var body = new
        {
            systemInstruction = system is null ? null : new { parts = new[] { new { text = system } } },
            contents,
            generationConfig = new { temperature = 0.7, maxOutputTokens = 2048 },
        };
        return CallAsync(body, ct);
    }

    /// <summary>Hội thoại có đính kèm file/ảnh ở lượt user mới nhất.</summary>
    public Task<AiResult> ChatWithFileAsync(
        IReadOnlyList<AiChatMessage> history,
        byte[] data,
        string mimeType,
        string? system = null,
        CancellationToken ct = default)
    {
        var lastUserIndex = -1;
        for (var i = history.Count - 1; i >= 0; i--)
        {
            if (history[i].FromUser)
            {
                lastUserIndex = i;
                break;
            }
        }

        var contents = history
            .Select((message, index) =>
            {
                var text = string.IsNullOrWhiteSpace(message.Text)
                    ? "Hãy phân tích file/hình ảnh tôi đính kèm."
                    : message.Text;

                var parts = index == lastUserIndex
                    ? new object[]
                    {
                        new { text },
                        new { inlineData = new { mimeType, data = Convert.ToBase64String(data) } },
                    }
                    : new object[] { new { text } };

                return (object)new
                {
                    role = message.FromUser ? "user" : "model",
                    parts,
                };
            })
            .ToArray();

        var body = new
        {
            systemInstruction = system is null ? null : new { parts = new[] { new { text = system } } },
            contents,
            generationConfig = new { temperature = 0.7, maxOutputTokens = 2048 },
        };
        return CallAsync(body, ct);
    }

    /// <summary>Đọc ảnh/PDF hồ sơ (CV, CCCD...) và trả về JSON/văn bản theo prompt trích xuất.</summary>
    public Task<AiResult> ExtractFromFileAsync(byte[] data, string mimeType, string prompt, CancellationToken ct = default)
    {
        var body = new
        {
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inlineData = new { mimeType, data = Convert.ToBase64String(data) } },
                    },
                },
            },
            generationConfig = new { temperature = 0.1, maxOutputTokens = 2048 },
        };
        return CallAsync(body, ct);
    }

    private async Task<AiResult> CallAsync(object body, CancellationToken ct)
    {
        if (!IsConfigured)
            return AiResult.Fail("Chưa cấu hình Gemini API key (Ai:Gemini:ApiKey). Dán key vào appsettings.Development.json rồi khởi động lại.");

        var url = $"{BaseUrl}/{_options.Model}:generateContent?key={_options.ApiKey}";
        try
        {
            using var resp = await _http.PostAsJsonAsync(url, body, JsonOptions, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!resp.IsSuccessStatusCode)
            {
                var apiMsg = root.TryGetProperty("error", out var err) && err.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : $"HTTP {(int)resp.StatusCode}";
                _logger.LogWarning("Gemini API lỗi: {Message}", apiMsg);
                return AiResult.Fail($"Gemini API lỗi: {apiMsg}");
            }

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                // Có thể bị chặn an toàn nội dung (promptFeedback.blockReason).
                if (root.TryGetProperty("promptFeedback", out var fb) && fb.TryGetProperty("blockReason", out var br))
                    return AiResult.Fail($"Nội dung bị chặn: {br.GetString()}");
                return AiResult.Fail("Gemini không trả về kết quả.");
            }

            var text = string.Concat(candidates[0]
                .GetProperty("content")
                .GetProperty("parts")
                .EnumerateArray()
                .Where(p => p.TryGetProperty("text", out _))
                .Select(p => p.GetProperty("text").GetString()));

            return string.IsNullOrWhiteSpace(text)
                ? AiResult.Fail("Gemini trả về rỗng.")
                : AiResult.Success(text);
        }
        catch (OperationCanceledException)
        {
            return AiResult.Fail("Đã hủy yêu cầu.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi gọi Gemini");
            return AiResult.Fail($"Không gọi được Gemini: {ex.Message}");
        }
    }
}
