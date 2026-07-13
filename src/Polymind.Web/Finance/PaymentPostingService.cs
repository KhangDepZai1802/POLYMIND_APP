using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Domain.Finance;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Auditing;
using Polymind.Web.Commissions;
using Polymind.Web.Display;

namespace Polymind.Web.Finance;

public sealed record PaymentPostingResult(bool Succeeded, string? Error, int NewCommissions, string? ReceiptCode = null)
{
    public static PaymentPostingResult Failed(string error) => new(false, error, 0);
    public static PaymentPostingResult Success(int newCommissions, string? receiptCode = null)
        => new(true, null, newCommissions, receiptCode);
}

/// <summary>
/// Hai thao tác TÁCH RỜI trên một khoản thu:
///   1) <see cref="MarkSubmittedAsync"/> — Tiến độ đóng tiền: xác nhận ứng viên ĐÃ NỘP tiền (Submitted).
///      Không hạch toán, không sinh hoa hồng, không sinh phiếu.
///   2) <see cref="MarkPaidAsync"/> — Khoản thu: kế toán DUYỆT (Paid) → sinh phiếu thu + kích hoạt hoa hồng.
/// Tách như vậy để bên Tiến độ không tự động duyệt thay kế toán. Caller phải re-check quyền trước khi gọi.
/// </summary>
public static class PaymentPostingService
{
    /// <summary>
    /// Tiến độ đóng tiền: ứng viên đã nộp tiền bước này. Chỉ ép NỘP đúng thứ tự 1→4 (không đòi kế toán
    /// duyệt xong bước trước), và KHÔNG chuyển sang Paid — việc đó dành riêng cho kế toán bên Khoản thu.
    /// </summary>
    public static async Task<PaymentPostingResult> MarkSubmittedAsync(
        ApplicationDbContext db,
        Payment payment,
        Guid actorId)
    {
        if (payment.Status is PaymentStatus.Submitted or PaymentStatus.Paid)
            return PaymentPostingResult.Success(0);

        if (payment.Stage is PaymentStage stage)
        {
            var blocking = PaymentPostingRules.UnsubmittedEarlierStages(stage, await SiblingsAsync(db, payment));
            if (blocking.Count > 0)
                return PaymentPostingResult.Failed(OutOfOrderSubmitMessage(stage, blocking));
        }

        var oldValue = Snapshot(payment);
        payment.Status = PaymentStatus.Submitted;
        payment.PaidDate ??= DateOnly.FromDateTime(DateTime.UtcNow);
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        db.AddAudit(actorId, "submit", "payments", payment.Id, oldValue, Snapshot(payment));
        await db.SaveChangesAsync();

        return PaymentPostingResult.Success(0);
    }

    public static async Task<PaymentPostingResult> MarkPaidAsync(
        ApplicationDbContext db,
        Payment payment,
        Guid actorId,
        string auditAction = "approve",
        object? oldValue = null)
    {
        if (payment.Status == PaymentStatus.Paid)
            return PaymentPostingResult.Success(0);

        if (payment.Stage is PaymentStage stage)
        {
            var blocking = PaymentPostingRules.UnpaidEarlierStages(stage, await SiblingsAsync(db, payment));
            if (blocking.Count > 0)
                return PaymentPostingResult.Failed(OutOfOrderMessage(stage, blocking));
        }

        oldValue ??= Snapshot(payment);
        payment.ApprovedBy = actorId;
        payment.Status = PaymentStatus.Paid;
        payment.PaidDate ??= DateOnly.FromDateTime(DateTime.UtcNow);
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        db.AddAudit(actorId, auditAction, "payments", payment.Id, oldValue, Snapshot(payment));

        var receipt = await EnsureReceiptAsync(db, payment, actorId);
        await db.SaveChangesAsync();

        var newCommissions = payment.Stage is null
            ? 0
            : await CommissionEngine.EnsureAsync(db, payment.CandidateId, actorId);
        return PaymentPostingResult.Success(newCommissions, receipt?.Code);
    }

