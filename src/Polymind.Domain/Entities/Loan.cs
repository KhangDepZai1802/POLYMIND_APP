using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Khoản hỗ trợ vay vốn của ứng viên (module Hỗ trợ vay). Mỗi ứng viên 1 hồ sơ vay.</summary>
public class Loan : BaseEntity
{
    public string Code { get; set; } = default!; // VAY-XXXX
    public Guid CandidateId { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Borrowing;
    public decimal? Amount { get; set; }          // số tiền vay
    public int? TermMonths { get; set; }          // thời hạn vay (tháng)
    public string? BankName { get; set; }         // ngân hàng cho vay
    public decimal? InterestRate { get; set; }    // lãi suất %/năm (tùy chọn)
    public DateOnly? DisbursedDate { get; set; }  // ngày giải ngân (nếu đã giải ngân)
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
}
