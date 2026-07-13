using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Domain.Loans;
using Polymind.Infrastructure.Persistence.Migrations;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M11 — Loans & Debt Collection. Pin hợp đồng Domain mà module phụ thuộc.
/// TC_M11_037..047 + regression BUG_M11_01 / CR-M11-1/2/3.
/// </summary>
public class M11_LoanRulesTests
{
    [Fact] // TC_M11_037 — Settled là mốc tất toán, điều kiện mở gate B20 "Hoàn thành quy trình"
    public void LoanStatus_settled_is_the_terminal_settlement_state()
    {
        var all = Enum.GetValues<LoanStatus>();

        Assert.Contains(LoanStatus.Settled, all);
        Assert.Contains(LoanStatus.Borrowing, all);
        Assert.Contains(LoanStatus.Disbursed, all);
        // NotBorrowed là legacy (không dùng trên UI) nhưng vẫn phải tồn tại để tương thích dữ liệu cũ.
        Assert.Contains(LoanStatus.NotBorrowed, all);
    }

    [Fact] // TC_M11_038 — chỉ hai loại nợ: vay ngân hàng vs nợ công ty
    public void LoanKind_distinguishes_bank_and_company()
    {
        Assert.Equal(new[] { LoanKind.Bank, LoanKind.Company }, Enum.GetValues<LoanKind>());
    }

    [Fact] // TC_M11_039 — kỳ trả góp có đủ trạng thái vòng đời
    public void LoanRepaymentStatus_contains_lifecycle_states()
    {
        var all = Enum.GetValues<LoanRepaymentStatus>();

        Assert.Contains(LoanRepaymentStatus.Pending, all);
        Assert.Contains(LoanRepaymentStatus.Partial, all);
        Assert.Contains(LoanRepaymentStatus.Paid, all);
        Assert.Contains(LoanRepaymentStatus.Overdue, all);
    }

    [Fact] // TC_M11_040 — hồ sơ vay mới mặc định vay ngân hàng, đang vay
    public void New_loan_defaults_to_bank_and_borrowing()
    {
        var loan = new Loan();

        Assert.Equal(LoanKind.Bank, loan.Kind);
        Assert.Equal(LoanStatus.Borrowing, loan.Status);
    }

    [Fact] // TC_M11_041 — kỳ trả góp mới mặc định chưa thu (Pending)
    public void New_loan_repayment_defaults_to_pending()
    {
        var repayment = new LoanRepayment();

        Assert.Equal(LoanRepaymentStatus.Pending, repayment.Status);
    }

    [Theory] // BUG_M11_01 — chỉ nợ công ty chưa tất toán mới chặn gate B20
    [InlineData(LoanKind.Bank, LoanStatus.Borrowing, false)]
    [InlineData(LoanKind.Bank, LoanStatus.Disbursed, false)]
    [InlineData(LoanKind.Company, LoanStatus.Borrowing, true)]
    [InlineData(LoanKind.Company, LoanStatus.Settled, false)]
    public void Workflow_gate_only_blocks_unsettled_company_debt(LoanKind kind, LoanStatus status, bool expected)
    {
        Assert.Equal(expected, LoanCollectionRules.BlocksWorkflowCompletion(kind, status));
    }

    [Fact] // CR-M11-3 — cấm tất toán thủ công khi vẫn còn dư nợ
    public void Settled_status_is_rejected_while_company_debt_is_outstanding()
    {
        var error = LoanCollectionRules.ValidateStatusChange(
            LoanKind.Company, LoanStatus.Disbursed, LoanStatus.Settled, 1_000_000m, canCollectDebt: true);

        Assert.NotNull(error);
        Assert.Contains("thu đủ 100%", error);
    }

    [Fact] // BUG_M11_01 — ngân hàng không có trạng thái tất toán tại công ty
    public void Bank_loan_cannot_be_marked_settled()
    {
        var error = LoanCollectionRules.ValidateStatusChange(
            LoanKind.Bank, LoanStatus.Disbursed, LoanStatus.Settled, 0m, canCollectDebt: true);

        Assert.NotNull(error);
    }

