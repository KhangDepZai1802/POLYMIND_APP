using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Khoản hỗ trợ vay vốn của ứng viên (module Hỗ trợ vay). Mỗi ứng viên 1 hồ sơ vay.</summary>
public class Loan : BaseEntity
{
    public string Code { get; set; } = default!; // VAY-XXXX
    public Guid CandidateId { get; set; }
    public LoanKind Kind { get; set; } = LoanKind.Bank; // Vay ngân hàng / Nợ công ty
    public LoanStatus Status { get; set; } = LoanStatus.Borrowing;
    public decimal? Amount { get; set; }          // số tiền vay / nợ
    public int? TermMonths { get; set; }          // thời hạn (tháng)
    public string? BankName { get; set; }         // ngân hàng cho vay (rỗng khi nợ công ty)
    public decimal? InterestRate { get; set; }    // lãi suất %/năm (tùy chọn)
    public DateOnly? DisbursedDate { get; set; }  // ngày giải ngân (nếu đã giải ngân)
    public decimal? MonthlyDeductionAmount { get; set; } // số tiền trừ dần vào lương mỗi kỳ
    public DateOnly? DeductionStartDate { get; set; }    // ngày bắt đầu trừ lương
    public string? Note { get; set; }             // cam kết riêng (dùng cho nợ công ty)
    public Guid CreatedBy { get; set; }
}
