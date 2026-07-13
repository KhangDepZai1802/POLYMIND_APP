using Polymind.Domain.Messaging;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Identity;

/// <summary>
/// Quy tắc ai được nhắn tin cho ai.
///
/// <para>Ma trận theo BẬC nằm ở <see cref="MessagingTiers"/> (Domain, unit-test được).
/// Tài liệu gốc: <c>docs/messaging-tiers.md</c>.</para>
///
/// <para>Lớp này chỉ còn phần phụ thuộc UI/Identity (nhãn role, nhận diện portal/CTV).
/// Việc "đúng người nào" (không chỉ đúng loại role) do tầng quan hệ ứng viên trong
/// <c>Messages.razor</c> quyết định — ma trận KHÔNG thay thế được nó.</para>
/// </summary>
public static class MessagingPolicy
{
    public static bool IsPortalUser(IReadOnlyCollection<string> roles)
        => roles.Contains(RoleNames.Parent) || roles.Contains(RoleNames.Student);

    public static bool IsCollaborator(IReadOnlyCollection<string> roles)
        => roles.Contains(RoleNames.Collaborator);

    public static bool IsSuperAdmin(IReadOnlyCollection<string> roles)
        => roles.Contains(RoleNames.SuperAdmin);

    /// <summary>Ma trận bậc — fail-closed. Xem <see cref="MessagingTiers.CanMessage"/>.</summary>
    public static bool CanMessage(IReadOnlyCollection<string> senderRoles, IReadOnlyCollection<string> recipientRoles)
        => MessagingTiers.CanMessage(senderRoles, recipientRoles);

    /// <summary>Nhãn vai trò chính (ưu tiên cao nhất) để hiển thị cạnh tên người dùng.</summary>
    public static string PrimaryRoleLabel(IReadOnlyCollection<string> roles)
    {
        var role = PrimaryRole(roles);
        return string.IsNullOrEmpty(role) ? "—" : RoleNames.All.GetValueOrDefault(role, role);
    }

    /// <summary>Role chính (ưu tiên cao nhất) — dùng cho bộ lọc vai trò ở danh bạ.</summary>
    public static string PrimaryRole(IReadOnlyCollection<string> roles)
    {
        foreach (var r in Priority)
            if (roles.Contains(r)) return r;
        return roles.Count > 0 ? roles.First() : "";
    }

    private static readonly string[] Priority =
    {
        RoleNames.SuperAdmin, RoleNames.Director, RoleNames.RecruitmentManager, RoleNames.Recruiter,
        RoleNames.Consultant, RoleNames.DocumentStaff, RoleNames.VisaStaff, RoleNames.Accountant,
        RoleNames.Agent, RoleNames.Collaborator
    };
}
