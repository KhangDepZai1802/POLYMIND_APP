namespace Polymind.Web.Ai;

/// <summary>Kết quả 1 lần gọi AI. Ok=false thì Error chứa lý do (chưa cấu hình key, lỗi mạng, lỗi API...).</summary>
public sealed record AiResult(bool Ok, string Text, string? Error)
{
    public static AiResult Success(string text) => new(true, text, null);
    public static AiResult Fail(string error) => new(false, "", error);
}

/// <summary>1 lượt hội thoại trong chatbot.</summary>
public sealed class AiChatMessage
{
    public bool FromUser { get; set; }
    public string Text { get; set; } = "";
    public string? AttachmentName { get; set; }
    public string? AttachmentMimeType { get; set; }
}
