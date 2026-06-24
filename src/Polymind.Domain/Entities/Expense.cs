using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Khoản chi (mục 7.2).</summary>
public class Expense : BaseEntity
{
    public string Code { get; set; } = default!; // EX-YYYYMMDD-XXXX
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public Guid? ReceiptId { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
}
