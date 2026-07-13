using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Commissions;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M09 — Agents &amp; Commissions. Pin hằng số tỉ lệ hoa hồng (Vietgroup 07/2026) — business-critical:
/// đổi âm thầm sẽ sai tiền hoa hồng toàn hệ thống (seeder + chuẩn hóa dữ liệu cũ dùng chung).
/// LƯU Ý PHẠM VI: CommissionEngine (idempotency, chọn config, tính amount) + clamp CTV 30-40%
/// nằm trong Polymind.Web → KHÔNG unit-test được ở đây (không ref Web). Xem 05-automation-report.md.
/// TC_M09_003, TC_M09_015/016, TC_M09_030..033.
/// </summary>
public class M09_CommissionRatesTests
{
    [Fact] // TC_M09_030 — đại lý hưởng tổng 5% chia 1 / 1.5 / 2.5 theo 3 mốc
    public void Agent_commission_rate_splits_are_1_1p5_2p5_totalling_5()
    {
        Assert.Equal(1m, AgentCommissionRates.Deposit);
        Assert.Equal(1.5m, AgentCommissionRates.Selected);
        Assert.Equal(2.5m, AgentCommissionRates.Departure);
        Assert.Equal(5m, AgentCommissionRates.Total);
        Assert.Equal(
            AgentCommissionRates.Deposit + AgentCommissionRates.Selected + AgentCommissionRates.Departure,
            AgentCommissionRates.Total);
    }

    [Fact] // TC_M09_031 — CTV hưởng 30-40% hoa hồng đại lý, mặc định 35%
    public void Collaborator_share_bounds_are_30_to_40_default_35()
    {
        Assert.Equal(30m, AgentCommissionRates.CollaboratorShareMin);
        Assert.Equal(40m, AgentCommissionRates.CollaboratorShareMax);
        Assert.Equal(35m, AgentCommissionRates.CollaboratorShareDefault);
        Assert.InRange(
            AgentCommissionRates.CollaboratorShareDefault,
            AgentCommissionRates.CollaboratorShareMin,
            AgentCommissionRates.CollaboratorShareMax);
    }

    [Fact] // TC_M09_032 — CTV mới mặc định 35% (khớp hằng số Vietgroup)
    public void New_collaborator_defaults_to_35_percent_share()
    {
        var ctv = new Collaborator();

        Assert.Equal(35m, ctv.CommissionSharePercentage);
        Assert.True(ctv.IsActive);
    }

    [Fact] // TC_M09_033 — hoa hồng mới phát sinh ở trạng thái Pending
    public void New_commission_starts_pending()
    {
        var commission = new AgentCommission();

        Assert.Equal(Polymind.Domain.Enums.CommissionStatus.Pending, commission.Status);
    }

    [Fact] // BUG_M09_01 / TC_M09_003 — DB phải là chốt idempotency cuối dưới concurrency
    public void Agent_commission_model_has_unique_idempotency_index()
    {
        using var db = new ApplicationDbContextFactory().CreateDbContext([]);
        var entity = db.Model.FindEntityType(typeof(AgentCommission));
        var index = Assert.Single(entity!.GetIndexes(), i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(AgentCommission.AgentId),
                nameof(AgentCommission.CandidateId),
                nameof(AgentCommission.Milestone),
            }));

        Assert.True(index.IsUnique);
        Assert.Equal(
            "ix_agent_commissions_agent_id_candidate_id_milestone",
            index.GetDatabaseName());
    }

    [Theory] // BUG_M09_02 / TC_M09_015 — chỉ Pending mới được duyệt
    [InlineData(CommissionStatus.Pending, true)]
    [InlineData(CommissionStatus.Approved, false)]
    [InlineData(CommissionStatus.Paid, false)]
    public void Approve_transition_is_guarded(CommissionStatus current, bool expected)
        => Assert.Equal(expected, AgentCommissionTransitions.CanApprove(current));

    [Theory] // BUG_M09_02 / TC_M09_016 — chỉ Approved mới được đánh dấu đã chi
    [InlineData(CommissionStatus.Pending, false)]
    [InlineData(CommissionStatus.Approved, true)]
    [InlineData(CommissionStatus.Paid, false)]
    public void Mark_paid_transition_is_guarded(CommissionStatus current, bool expected)
        => Assert.Equal(expected, AgentCommissionTransitions.CanMarkPaid(current));

    [Fact] // CR-M09-1 / U-M09-1 — % CTV đã snapshot không đổi theo cấu hình hiện tại
    public void Collaborator_share_uses_snapshot_from_commission_history()
    {
        var commission = new AgentCommission
        {
            CommissionAmount = 1_000_000m,
            CollaboratorId = Guid.NewGuid(),
            CollaboratorSharePercentage = 35m,
        };
        var collaborator = new Collaborator { CommissionSharePercentage = 40m };

        var historicalShare = AgentCommissionRates.CollaboratorShareAmount(
            commission.CommissionAmount,
            commission.CollaboratorSharePercentage!.Value);

        Assert.Equal(350_000m, historicalShare);
        Assert.Equal(400_000m, AgentCommissionRates.CollaboratorShareAmount(
            commission.CommissionAmount,
            collaborator.CommissionSharePercentage));
    }

    [Theory] // CR-M09-2 / U-M09-2 — partner fail-closed, staff giữ leaderboard đầy đủ
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    public void Partner_only_sees_own_agency_data(bool partnerOnly, bool sameAgency, bool expected)
    {
        var ownAgentId = Guid.NewGuid();
        var dataAgentId = sameAgency ? ownAgentId : Guid.NewGuid();

        Assert.Equal(expected, PartnerLeaderboardVisibility.CanSeeAgentData(
            partnerOnly,
            ownAgentId,
            dataAgentId));
    }

    [Fact]
    public void Unmapped_partner_cannot_see_any_agency_data()
        => Assert.False(PartnerLeaderboardVisibility.CanSeeAgentData(
            isPartnerOnly: true,
            currentAgentId: null,
            dataAgentId: Guid.NewGuid()));

    [Fact] // CR-M09-1 — model/migration contract cho snapshot lịch sử
    public void Commission_model_persists_collaborator_snapshot_and_indexes_recipient()
    {
        using var db = new ApplicationDbContextFactory().CreateDbContext([]);
        var entity = db.Model.FindEntityType(typeof(AgentCommission))!;
        var percentage = entity.FindProperty(nameof(AgentCommission.CollaboratorSharePercentage));

        Assert.NotNull(entity.FindProperty(nameof(AgentCommission.CollaboratorId)));
        Assert.Equal(5, percentage!.GetPrecision());
        Assert.Equal(2, percentage.GetScale());
        Assert.Contains(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(AgentCommission.CollaboratorId) }));
    }
}
