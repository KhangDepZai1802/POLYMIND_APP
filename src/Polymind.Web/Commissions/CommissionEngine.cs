using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polymind.Domain.Commissions;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Auditing;

namespace Polymind.Web.Commissions;

/// <summary>
/// Sinh hoa hồng đại lý theo GIAI ĐOẠN ĐÓNG TIỀN (góp ý Vietgroup 07/2026): mỗi mốc hoa hồng
/// chỉ phát sinh khi ứng viên đã đóng đúng giai đoạn tương ứng (Payment.Stage = Paid).
/// Idempotent theo (AgentId, CandidateId, Milestone); giữ NGUYÊN cách tính số tiền cũ
/// (% cấu hình × chi phí đơn hàng) nên tổng hoa hồng không đổi — chỉ đổi THỜI ĐIỂM phát sinh.
/// </summary>
public static class CommissionEngine
{
    private const string IdempotencyIndexName = "ix_agent_commissions_agent_id_candidate_id_milestone";

    /// <summary>Mốc hoa hồng ↔ giai đoạn đóng tiền kích hoạt.</summary>
    public static readonly (CommissionMilestone Milestone, PaymentStage Stage)[] Map =
    {
        (CommissionMilestone.Deposit, PaymentStage.Deposit),
        (CommissionMilestone.Selected, PaymentStage.ServiceFee),
        (CommissionMilestone.Departure, PaymentStage.Settlement),
    };

    /// <summary>
    /// Với 1 ứng viên: phát sinh các lát hoa hồng cho những giai đoạn đóng tiền đã hoàn tất
    /// mà chưa có hoa hồng. Hàm tự lưu hoa hồng + audit và trả về số lát thực sự do lượt gọi này tạo.
    /// Unique index là chốt cuối; khi hai caller đụng nhau, chỉ retry đúng lỗi từ index idempotency.
    /// </summary>
    public static async Task<int> EnsureAsync(ApplicationDbContext db, Guid candidateId, Guid actorId)
    {
        var candidate = await db.Candidates.AsNoTracking()
            .Where(c => c.Id == candidateId)
            .Select(c => new { c.Id, c.AgentId, c.CollaboratorId })
            .FirstOrDefaultAsync();
        if (candidate?.AgentId is not Guid agentId) return 0;

        var cjo = await db.CandidateJobOrders.AsNoTracking()
            .Where(j => j.CandidateId == candidateId)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync();
        if (cjo is null) return 0;

        var jo = await db.JobOrders.AsNoTracking().FirstOrDefaultAsync(j => j.Id == cjo.JobOrderId);
        if (jo is null) return 0;
        var baseAmount = jo.CostAmount ?? 0m;

        var paidStages = await db.Payments.AsNoTracking()
            .Where(p => p.CandidateId == candidateId && p.Stage != null && p.Status == PaymentStatus.Paid)
            .Select(p => p.Stage!.Value)
            .ToListAsync();
        if (paidStages.Count == 0) return 0;

        var configs = await db.AgentCommissionConfigs.AsNoTracking()
            .Where(c => c.AgentId == agentId).ToListAsync();

        var collaboratorSnapshot = candidate.CollaboratorId is Guid collaboratorId
            ? await db.Collaborators.AsNoTracking()
                .Where(c => c.Id == collaboratorId && c.AgentId == agentId)
                .Select(c => new { c.Id, c.CommissionSharePercentage })
                .FirstOrDefaultAsync()
            : null;

        var pending = new List<AgentCommission>();
        foreach (var (milestone, stage) in Map)
        {
            if (!paidStages.Contains(stage)) continue;

            // Mỗi ứng viên chỉ hưởng mốc này 1 lần (kể cả khi đổi đơn ở bước 7.5).
            var exists = await db.AgentCommissions.AnyAsync(c =>
                c.AgentId == agentId && c.CandidateId == candidateId && c.Milestone == milestone);
            if (exists) continue;

            var cfg = configs.Where(c => c.Milestone == milestone)
                .OrderByDescending(c => c.JobOrderId == cjo.JobOrderId)
                .ThenByDescending(c => c.Country == jo.Country)
                .FirstOrDefault();
            if (cfg is null) continue;

            var amount = cfg.Percentage.HasValue
                ? baseAmount * cfg.Percentage.Value / 100m
                : (cfg.FixedAmount ?? 0m);

            var commission = new AgentCommission
            {
                AgentId = agentId,
                CandidateId = candidateId,
                JobOrderId = cjo.JobOrderId,
                ConfigId = cfg.Id,
                Milestone = milestone,
                Stage = stage,
                BaseAmount = baseAmount,
                CommissionAmount = amount,
                CollaboratorId = collaboratorSnapshot?.Id,
                CollaboratorSharePercentage = collaboratorSnapshot is null
                    ? null
                    : AgentCommissionRates.NormalizeCollaboratorShare(collaboratorSnapshot.CommissionSharePercentage),
                Status = CommissionStatus.Pending,
            };
            pending.Add(commission);
        }

        return await PersistAsync(db, pending, actorId);
    }

    private static async Task<int> PersistAsync(
        ApplicationDbContext db,
        List<AgentCommission> desired,
        Guid actorId)
    {
        if (desired.Count == 0) return 0;

        // Tối đa ba mốc. Mỗi conflict đồng nghĩa một caller khác vừa thắng ít nhất một khóa;
        // nạp lại rồi chỉ retry những mốc vẫn còn thiếu.
        for (var attempt = 0; attempt <= Map.Length; attempt++)
        {
            var agentId = desired[0].AgentId;
            var candidateId = desired[0].CandidateId;
            var existing = await db.AgentCommissions.AsNoTracking()
                .Where(c => c.AgentId == agentId && c.CandidateId == candidateId)
                .Select(c => c.Milestone)
                .ToListAsync();
            var existingSet = existing.ToHashSet();
            var remaining = desired.Where(c => !existingSet.Contains(c.Milestone)).ToList();
            if (remaining.Count == 0) return 0;

            db.AgentCommissions.AddRange(remaining);
            foreach (var commission in remaining)
            {
                db.AddAudit(actorId, "create", "agent_commissions", commission.Id, null, new
                {
                    commission.AgentId,
                    commission.CandidateId,
                    commission.JobOrderId,
                    commission.ConfigId,
                    commission.Milestone,
                    commission.Stage,
                    commission.BaseAmount,
                    commission.CommissionAmount,
                    commission.CollaboratorId,
                    commission.CollaboratorSharePercentage,
                    commission.Status,
                });
            }

            try
            {
                await db.SaveChangesAsync();
                return remaining.Count;
            }
            catch (DbUpdateException ex) when (IsIdempotencyConflict(ex))
            {
                DetachGeneratedEntries(db, remaining);
                if (attempt == Map.Length) throw;
            }
        }

        return 0;
    }

    private static bool IsIdempotencyConflict(DbUpdateException ex)
        => ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: IdempotencyIndexName,
        };

    private static void DetachGeneratedEntries(
        ApplicationDbContext db,
        IReadOnlyCollection<AgentCommission> commissions)
    {
        var ids = commissions.Select(c => c.Id).ToHashSet();
        foreach (var entry in db.ChangeTracker.Entries<AgentCommission>()
                     .Where(e => ids.Contains(e.Entity.Id)))
            entry.State = EntityState.Detached;

        foreach (var entry in db.ChangeTracker.Entries<AuditLog>()
                     .Where(e => e.Entity.Resource == "agent_commissions"
                                 && e.Entity.ResourceId is Guid resourceId
                                 && ids.Contains(resourceId)))
            entry.State = EntityState.Detached;
    }
}
