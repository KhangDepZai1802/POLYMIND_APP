using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>
/// Phiếu đánh giá học viên định kỳ theo 4 tiêu chí (góp ý Vietgroup 07/2026):
/// Chuyên cần / Chuyên môn / Kỷ luật / Tài chính. Có minh chứng đính kèm để Phụ huynh,
/// Đại lý xem và làm báo cáo cho đối tác Nhật.
/// </summary>
public class TrainingEvaluation : BaseEntity
{
    public Guid CandidateId { get; set; }
    public TrainingTrack? Track { get; set; }         // null = đánh giá chung (không theo mảng)
    public DateOnly EvaluationDate { get; set; }
    public EvaluationRating Attendance { get; set; }  // Chuyên cần
    public EvaluationRating Professional { get; set; }// Chuyên môn
    public EvaluationRating Discipline { get; set; }  // Kỷ luật
    public EvaluationRating Financial { get; set; }   // Tài chính
    public string? Note { get; set; }
    /// <summary>Danh sách object key MinIO của minh chứng, lưu JSON (tái dùng pattern như Message.Body).</summary>
    public string? AttachmentsJson { get; set; }
    public Guid CreatedBy { get; set; }
}
