using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Enums;
using Polymind.Domain.Finance;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Auditing;

namespace Polymind.Web.Finance;

public sealed record ArchiveResult(bool Succeeded, string? Error, int Affected)
{
    public static ArchiveResult Failed(string error) => new(false, error, 0);
    public static ArchiveResult Success(int affected) => new(true, null, affected);
}

/// <summary>
/// Kho lưu trữ tài chính — CHỈ ẨN khỏi bảng làm việc, không xóa dữ liệu và không miễn nợ.
/// Hai kho ĐỘC LẬP nhau:
///   • Khoản thu (lịch 4 bước của một ứng viên) — ẩn khỏi Tiến độ đóng tiền + Khoản thu cùng lúc,
///     vì hai tab đó đọc chung một tập Payment.
///   • Phiếu thu/chi — chứng từ có vòng đời riêng, lưu trữ riêng; lưu trữ khoản thu KHÔNG kéo theo phiếu.
/// Mọi thao tác đều khôi phục được và đều ghi audit.
/// </summary>
public static class FinanceArchiveService
{
    /// <summary>
    /// Lưu trữ toàn bộ lịch 4 bước của một ứng viên. Chốt chặn ở server: chỉ khi ĐÃ THU ĐỦ 100%.
    /// Không dùng được như đường tất toán sớm — còn thiếu 1 bước là từ chối.
    /// </summary>
    public static async Task<ArchiveResult> ArchiveScheduleAsync(
        ApplicationDbContext db, Guid candidateId, Guid actorId)
    {
        var payments = await db.Payments
            .Where(p => p.CandidateId == candidateId && p.Stage != null && p.ArchivedAt == null)
            .ToListAsync();

        if (payments.Count == 0)
            return ArchiveResult.Failed("Ứng viên chưa có lịch đóng tiền để lưu trữ.");

        var stages = payments.Select(p => (Stage: p.Stage!.Value, p.Status));
        if (!PaymentPostingRules.CanArchiveSchedule(stages))
            return ArchiveResult.Failed(
                $"Chỉ lưu trữ được khi đã thu đủ {PaymentPostingRules.TotalStages}/{PaymentPostingRules.TotalStages} bước. "
                + "Ứng viên còn khoản chưa thu — lưu trữ không phải là cách tất toán.");

        var now = DateTimeOffset.UtcNow;
        foreach (var p in payments)
        {
            p.ArchivedAt = now;
            p.ArchivedBy = actorId;
            p.UpdatedAt = now;
        }
        db.AddAudit(actorId, "archive", "payments", candidateId, null,
            new { CandidateId = candidateId, Count = payments.Count });
        await db.SaveChangesAsync();

        return ArchiveResult.Success(payments.Count);
    }

    public static async Task<ArchiveResult> RestoreScheduleAsync(
        ApplicationDbContext db, Guid candidateId, Guid actorId)
    {
        var payments = await db.Payments
            .Where(p => p.CandidateId == candidateId && p.Stage != null && p.ArchivedAt != null)
            .ToListAsync();

        if (payments.Count == 0)
            return ArchiveResult.Failed("Không có khoản thu nào trong kho lưu trữ của ứng viên này.");

        foreach (var p in payments)
        {
            p.ArchivedAt = null;
            p.ArchivedBy = null;
            p.UpdatedAt = DateTimeOffset.UtcNow;
        }
        db.AddAudit(actorId, "restore", "payments", candidateId, null,
            new { CandidateId = candidateId, Count = payments.Count });
        await db.SaveChangesAsync();

        return ArchiveResult.Success(payments.Count);
    }

    /// <summary>Lưu trữ một phiếu thu/chi — độc lập hoàn toàn với kho lưu trữ khoản thu.</summary>
    public static async Task<ArchiveResult> ArchiveReceiptsAsync(
        ApplicationDbContext db, IReadOnlyCollection<Guid> receiptIds, Guid actorId)
        => await SetReceiptArchivedAsync(db, receiptIds, actorId, archived: true);

    public static async Task<ArchiveResult> RestoreReceiptsAsync(
        ApplicationDbContext db, IReadOnlyCollection<Guid> receiptIds, Guid actorId)
        => await SetReceiptArchivedAsync(db, receiptIds, actorId, archived: false);

    private static async Task<ArchiveResult> SetReceiptArchivedAsync(
        ApplicationDbContext db, IReadOnlyCollection<Guid> receiptIds, Guid actorId, bool archived)
    {
        var receipts = await db.Receipts
            .Where(r => receiptIds.Contains(r.Id) && (r.ArchivedAt != null) != archived)
            .ToListAsync();

        if (receipts.Count == 0)
            return ArchiveResult.Failed("Không có phiếu nào cần xử lý.");

        var now = DateTimeOffset.UtcNow;
        foreach (var r in receipts)
        {
            r.ArchivedAt = archived ? now : null;
            r.ArchivedBy = archived ? actorId : null;
            r.UpdatedAt = now;
        }
        db.AddAudit(actorId, archived ? "archive" : "restore", "receipts", receipts[0].Id, null,
            new { Count = receipts.Count, Ids = receipts.Select(r => r.Id).ToArray() });
        await db.SaveChangesAsync();

        return ArchiveResult.Success(receipts.Count);
    }
}
