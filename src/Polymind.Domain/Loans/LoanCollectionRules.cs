using Polymind.Domain.Entities;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Loans;

/// <summary>Luật thu nợ công ty và tất toán, dùng chung cho UI và regression test.</summary>
public static class LoanCollectionRules
{
    public sealed record CollectionResult(bool Succeeded, string? Error, decimal Amount, bool Settled)
    {
        public static CollectionResult Fail(string error) => new(false, error, 0m, false);
    }

    public static bool BlocksWorkflowCompletion(LoanKind kind, LoanStatus status)
        => kind == LoanKind.Company && status != LoanStatus.Settled;

    public static decimal Outstanding(decimal? fallbackAmount, IEnumerable<LoanRepayment> repayments)
    {
        var rows = repayments.ToList();
        return rows.Count == 0
            ? Math.Max(0m, fallbackAmount ?? 0m)
            : rows.Sum(r => Math.Max(0m, r.Amount - r.PaidAmount));
    }

    public static string? ValidateStatusChange(
        LoanKind kind,
        LoanStatus current,
        LoanStatus requested,
        decimal outstanding,
        bool canCollectDebt)
    {
        if (kind == LoanKind.Bank && requested == LoanStatus.Settled)
            return "Vay ngân hàng không theo dõi trạng thái tất toán tại công ty.";

        if (current != requested
            && (current == LoanStatus.Settled || requested == LoanStatus.Settled)
            && !canCollectDebt)
            return "Chỉ Kế toán hoặc Super Admin được thay đổi trạng thái tất toán.";

        if (requested == LoanStatus.Settled && outstanding > 0m)
            return $"Khoản nợ còn {outstanding:N0} đ chưa thu; phải thu đủ 100% trước khi tất toán.";

        return null;
    }

    public static CollectionResult Collect(
        Loan loan,
        IReadOnlyCollection<LoanRepayment> repayments,
        Guid? installmentId,
        DateOnly paidDate,
        DateTimeOffset now)
    {
        if (loan.Kind != LoanKind.Company)
            return CollectionResult.Fail("Chỉ được thu nợ đối với khoản nợ công ty.");

        List<LoanRepayment> targets;
        if (installmentId is Guid id)
        {
            var target = repayments.FirstOrDefault(r => r.Id == id && r.LoanId == loan.Id);
            if (target is null)
                return CollectionResult.Fail("Không tìm thấy kỳ trả nợ.");
            if (target.PaidAmount >= target.Amount || target.Status == LoanRepaymentStatus.Paid)
                return CollectionResult.Fail("Kỳ trả nợ này đã được thu.");
            targets = [target];
        }
        else
        {
            targets = repayments
                .Where(r => r.PaidAmount < r.Amount || r.Status != LoanRepaymentStatus.Paid)
                .OrderBy(r => r.InstallmentNo)
                .ToList();
        }

        var collected = targets.Sum(r => Math.Max(0m, r.Amount - r.PaidAmount));
        if (targets.Count == 0)
        {
            if (repayments.Count > 0)
                return CollectionResult.Fail("Khoản nợ đã được thu đủ.");
            collected = Math.Max(0m, loan.Amount ?? 0m);
        }

        if (collected <= 0m)
            return CollectionResult.Fail("Khoản nợ không còn số tiền phải thu.");

        foreach (var target in targets)
        {
            target.PaidAmount = target.Amount;
            target.PaidDate = paidDate;
            target.Status = LoanRepaymentStatus.Paid;
            target.UpdatedAt = now;
        }

        var outstanding = Outstanding(loan.Amount, repayments);
        var settled = repayments.Count == 0 || outstanding <= 0m;
        if (settled)
            loan.Status = LoanStatus.Settled;
        else if (loan.Status == LoanStatus.Settled)
            loan.Status = LoanStatus.Disbursed;
        loan.UpdatedAt = now;

        return new CollectionResult(true, null, collected, settled);
    }
}
