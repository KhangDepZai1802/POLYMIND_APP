using Polymind.Domain.Messaging;
using Polymind.Infrastructure.Persistence.Constants;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M14 / CR-M14-3 — ma trận phân bậc tin nhắn. Luật gốc: `docs/messaging-tiers.md` (user chốt 2026-07-13).
///
/// Trước đây `MessagingPolicy` nằm ở `Polymind.Web` nên ma trận role phải kiểm THỦ CÔNG (blocker QA).
/// Nay ma trận nằm ở `Polymind.Domain.Messaging.MessagingTiers` → phủ được bằng máy.
///
/// Lưu ý: ma trận chỉ quyết định LOẠI ROLE. Việc "đúng người nào" do tầng quan hệ ứng viên quyết định
/// (`M14_MessagingRulesTests` + `Messages.razor`). Hai tầng chồng lên nhau, không thay thế nhau.
/// </summary>
public class M14_MessagingMatrixTests
{
    private static bool Can(string sender, string recipient)
        => MessagingTiers.CanMessage(new[] { sender }, new[] { recipient });

    // ===== (1) Super Admin: hai chiều, không giới hạn =====

    [Theory]
    [InlineData(RoleNames.Director)]
    [InlineData(RoleNames.Accountant)]
    [InlineData(RoleNames.RecruitmentManager)]
    [InlineData(RoleNames.DocumentStaff)]
    [InlineData(RoleNames.VisaStaff)]
    [InlineData(RoleNames.Consultant)]
    [InlineData(RoleNames.Recruiter)]
    [InlineData(RoleNames.Agent)]
    [InlineData(RoleNames.Collaborator)]
    [InlineData(RoleNames.Parent)]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.SuperAdmin)]
    public void Super_admin_can_message_everyone_both_ways(string other)
    {
        Assert.True(Can(RoleNames.SuperAdmin, other), $"super_admin → {other} phải được phép");
        Assert.True(Can(other, RoleNames.SuperAdmin), $"{other} → super_admin phải được phép (kênh hỗ trợ)");
    }

    // ===== (2) Chênh bậc ≤ 1 =====

    [Theory] // bậc 2 ↔ bậc 3
    [InlineData(RoleNames.Accountant)]
    [InlineData(RoleNames.RecruitmentManager)]
    [InlineData(RoleNames.DocumentStaff)]
    [InlineData(RoleNames.VisaStaff)]
    public void Director_and_operations_can_message_each_other(string ops)
    {
        Assert.True(Can(RoleNames.Director, ops));
        Assert.True(Can(ops, RoleNames.Director));
    }

    [Theory] // bậc 3 ↔ bậc 4 (kể cả Đại lý — đại lý chỉ bị cô lập TRONG bậc 4)
    [InlineData(RoleNames.Accountant, RoleNames.Consultant)]
    [InlineData(RoleNames.Accountant, RoleNames.Recruiter)]
    [InlineData(RoleNames.Accountant, RoleNames.Agent)]
    [InlineData(RoleNames.DocumentStaff, RoleNames.Consultant)]
    [InlineData(RoleNames.VisaStaff, RoleNames.Agent)]
    [InlineData(RoleNames.RecruitmentManager, RoleNames.Agent)]
    public void Operations_and_field_can_message_each_other(string ops, string field)
    {
        Assert.True(Can(ops, field));
        Assert.True(Can(field, ops));
    }

    [Theory] // cùng bậc 3 — cho phép cả cùng role lẫn khác role
    [InlineData(RoleNames.Accountant, RoleNames.Accountant)]
    [InlineData(RoleNames.Accountant, RoleNames.DocumentStaff)]
    [InlineData(RoleNames.DocumentStaff, RoleNames.VisaStaff)]
    [InlineData(RoleNames.RecruitmentManager, RoleNames.Accountant)]
    public void Same_tier_operations_allowed(string a, string b) => Assert.True(Can(a, b));

    [Fact] // cùng bậc 2
    public void Director_to_director_allowed() => Assert.True(Can(RoleNames.Director, RoleNames.Director));

    [Theory] // bậc 4: TVV ↔ NV tuyển dụng, và NVTD ↔ NVTD (chỉ TVV✗TVV bị chặn)
    [InlineData(RoleNames.Consultant, RoleNames.Recruiter)]
    [InlineData(RoleNames.Recruiter, RoleNames.Consultant)]
    [InlineData(RoleNames.Recruiter, RoleNames.Recruiter)]
    public void Field_staff_without_agent_can_message_each_other(string a, string b) => Assert.True(Can(a, b));

    // ===== (3) Ba ngoại lệ chặn =====

    [Fact] // TVV không nhắn TVV khác
    public void Consultant_cannot_message_consultant()
        => Assert.False(Can(RoleNames.Consultant, RoleNames.Consultant));

    [Fact] // CTV không nhắn CTV khác
    public void Collaborator_cannot_message_collaborator()
        => Assert.False(Can(RoleNames.Collaborator, RoleNames.Collaborator));

    [Theory] // Đại lý bị cô lập khỏi TOÀN BỘ bậc 4 (đại lý khác = đối thủ; TVV/NVTD = nhân sự nội bộ)
    [InlineData(RoleNames.Agent)]
    [InlineData(RoleNames.Consultant)]
    [InlineData(RoleNames.Recruiter)]
    public void Agent_is_isolated_within_field_tier(string otherField)
    {
        Assert.False(Can(RoleNames.Agent, otherField), $"agent → {otherField} phải bị chặn");
        Assert.False(Can(otherField, RoleNames.Agent), $"{otherField} → agent phải bị chặn");
    }

    // ===== (2b) Chênh bậc ≥ 2 → chặn =====

    [Theory]
    [InlineData(RoleNames.Director, RoleNames.Consultant)]   // 2 ↔ 4
    [InlineData(RoleNames.Director, RoleNames.Recruiter)]    // 2 ↔ 4
    [InlineData(RoleNames.Director, RoleNames.Agent)]        // 2 ↔ 4
    [InlineData(RoleNames.Director, RoleNames.Student)]      // 2 ↔ 5
    [InlineData(RoleNames.Director, RoleNames.Parent)]       // 2 ↔ 5
    [InlineData(RoleNames.Director, RoleNames.Collaborator)] // 2 ↔ 5
    [InlineData(RoleNames.Accountant, RoleNames.Student)]    // 3 ↔ 5
    [InlineData(RoleNames.DocumentStaff, RoleNames.Parent)]  // 3 ↔ 5
    [InlineData(RoleNames.VisaStaff, RoleNames.Student)]     // 3 ↔ 5
    [InlineData(RoleNames.RecruitmentManager, RoleNames.Collaborator)] // 3 ↔ 5
    public void Tier_gap_of_two_or_more_is_blocked(string a, string b)
    {
        Assert.False(Can(a, b), $"{a} → {b} phải bị chặn (chênh ≥ 2 bậc)");
        Assert.False(Can(b, a), $"{b} → {a} phải bị chặn (chênh ≥ 2 bậc)");
    }

    // ===== bậc 4 ↔ bậc 5 (ma trận cho phép; tầng quan hệ siết tiếp) =====

    [Theory]
    [InlineData(RoleNames.Consultant, RoleNames.Student)]
    [InlineData(RoleNames.Consultant, RoleNames.Parent)]
    [InlineData(RoleNames.Agent, RoleNames.Collaborator)]
    [InlineData(RoleNames.Recruiter, RoleNames.Student)] // ma trận OK; quan hệ ứng viên mới là chỗ chặn
    public void Field_and_portal_allowed_at_matrix_level(string field, string portal)
    {
        Assert.True(Can(field, portal));
        Assert.True(Can(portal, field));
    }

    [Theory] // bậc 5 nội bộ: người nhà nhắn nhau được (quan hệ ứng viên siết đúng người)
    [InlineData(RoleNames.Student, RoleNames.Parent)]
    [InlineData(RoleNames.Collaborator, RoleNames.Student)]
    [InlineData(RoleNames.Collaborator, RoleNames.Parent)]
    public void Portal_tier_internal_allowed_at_matrix_level(string a, string b) => Assert.True(Can(a, b));

    // ===== Fail-closed =====

    [Fact]
    public void Empty_roles_fail_closed()
    {
        Assert.False(MessagingTiers.CanMessage(Array.Empty<string>(), new[] { RoleNames.SuperAdmin }));
        Assert.False(MessagingTiers.CanMessage(new[] { RoleNames.SuperAdmin }, Array.Empty<string>()));
    }

    [Fact]
    public void Unknown_role_fails_closed()
    {
        Assert.False(Can("nonexistent_role", RoleNames.SuperAdmin));
        Assert.False(Can(RoleNames.SuperAdmin, "nonexistent_role"));
    }

    [Fact] // user đa-role: lấy bậc CAO NHẤT (số nhỏ nhất) làm bậc hiệu lực
    public void Multi_role_user_takes_highest_tier()
    {
        // Người vừa là CTV (bậc 5) vừa là Kế toán (bậc 3) → bậc hiệu lực = 3.
        var multi = new[] { RoleNames.Collaborator, RoleNames.Accountant };

        // Bậc 3 → nhắn được Giám đốc (bậc 2, chênh 1).
        Assert.True(MessagingTiers.CanMessage(multi, new[] { RoleNames.Director }));

        // Nhưng cũng vì bậc hiệu lực là 3, họ KHÔNG còn với tới bậc 5 (chênh 2) —
        // kể cả CTV khác. Nâng bậc thì mất kênh xuống portal, đúng ma trận.
        Assert.False(MessagingTiers.CanMessage(multi, new[] { RoleNames.Collaborator }));
        Assert.False(MessagingTiers.CanMessage(multi, new[] { RoleNames.Student }));
    }

    // ===== Chống lệch tên role giữa Domain và Infrastructure =====

    [Fact] // Domain dùng string literal (không ref được RoleNames) → khóa lại để không lệch âm thầm
    public void Tier_map_covers_every_role()
    {
        var declared = RoleNames.All.Keys.OrderBy(x => x, StringComparer.Ordinal);
        var mapped = MessagingTiers.TierByRole.Keys.OrderBy(x => x, StringComparer.Ordinal);

        Assert.Equal(declared, mapped);
    }
}
