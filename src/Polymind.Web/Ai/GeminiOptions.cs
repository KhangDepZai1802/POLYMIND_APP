namespace Polymind.Web.Ai;

/// <summary>Cấu hình Gemini (Google Generative Language API). Key đặt ở Ai:Gemini:ApiKey.</summary>
public sealed class GeminiOptions
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gemini-2.5-flash";
}
