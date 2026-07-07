using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>
/// Một kỳ trả góp của khoản nợ (trừ dần vào lương) — góp ý Vietgroup 07/2026.
/// Kế toán dùng ở chức năng Thu nợ để đánh dấu từng kỳ đã thu.
/// </summary>
public class LoanRepayment : BaseEntity
{
    public Guid LoanId { get; set; }
    public int InstallmentNo { get; set; }        // số thứ tự kỳ (1..N)
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }           // số tiền phải trả kỳ này
    public decimal PaidAmount { get; set; }       // đã thu được
    public DateOnly? PaidDate { get; set; }
    public LoanRepaymentStatus Status { get; set; } = LoanRepaymentStatus.Pending;
    public string? Note { get; set; }
}
