using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;
using Polymind.Web.Display;

namespace Polymind.Web.Notifications;

/// <summary>
/// Sinh reminder theo người phụ trách, lưu theo kênh nhận, rồi dispatcher nền đánh dấu/gửi qua sender tương ứng.
/// </summary>
public class NotificationService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    UserManager<ApplicationUser> userManager,
    IEnumerable<INotificationSender> senders,
    ILogger<NotificationService> logger)
{
    private const int LookAheadDays = 7;

    public async Task<int> GenerateRemindersAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var user = await userManager.FindByIdAsync(userId.ToString());
        var roles = user is null ? Array.Empty<string>() : await userManager.GetRolesAsync(user);
        var canSeeAll = roles.Contains(RoleNames.SuperAdmin) || roles.Contains(RoleNames.Director);

        var events = await BuildReminderEventsAsync(db);
        if (canSeeAll)
            return await PersistEventsAsync(db, events, currentUserOnly: null);

        var scoped = events.Where(e => e.Recipients.Contains(userId)).ToList();
        return await PersistEventsAsync(db, scoped, userId);
    }

    public async Task<int> GenerateRemindersForAllUsersAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var events = await BuildReminderEventsAsync(db);
        return await PersistEventsAsync(db, events, currentUserOnly: null);
    }

    public async Task<int> SendPendingAsync(int take = 200, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var pending = await db.Notifications
            .Where(n => n.SentAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
        if (pending.Count == 0) return 0;

        var byChannel = senders.ToDictionary(s => s.Channel);
        var sent = 0;
        foreach (var notification in pending)
        {
            if (!byChannel.TryGetValue(notification.Channel, out var sender))
            {
                logger.LogWarning("Không có sender cho kênh {Channel}", notification.Channel);
                continue;
            }

            try
            {
                var result = await sender.SendAsync(notification, cancellationToken);
                if (!result.Success)
                {
                    logger.LogWarning("Gửi thông báo {NotificationId} thất bại: {Message}", notification.Id, result.Message);
                    continue;
                }

                notification.SentAt = DateTimeOffset.UtcNow;
                notification.UpdatedAt = DateTimeOffset.UtcNow;
                sent++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Gửi thông báo {NotificationId} lỗi", notification.Id);
            }
        }

        if (sent > 0) await db.SaveChangesAsync(cancellationToken);
        return sent;
    }

    public async Task<List<NotificationPreference>> GetPreferencesAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.NotificationPreferences.Where(p => p.UserId == userId).ToListAsync();
        var byType = existing.ToDictionary(p => p.Type);
        var result = new List<NotificationPreference>();
        foreach (var type in Enum.GetValues<NotificationType>())
        {
            if (byType.TryGetValue(type, out var pref))
            {
                result.Add(pref);
                continue;
            }

            result.Add(new NotificationPreference
            {
                UserId = userId,
                Type = type,
                InAppEnabled = true,
            });
        }
        return result.OrderBy(p => p.Type).ToList();
    }

    public async Task SavePreferencesAsync(Guid userId, IReadOnlyCollection<NotificationPreference> preferences)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.NotificationPreferences.Where(p => p.UserId == userId).ToListAsync();
        var byType = existing.ToDictionary(p => p.Type);
        foreach (var incoming in preferences)
        {
            if (!byType.TryGetValue(incoming.Type, out var pref))
            {
                pref = new NotificationPreference { UserId = userId, Type = incoming.Type };
                db.NotificationPreferences.Add(pref);
            }

            pref.InAppEnabled = incoming.InAppEnabled;
            pref.EmailEnabled = incoming.EmailEnabled;
            pref.SmsEnabled = incoming.SmsEnabled;
            pref.ZaloEnabled = incoming.ZaloEnabled;
            pref.UpdatedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetForUserAsync(Guid userId, int take = 100)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Notifications
            .Where(n => n.UserId == userId && n.Channel == NotificationChannel.InApp)
            .OrderByDescending(n => n.IsRead ? 0 : 1)
            .ThenByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Notifications.CountAsync(n =>
            n.UserId == userId && n.Channel == NotificationChannel.InApp && !n.IsRead);
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
        var unread = await db.Notifications
            .Where(n => n.UserId == userId && n.Channel == NotificationChannel.InApp && !n.IsRead)
            .ToListAsync();
        var now = DateTimeOffset.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
            n.UpdatedAt = now;
        }
        if (unread.Count > 0) await db.SaveChangesAsync();
    }

    private async Task<List<ReminderEvent>> BuildReminderEventsAsync(ApplicationDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var horizon = today.AddDays(LookAheadDays);
        var events = new List<ReminderEvent>();

        var candidateNames = await db.Candidates.ToDictionaryAsync(c => c.Id, c => c.FullName);
        string Name(Guid candidateId) => candidateNames.GetValueOrDefault(candidateId, "Ứng viên");

        var roleRecipients = await LoadRoleRecipientsAsync(db);
        var cjoOwners = await db.CandidateJobOrders
            .Where(c => c.AssignedTo != null)
            .Select(c => new { c.CandidateId, c.AssignedTo })
            .ToListAsync();
        var ownerByCandidate = cjoOwners
            .GroupBy(x => x.CandidateId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.AssignedTo!.Value).Distinct().ToList());

        List<Guid> CandidateOwnersOr(Guid candidateId, params string[] fallbackRoles)
            => ownerByCandidate.TryGetValue(candidateId, out var owners) && owners.Count > 0
                ? owners
                : RoleUsers(roleRecipients, fallbackRoles);

        var duePayments = await db.Payments
            .Where(p => p.Status != PaymentStatus.Paid && p.Status != PaymentStatus.Refunded
                        && p.DueDate != null && p.DueDate <= horizon)
            .Select(p => new { p.Id, p.CandidateId, p.Amount, p.DueDate, p.Status })
            .ToListAsync();
        foreach (var p in duePayments)
        {
            var overdue = p.DueDate < today || p.Status == PaymentStatus.Overdue;
            var label = overdue ? "Khoản thu quá hạn" : "Khoản thu sắp đến hạn";
            events.Add(new ReminderEvent(
                NotificationType.ReminderPayment, p.Id, "payment",
                $"{label}: {Name(p.CandidateId)}",
                $"{p.Amount:N0} đ — hạn {p.DueDate:dd/MM/yyyy}.",
                CandidateOwnersOr(p.CandidateId, RoleNames.Accountant, RoleNames.Director)));
        }

        var visas = await db.Visas
            .Where(v => v.Status != VisaStatus.Approved && v.Status != VisaStatus.Rejected
                        && ((v.InterviewDate != null && v.InterviewDate >= today && v.InterviewDate <= horizon)
                            || (v.ResultDate != null && v.ResultDate >= today && v.ResultDate <= horizon)))
            .Select(v => new { v.Id, v.CandidateId, v.InterviewDate, v.ResultDate, v.Country, v.HandledBy })
            .ToListAsync();
        foreach (var v in visas)
        {
            var date = v.InterviewDate ?? v.ResultDate;
            var what = v.InterviewDate != null ? "Phỏng vấn visa" : "Có kết quả visa";
            var recipients = v.HandledBy is not null
                ? new List<Guid> { v.HandledBy.Value }
                : CandidateOwnersOr(v.CandidateId, RoleNames.VisaStaff, RoleNames.Director);
            events.Add(new ReminderEvent(
                NotificationType.ReminderVisa, v.Id, "visa",
                $"{what}: {Name(v.CandidateId)}",
                $"{v.Country} — ngày {date:dd/MM/yyyy}.",
                recipients));
        }

        var flights = await db.Flights
            .Where(f => f.ActualDepartureAt == null && f.DepartureDate != null
                        && f.DepartureDate >= today && f.DepartureDate <= horizon)
            .Select(f => new { f.Id, f.CandidateId, f.DepartureDate, f.Airline, f.DestinationCountry })
            .ToListAsync();
        foreach (var f in flights)
        {
            events.Add(new ReminderEvent(
                NotificationType.ReminderDeparture, f.Id, "flight",
                $"Sắp xuất cảnh: {Name(f.CandidateId)}",
                $"{f.Airline} → {f.DestinationCountry} — bay {f.DepartureDate:dd/MM/yyyy}.",
                CandidateOwnersOr(f.CandidateId, RoleNames.VisaStaff, RoleNames.Director)));
        }

        var horizonStart = DateTimeOffset.UtcNow;
        var horizonEnd = DateTimeOffset.UtcNow.AddDays(LookAheadDays);
        var leadAppointments = await db.Leads
            .Where(l => l.AppointmentAt != null && l.AppointmentAt >= horizonStart && l.AppointmentAt <= horizonEnd
                        && l.Status != LeadStatus.Converted && l.Status != LeadStatus.Cancelled)
            .Select(l => new { l.Id, l.FullName, l.AppointmentAt, l.AssignedTo })
            .ToListAsync();
        foreach (var l in leadAppointments)
        {
            var recipients = l.AssignedTo is not null
                ? new List<Guid> { l.AssignedTo.Value }
                : RoleUsers(roleRecipients, RoleNames.Recruiter, RoleNames.RecruitmentManager);
            events.Add(new ReminderEvent(
                NotificationType.ReminderInterview, l.Id, "lead",
                $"Lịch hẹn tư vấn: {l.FullName}",
                $"Hẹn lúc {l.AppointmentAt!.Value.ToLocalTime():dd/MM/yyyy HH:mm}.",
                recipients));
        }

        // Nhắc chăm sóc lead (góp ý Vietgroup): lead đứng yên một trạng thái quá ngưỡng giờ
        // (LeadCareRules) → nhắc tư vấn viên phụ trách + trưởng phòng tuyển dụng + super admin.
        var now = DateTimeOffset.UtcNow;
        var convertedLeadIds = (await db.Candidates.Where(c => c.LeadId != null)
            .Select(c => c.LeadId!.Value).Distinct().ToListAsync()).ToHashSet();
        var activeLeads = await db.Leads
            .Where(l => l.Status != LeadStatus.Converted
                        && l.Status != LeadStatus.Unsuitable
                        && l.Status != LeadStatus.Cancelled)
            .Select(l => new { l.Id, l.FullName, l.Status, l.CreatedAt, l.AppointmentAt, l.AssignedTo })
            .ToListAsync();
        activeLeads = activeLeads.Where(l => !convertedLeadIds.Contains(l.Id)).ToList();
        if (activeLeads.Count > 0)
        {
            var staleLeadIds = activeLeads.Select(l => l.Id).ToList();
            var lastStatusChanges = await db.LeadActivities
                .Where(a => a.ActivityType == LeadActivityType.StatusChange && staleLeadIds.Contains(a.LeadId))
                .GroupBy(a => a.LeadId)
                .Select(g => new { LeadId = g.Key, At = g.Max(a => a.CreatedAt) })
                .ToDictionaryAsync(x => x.LeadId, x => x.At);
            var overseers = RoleUsers(roleRecipients, RoleNames.RecruitmentManager, RoleNames.SuperAdmin);
            foreach (var l in activeLeads)
            {
                var lastChange = lastStatusChanges.GetValueOrDefault(l.Id, l.CreatedAt);
                if (!LeadCareRules.TryGetOverdue(l.Status, lastChange, l.AppointmentAt, now, out var stalledHours))
                    continue;

                var recipients = (l.AssignedTo is not null
                        ? new List<Guid> { l.AssignedTo.Value }
                        : RoleUsers(roleRecipients, RoleNames.Recruiter, RoleNames.Consultant))
                    .Concat(overseers)
                    .Distinct()
                    .ToList();
                events.Add(new ReminderEvent(
                    NotificationType.ReminderLeadCare, l.Id, "lead",
                    $"Lead cần chăm sóc: {l.FullName}",
                    $"Đứng ở trạng thái \"{Labels.Vi(l.Status)}\" đã {LeadCareRules.DurationLabel(stalledHours)} — {LeadCareRules.NextAction(l.Status)}.",
                    recipients));
            }
        }

        var docCandidateIds = (await db.CandidateDocuments.Select(d => d.CandidateId).Distinct().ToListAsync()).ToHashSet();
        var needDocs = await db.CandidateJobOrders
            .Where(cjo => cjo.Status == CandidateJobOrderStatus.Active && cjo.CurrentStep >= WorkflowStep.Document)
            .Select(cjo => new { cjo.CandidateId, cjo.AssignedTo })
            .Distinct()
            .ToListAsync();
        foreach (var row in needDocs.Where(x => !docCandidateIds.Contains(x.CandidateId)))
        {
            var recipients = row.AssignedTo is not null
                ? new List<Guid> { row.AssignedTo.Value }
                : RoleUsers(roleRecipients, RoleNames.DocumentStaff, RoleNames.RecruitmentManager);
            events.Add(new ReminderEvent(
                NotificationType.ReminderDocument, row.CandidateId, "candidate",
                $"Thiếu hồ sơ: {Name(row.CandidateId)}",
                "Ứng viên đã tới bước hoàn thiện hồ sơ nhưng chưa có tài liệu nào được tải lên.",
                recipients));
        }

        // Nhắc thanh toán hoa hồng (§13): hoa hồng đã duyệt nhưng chưa chi → nhắc kế toán/giám đốc.
        var agentNames = await db.Agents.ToDictionaryAsync(a => a.Id, a => a.Name);
        var payableCommissions = await db.AgentCommissions
            .Where(c => c.Status == CommissionStatus.Approved)
            .Select(c => new { c.Id, c.AgentId, c.CommissionAmount })
            .ToListAsync();
        foreach (var c in payableCommissions)
        {
            events.Add(new ReminderEvent(
                NotificationType.CommissionPayment, c.Id, "commission",
                $"Hoa hồng chờ chi: {agentNames.GetValueOrDefault(c.AgentId, "Đại lý")}",
                $"{c.CommissionAmount:N0} đ đã được duyệt, chờ thanh toán cho đại lý.",
                RoleUsers(roleRecipients, RoleNames.Accountant, RoleNames.Director)));
        }

        return events;
    }

    private async Task<int> PersistEventsAsync(ApplicationDbContext db, IEnumerable<ReminderEvent> events, Guid? currentUserOnly)
    {
        var eventList = events.ToList();
        if (eventList.Count == 0) return 0;

        var existing = await db.Notifications
            .Where(n => n.ReferenceId != null)
            .Select(n => new { n.UserId, n.Type, n.ReferenceId, n.Channel, n.IsRead, n.CreatedAt })
            .ToListAsync();
        var seen = existing.Select(x => (x.UserId, x.Type, x.ReferenceId!.Value, x.Channel)).ToHashSet();

        // Nhắc chăm sóc lead được phép NHẮC LẠI khi lead vẫn đứng yên: chỉ chặn khi còn thông báo
        // chưa đọc hoặc đã nhắc trong 24 giờ qua (các loại nhắc khác giữ nguyên: 1 lần / reference).
        var leadCareCutoff = DateTimeOffset.UtcNow.AddHours(-24);
        var leadCareBlocked = existing
            .Where(x => x.Type == NotificationType.ReminderLeadCare)
            .GroupBy(x => (x.UserId, ReferenceId: x.ReferenceId!.Value, x.Channel))
            .Where(g => g.Any(x => !x.IsRead || x.CreatedAt >= leadCareCutoff))
            .Select(g => g.Key)
            .ToHashSet();

        var preferences = await db.NotificationPreferences.ToListAsync();
        var prefByUserType = preferences.ToDictionary(p => (p.UserId, p.Type));
        var toAdd = new List<Notification>();

        foreach (var reminder in eventList)
        {
            var recipients = reminder.Recipients.Distinct().ToList();
            if (currentUserOnly is not null)
                recipients = recipients.Contains(currentUserOnly.Value) ? new List<Guid> { currentUserOnly.Value } : new List<Guid>();

            foreach (var userId in recipients)
            {
                foreach (var channel in ChannelsFor(prefByUserType.GetValueOrDefault((userId, reminder.Type))))
                {
                    if (reminder.Type == NotificationType.ReminderLeadCare)
                    {
                        if (!leadCareBlocked.Add((userId, reminder.ReferenceId, channel))) continue;
                    }
                    else if (!seen.Add((userId, reminder.Type, reminder.ReferenceId, channel))) continue;
                    toAdd.Add(new Notification
                    {
                        UserId = userId,
                        Type = reminder.Type,
                        Channel = channel,
                        Title = reminder.Title,
                        Body = reminder.Body,
                        ReferenceType = reminder.ReferenceType,
                        ReferenceId = reminder.ReferenceId,
                        IsRead = false,
                    });
                }
            }
        }

        if (toAdd.Count == 0) return 0;
        db.Notifications.AddRange(toAdd);
        await db.SaveChangesAsync();
        return toAdd.Count;
    }

    private static IEnumerable<NotificationChannel> ChannelsFor(NotificationPreference? pref)
    {
        if (pref is null)
        {
            yield return NotificationChannel.InApp;
            yield break;
        }

        if (pref.InAppEnabled) yield return NotificationChannel.InApp;
        if (pref.EmailEnabled) yield return NotificationChannel.Email;
        if (pref.SmsEnabled) yield return NotificationChannel.Sms;
        if (pref.ZaloEnabled) yield return NotificationChannel.Zalo;
    }

    private static async Task<Dictionary<string, List<Guid>>> LoadRoleRecipientsAsync(ApplicationDbContext db)
    {
        var pairs = await db.UserRoles
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name, ur.UserId })
            .Where(x => x.Name != null)
            .ToListAsync();
        return pairs
            .GroupBy(x => x.Name!)
            .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).Distinct().ToList());
    }

    private static List<Guid> RoleUsers(Dictionary<string, List<Guid>> roleRecipients, params string[] roles)
        => roles.SelectMany(role => roleRecipients.GetValueOrDefault(role) ?? new List<Guid>())
            .Distinct()
            .ToList();

    private sealed record ReminderEvent(
        NotificationType Type,
        Guid ReferenceId,
        string ReferenceType,
        string Title,
        string Body,
        List<Guid> Recipients);
}
