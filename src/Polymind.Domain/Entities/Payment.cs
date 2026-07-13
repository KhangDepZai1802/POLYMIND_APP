using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Khoản thu từ ứng viên (Module 4 / mục 7.1).</summary>
public class Payment : BaseEntity
{
    public string Code { get; set; } = default!; // PT-YYYYMMDD-XXXX
    public Guid CandidateId { get; set; }
    public Guid? JobOrderId { get; set; }
    public PaymentType PaymentType { get; set; }
    // Bước trong lịch đóng tiền 4 bước (Đặt cọc→Tất toán) tính theo chi phí đơn hàng. Null = khoản thu lẻ ngoài lịch.
    public PaymentStage? Stage { get; set; }
    public decimal Amount { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod? PaymentMethod { get; set; }
    public Guid? ReceiptId { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ApprovedBy { get; set; }

    // Kho lưu trữ: ẩn khỏi Tiến độ đóng tiền + Khoản thu khi ứng viên đã thu đủ 100%.
    // KHÔNG phải xóa và KHÔNG phải miễn nợ — bản ghi còn nguyên, vẫn tính vào tổng đã thu, khôi phục được.
    public DateTimeOffset? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }
}
