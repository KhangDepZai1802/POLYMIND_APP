using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>
/// Tiến trình đào tạo của ứng viên theo từng mảng (Học tiếng / Chuyên môn) — góp ý Vietgroup 07/2026.
/// Mỗi (ứng viên, mảng) một bản ghi; nguồn cho "2 ô riêng biệt" ở chi tiết ứng viên.
/// </summary>
public class TrainingRecord : BaseEntity
{
    public Guid CandidateId { get; set; }
    public TrainingTrack Track { get; set; }
    public bool IsEnrolled { get; set; } = true;      // false = đơn hàng không yêu cầu học mảng này (Bỏ qua)
    public string? LevelLabel { get; set; }           // ví dụ "N5 → N4" hoặc "Cơ bản"
    public int ProgressPercent { get; set; }          // 0..100
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
}
