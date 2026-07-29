using Polymind.Domain.Entities;
using Polymind.Domain.Finance;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M10 — Luồng duyệt khoản chi (U-M10-1 / RB-7). Chốt bất biến: tiền không ra khỏi công ty
/// bằng một thao tác duy nhất — khoản chi phải được duyệt trước khi xuất phiếu chi.
/// LƯU Ý PHẠM VI: kiểm quyền `expenses:approve` và tái kiểm từ DB nằm ở `Finance.razor`
/// (`Polymind.Web`) → không unit-test được ở đây; xem 05-automation-report.md.
/// </summary>
public class M10_ExpenseApprovalRulesTests
{
    private static Expense NewExpense() => new()
    {
        Code = "EX-20260721-0001",
        Amount = 5_000_000m,
        ExpenseDate = new DateOnly(2026, 7, 21),
        CreatedBy = Guid.NewGuid(),
    };

    [Fact] // Khoản chi mới luôn ở trạng thái CHỜ DUYỆT
    public void New_expense_starts_unapproved()
    {
        var e = NewExpense();

        Assert.Null(e.ApprovedBy);
        Assert.False(ExpenseApprovalRules.IsApproved(e));
        Assert.True(ExpenseApprovalRules.CanApprove(e));
    }

    [Fact] // BẤT BIẾN RB-7 — chưa duyệt thì KHÔNG được xuất phiếu chi
    public void Unapproved_expense_cannot_create_receipt()
        => Assert.False(ExpenseApprovalRules.CanCreateReceipt(NewExpense()));

    [Fact] // Duyệt xong mới mở khóa phiếu chi, và ghi đúng người duyệt
    public void Approve_records_approver_and_unlocks_receipt()
    {
        var e = NewExpense();
        var approverId = Guid.NewGuid();

        var approved = ExpenseApprovalRules.Approve(e, approverId);

        Assert.True(approved);
        Assert.Equal(approverId, e.ApprovedBy);
        Assert.True(ExpenseApprovalRules.IsApproved(e));
        Assert.True(ExpenseApprovalRules.CanCreateReceipt(e));
    }

    [Fact] // Duyệt lần hai bị chặn — không ghi đè người duyệt gốc trong lịch sử
    public void Second_approval_is_rejected_and_keeps_original_approver()
    {
        var e = NewExpense();
        var firstApprover = Guid.NewGuid();
        ExpenseApprovalRules.Approve(e, firstApprover);

        var secondApproved = ExpenseApprovalRules.Approve(e, Guid.NewGuid());

        Assert.False(secondApproved);
        Assert.Equal(firstApprover, e.ApprovedBy);
        Assert.False(ExpenseApprovalRules.CanApprove(e));
    }

    [Fact] // Duyệt cập nhật UpdatedAt để lần sửa cuối phản ánh đúng thời điểm duyệt
    public void Approve_touches_updated_at()
    {
        var e = NewExpense();
        e.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-3);
        var before = e.UpdatedAt;

        ExpenseApprovalRules.Approve(e, Guid.NewGuid());

        Assert.True(e.UpdatedAt > before);
    }
}
