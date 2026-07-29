using Polymind.Domain.Entities;

namespace Polymind.Domain.Finance;

/// <summary>
/// Luật duyệt khoản chi (RB-7). Khoản chi vừa tạo ở trạng thái CHỜ DUYỆT
/// (<see cref="Expense.ApprovedBy"/> = null); phải có người đủ quyền duyệt thì
/// mới được xuất phiếu chi — tiền không ra khỏi công ty bằng một thao tác duy nhất.
/// </summary>
public static class ExpenseApprovalRules
{
    /// <summary>Khoản chi đã được duyệt chưa.</summary>
    public static bool IsApproved(Expense expense) => expense.ApprovedBy is not null;

    /// <summary>
    /// Chỉ duyệt được khoản chi CHƯA duyệt. Duyệt lại lần hai bị chặn để không ghi đè
    /// người duyệt gốc trong lịch sử.
    /// </summary>
    public static bool CanApprove(Expense expense) => !IsApproved(expense);

    /// <summary>
    /// Chỉ xuất phiếu chi khi khoản chi đã được duyệt — đây là điểm chặn thật sự của luồng RB-7.
    /// </summary>
    public static bool CanCreateReceipt(Expense expense) => IsApproved(expense);

    /// <summary>
    /// Ghi nhận người duyệt. Trả về false nếu khoản chi đã duyệt trước đó (không đổi gì).
    /// </summary>
    public static bool Approve(Expense expense, Guid approverId)
    {
        if (!CanApprove(expense)) return false;

        expense.ApprovedBy = approverId;
        expense.UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }
}