    /// <summary>
    /// Duyệt xong là có ngay chứng từ để in — kế toán không phải bấm thêm một nút "Phiếu thu" nữa.
    /// Idempotent: khoản thu đã có phiếu thì không tạo trùng.
    /// </summary>
    private static async Task<Receipt?> EnsureReceiptAsync(ApplicationDbContext db, Payment payment, Guid actorId)
    {
        if (await db.Receipts.AnyAsync(r => r.PaymentId == payment.Id))
            return null;

        var receipt = new Receipt
        {
            Code = $"RC-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            ReceiptType = ReceiptType.Income,
            CandidateId = payment.CandidateId,
            PaymentId = payment.Id,
            Amount = payment.Amount,
            Description = DescribeIncomeReceipt(payment),
            ReceiptDate = payment.PaidDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedBy = actorId,
        };
        db.Receipts.Add(receipt);
        payment.ReceiptId = receipt.Id;
        db.AddAudit(actorId, "create", "receipts", receipt.Id, null,
            new { receipt.Code, receipt.Amount, receipt.PaymentId });

        return receipt;
    }

    /// <summary>Diễn giải phiếu thu — dùng chung cho lúc duyệt và lúc lập bù (<see cref="ReceiptBackfillService"/>).</summary>
    public static string DescribeIncomeReceipt(Payment payment) => payment.Stage is PaymentStage st
        ? $"Phiếu thu {payment.Code} — {PaymentSchedule.StepLabel(st)}"
        : $"Phiếu thu {payment.Code} ({Labels.Vi(payment.PaymentType)})";

    private static async Task<List<(PaymentStage Stage, PaymentStatus Status)>> SiblingsAsync(
        ApplicationDbContext db, Payment payment)
    {
        var rows = await db.Payments.AsNoTracking()
            .Where(x => x.CandidateId == payment.CandidateId && x.Stage != null)
            .Select(x => new { Stage = x.Stage!.Value, x.Status })
            .ToListAsync();

        return rows.Select(x => (x.Stage, x.Status)).ToList();
    }

    private static string OutOfOrderSubmitMessage(PaymentStage current, IReadOnlyList<PaymentStage> blocking)
    {
        var missing = string.Join(", ", blocking.Select(PaymentSchedule.StepLabel));
        var plural = blocking.Count > 1 ? "các bước" : "bước";

        return $"Chưa xác nhận được {PaymentSchedule.StepLabel(current)} vì ứng viên chưa nộp {plural}: {missing}. "
             + $"Phải nộp theo đúng thứ tự: {PaymentSchedule.OrderHint()}.";
    }

    /// <summary>
    /// Thông báo khi kế toán duyệt sai thứ tự: nêu ĐÍCH DANH bước đang duyệt và bước còn thiếu,
    /// thay vì chỉ nói chung chung "phải đóng theo thứ tự 1 → 4".
    /// </summary>
    private static string OutOfOrderMessage(PaymentStage current, IReadOnlyList<PaymentStage> blocking)
    {
        var missing = string.Join(", ", blocking.Select(PaymentSchedule.StepLabel));
        var plural = blocking.Count > 1 ? "các bước" : "bước";

        return $"Chưa duyệt được {PaymentSchedule.StepLabel(current)} vì {plural} trước đó chưa thu: {missing}. "
             + $"Lịch đóng tiền phải theo đúng thứ tự: {PaymentSchedule.OrderHint()}.";
    }

    private static object Snapshot(Payment p) => new
    {
        p.Code,
        p.CandidateId,
        p.JobOrderId,
        p.PaymentType,
        p.Stage,
        p.Amount,
        p.DueDate,
        p.PaidDate,
        p.Status,
        p.PaymentMethod,
        p.Notes,
        p.CreatedBy,
        p.ApprovedBy,
    };
}
