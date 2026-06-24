using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;

namespace Polymind.Web.Notifications;

/// <summary>
/// Nền thông báo (stub Module 8 / mục 13). Sinh reminder nội bộ (in-app) cho người dùng:
/// khoản thu quá hạn/sắp tới, lịch visa, lịch xuất cảnh, hồ sơ còn thiếu.
/// Chưa gửi Email/SMS/Zalo thật — chỉ tạo bản ghi <see cref="Notification"/> kênh InApp.
/// Sinh idempotent: mỗi (UserId, Type, ReferenceId) chỉ tạo 1 lần.
/// </summary>
public class NotificationService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    /// <summary>Ngưỡng nhắc trước (ngày) cho các mốc sắp tới.</summary>
    private const int LookAheadDays = 7;

    /// <summary>Quét dữ liệu nghiệp vụ, tạo các reminder còn thiếu cho user. Trả về số bản ghi mới tạo.</summary>
    public async Task<int> GenerateRemindersAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var horizon = today.AddDays(LookAheadDays);

        // Đã có reminder nào cho user (theo Type + ReferenceId) để tránh trùng.
        var existing = await db.Notifications
            .Where(n => n.UserId == userId && n.ReferenceId != null)
            .Select(n => new { n.Type, n.ReferenceId })
            .ToListAsync();
        var seen = existing.Select(x => (x.Type, x.ReferenceId!.Value)).ToHashSet();

        var candidateNames = await db.Candidates.ToDictionaryAsync(c => c.Id, c => c.FullName);
        var toAdd = new List<Notification>();

        void Add(NotificationType type, Guid refId, string refType, string title, string body)
        {
            if (!seen.Add((type, refId))) return;
            toAdd.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Channel = NotificationChannel.InApp,
                Title = title,
                Body = body,
                ReferenceType = refType,
                ReferenceId = refId,
                IsRead = false,
                SentAt = DateTimeOffset.UtcNow,
            });
        }

        string Name(Guid candidateId) => candidateNames.GetValueOrDefault(candidateId, "Ứng viên");

        // --- Khoản thu quá hạn / sắp đến hạn ---
        var duePayments = await db.Payments
            .Where(p => p.Status != PaymentStatus.Paid && p.Status != PaymentStatus.Refunded
                        && p.DueDate != null && p.DueDate <= horizon)
            .Select(p => new { p.Id, p.CandidateId, p.Amount, p.DueDate, p.Status })
            .ToListAsync();
        foreach (var p in duePayments)
        {
            var overdue = p.DueDate < today || p.Status == PaymentStatus.Overdue;
            var label = overdue ? "Khoản thu quá hạn" : "Khoản thu sắp đến hạn";
            Add(NotificationType.ReminderPayment, p.Id, "payment", $"{label}: {Name(p.CandidateId)}",
                $"{p.Amount:N0} đ — hạn {p.DueDate:dd/MM/yyyy}.");
        }

        // --- Lịch visa sắp tới (phỏng vấn hoặc có kết quả) ---
        var visas = await db.Visas
            .Where(v => v.Status != VisaStatus.Approved && v.Status != VisaStatus.Rejected
                        && ((v.InterviewDate != null && v.InterviewDate >= today && v.InterviewDate <= horizon)
                            || (v.ResultDate != null && v.ResultDate >= today && v.ResultDate <= horizon)))
            .Select(v => new { v.Id, v.CandidateId, v.InterviewDate, v.ResultDate, v.Country })
            .ToListAsync();
        foreach (var v in visas)
        {
            var date = v.InterviewDate ?? v.ResultDate;
            var what = v.InterviewDate != null ? "Phỏng vấn visa" : "Có kết quả visa";
            Add(NotificationType.ReminderVisa, v.Id, "visa", $"{what}: {Name(v.CandidateId)}",
                $"{v.Country} — ngày {date:dd/MM/yyyy}.");
        }

        // --- Lịch xuất cảnh sắp tới ---
        var flights = await db.Flights
            .Where(f => f.ActualDepartureAt == null && f.DepartureDate != null
                        && f.DepartureDate >= today && f.DepartureDate <= horizon)
            .Select(f => new { f.Id, f.CandidateId, f.DepartureDate, f.Airline, f.DestinationCountry })
            .ToListAsync();
        foreach (var f in flights)
        {
            Add(NotificationType.ReminderDeparture, f.Id, "flight", $"Sắp xuất cảnh: {Name(f.CandidateId)}",
                $"{f.Airline} → {f.DestinationCountry} — bay {f.DepartureDate:dd/MM/yyyy}.");
        }

        // --- Hồ sơ còn thiếu: ứng viên đã tới bước Hoàn thiện hồ sơ nhưng chưa có tài liệu nào ---
        var docCandidateIds = (await db.CandidateDocuments.Select(d => d.CandidateId).Distinct().ToListAsync())
            .ToHashSet();
        var needDocs = await db.CandidateJobOrders
            .Where(cjo => cjo.Status == CandidateJobOrderStatus.Active && cjo.CurrentStep >= WorkflowStep.Document)
            .Select(cjo => cjo.CandidateId)
            .Distinct()
            .ToListAsync();
        foreach (var cid in needDocs.Where(cid => !docCandidateIds.Contains(cid)))
        {
            Add(NotificationType.ReminderDocument, cid, "candidate", $"Thiếu hồ sơ: {Name(cid)}",
                "Ứng viên đã tới bước hoàn thiện hồ sơ nhưng chưa có tài liệu nào được tải lên.");
        }

        if (toAdd.Count > 0)
        {
            db.Notifications.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
        return toAdd.Count;
    }

    public async Task<List<Notification>> GetForUserAsync(Guid userId, int take = 100)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.IsRead ? 0 : 1)
            .ThenByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkReadAsync(Guid notificationId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId);
        if (n is null || n.IsRead) return;
        n.IsRead = true;
        n.ReadAt = DateTimeOffset.UtcNow;
        n.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var unread = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        var now = DateTimeOffset.UtcNow;
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = now; n.UpdatedAt = now; }
        if (unread.Count > 0) await db.SaveChangesAsync();
    }
}
