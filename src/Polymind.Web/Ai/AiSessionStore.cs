using System.Collections.Concurrent;

namespace Polymind.Web.Ai;

/// <summary>
/// RB-5: Lưu hội thoại AI + kết quả trích xuất CV theo TỪNG người dùng, tồn tại suốt phiên đăng nhập.
/// Là singleton in-memory phía server nên dữ liệu sống sót khi chuyển trang và khi F5/refresh (cùng userId),
/// chỉ bị xóa khi người dùng đăng xuất (gọi <see cref="Clear"/> ở endpoint logout).
/// </summary>
public sealed class AiSessionStore
{
    private readonly ConcurrentDictionary<Guid, AiSessionState> _sessions = new();

    /// <summary>Lấy (tạo nếu chưa có) trạng thái AI của 1 người dùng.</summary>
    public AiSessionState Get(Guid userId) => _sessions.GetOrAdd(userId, _ => new AiSessionState());

    /// <summary>Xóa toàn bộ dữ liệu AI của người dùng — gọi khi đăng xuất.</summary>
    public void Clear(Guid userId) => _sessions.TryRemove(userId, out _);
}

/// <summary>Trạng thái AI của 1 người dùng trong phiên đăng nhập.</summary>
public sealed class AiSessionState
{
    /// <summary>Lịch sử hội thoại (mutate trực tiếp để tự động lưu lại trong store).</summary>
    public List<AiChatMessage> History { get; } = new();

    /// <summary>Kết quả trích xuất CV/ảnh gần nhất.</summary>
    public AiResult? CvResult { get; set; }

    /// <summary>Tên file CV/ảnh đã trích xuất gần nhất (chỉ để hiển thị lại).</summary>
    public string? CvFileName { get; set; }
}
