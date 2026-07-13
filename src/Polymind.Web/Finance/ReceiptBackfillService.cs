using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;

namespace Polymind.Web.Finance;

/// <summary>
/// Sổ và chứng từ phải khớp nhau: khoản thu ĐÃ DUYỆT (Paid) thì bắt buộc có phiếu thu để in.
/// Dữ liệu cũ vi phạm điều đó — seed demo gán thẳng Status = Paid mà không lập phiếu, và các khoản
/// duyệt từ trước khi <see cref="PaymentPostingService"/> tự sinh phiếu cũng chỉ có Payment — nên tab
/// "Khoản thu" đầy ứng viên đã duyệt trong khi tab "Phiếu thu/chi" gần như trống.
/// Vá lúc khởi động; idempotent (chỉ đụng khoản thu CHƯA có phiếu) nên chạy lại vô hại.
/// </summary>
public static class ReceiptBackfillService
{
    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ReceiptBackfill");

        var linkedPaymentIds = (await db.Receipts
            .Where(r => r.PaymentId != null)
            .Select(r => r.PaymentId!.Value)
            .ToListAsync()).ToHashSet();

        var paid = await db.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .OrderBy(p => p.PaidDate)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();

        var missing = paid.Where(p => !linkedPaymentIds.Contains(p.Id) && p.ReceiptId is null).ToList();
        if (missing.Count == 0) return;

        var fallbackActor = await db.Users.Select(u => u.Id).FirstOrDefaultAsync();
        var usedCodes = (await db.Receipts.Select(r => r.Code).ToListAsync()).ToHashSet();

        foreach (var p in missing)
        {
            var date = p.PaidDate ?? DateOnly.FromDateTime(p.CreatedAt.UtcDateTime);
            var actor = p.ApprovedBy ?? (p.CreatedBy == Guid.Empty ? fallbackActor : p.CreatedBy);
            var receipt = new Receipt
            {
                Code = NextCode(usedCodes, date),
                ReceiptType = ReceiptType.Income,
                CandidateId = p.CandidateId,
                PaymentId = p.Id,
                Amount = p.Amount,
                Description = PaymentPostingService.DescribeIncomeReceipt(p),
                ReceiptDate = date,
                CreatedBy = actor,
            };
            db.Receipts.Add(receipt);
            p.ReceiptId = receipt.Id;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Đã lập bù {Count} phiếu thu cho khoản thu đã duyệt nhưng thiếu chứng từ.", missing.Count);
    }

    /// <summary>Mã phiếu là unique index — sinh nhiều phiếu một lượt nên phải tự tránh trùng, không bốc số ngẫu nhiên.</summary>
    private static string NextCode(HashSet<string> used, DateOnly date)
    {
        for (var n = 1000; n <= 9999; n++)
        {
            var code = $"RC-{date:yyyyMMdd}-{n}";
            if (used.Add(code)) return code;
        }

        string fallback;
        do
        {
            fallback = $"RC-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        } while (!used.Add(fallback));

        return fallback;
    }
}