    [Fact] // CR-M11-1 — chỉ finance role mới được đổi trạng thái liên quan Settled
    public void Non_finance_actor_cannot_change_settlement_status()
    {
        var error = LoanCollectionRules.ValidateStatusChange(
            LoanKind.Company, LoanStatus.Disbursed, LoanStatus.Settled, 0m, canCollectDebt: false);

        Assert.NotNull(error);
        Assert.Contains("Kế toán", error);
    }

    [Fact] // CR-M11-2/3 — thu một kỳ ghi đúng tiền, chưa tất toán khi vẫn còn kỳ khác
    public void Collecting_one_installment_keeps_loan_open_until_every_installment_is_paid()
    {
        var loan = CompanyLoan();
        var first = Repayment(loan.Id, 1, 600_000m);
        var second = Repayment(loan.Id, 2, 400_000m);

        var result = LoanCollectionRules.Collect(
            loan, [first, second], first.Id, new DateOnly(2026, 7, 11), DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal(600_000m, result.Amount);
        Assert.False(result.Settled);
        Assert.Equal(LoanRepaymentStatus.Paid, first.Status);
        Assert.Equal(LoanStatus.Disbursed, loan.Status);
    }

    [Fact] // CR-M11-3 — Thu hết thu toàn bộ tiền thật còn lại rồi mới auto-settle
    public void Collect_remaining_marks_all_installments_paid_and_settles_loan()
    {
        var loan = CompanyLoan();
        var first = Repayment(loan.Id, 1, 600_000m, paid: 100_000m);
        var second = Repayment(loan.Id, 2, 400_000m);

        var result = LoanCollectionRules.Collect(
            loan, [first, second], installmentId: null, new DateOnly(2026, 7, 11), DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal(900_000m, result.Amount);
        Assert.True(result.Settled);
        Assert.All(new[] { first, second }, r => Assert.Equal(LoanRepaymentStatus.Paid, r.Status));
        Assert.Equal(LoanStatus.Settled, loan.Status);
    }

    [Fact] // CR-M11-3 — khoản nợ không có lịch vẫn phải thu đủ Amount trước khi tất toán
    public void Collect_remaining_without_schedule_collects_full_company_debt()
    {
        var loan = CompanyLoan();
        loan.Amount = 2_500_000m;

        var result = LoanCollectionRules.Collect(
            loan, [], installmentId: null, new DateOnly(2026, 7, 11), DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal(2_500_000m, result.Amount);
        Assert.True(result.Settled);
        Assert.Equal(LoanStatus.Settled, loan.Status);
    }

    [Fact] // CR-M11-2 — schema receipt có migration nguồn Loan/LoanRepayment và unique kỳ thu
    public void Loan_receipt_migration_is_discoverable_and_adds_source_links()
    {
        var migration = new LinkLoanDebtCollectionReceipts
        {
            ActiveProvider = "Npgsql.EntityFrameworkCore.PostgreSQL",
        };
        var migrationId = typeof(LinkLoanDebtCollectionReceipts)
            .GetCustomAttribute<MigrationAttribute>()?.Id;
        var columns = migration.UpOperations.OfType<AddColumnOperation>().Select(x => x.Name).ToArray();
        var indexes = migration.UpOperations.OfType<CreateIndexOperation>().ToArray();

        Assert.Equal("20260711123000_LinkLoanDebtCollectionReceipts", migrationId);
        Assert.Equal(new[] { "loan_id", "loan_repayment_id" }, columns);
        Assert.Contains(indexes, x => x.Name == "ix_receipts_loan_id" && !x.IsUnique);
        Assert.Contains(indexes, x => x.Name == "ix_receipts_loan_repayment_id" && x.IsUnique);
    }

    private static Loan CompanyLoan() => new()
    {
        Id = Guid.NewGuid(),
        Kind = LoanKind.Company,
        Status = LoanStatus.Disbursed,
        Amount = 1_000_000m,
    };

    private static LoanRepayment Repayment(Guid loanId, int no, decimal amount, decimal paid = 0m) => new()
    {
        Id = Guid.NewGuid(),
        LoanId = loanId,
        InstallmentNo = no,
        Amount = amount,
        PaidAmount = paid,
        Status = paid >= amount ? LoanRepaymentStatus.Paid : paid > 0m ? LoanRepaymentStatus.Partial : LoanRepaymentStatus.Pending,
    };
}
