using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Identity;

/// <summary>
/// Quy tắc ai được nhắn tin cho ai (theo role người nhận).
/// - Ai cũng nhắn được cho Super Admin (kênh hỗ trợ).
/// - CHỈ Super Admin mới nhắn được cho Giám đốc.
/// - Đại lý (role agent ≈ đại lý/CTV) chỉ nhận tin từ Super Admin / Giám đốc / TP tuyển dụng / NV tuyển dụng
///   (những người "có liên quan" tới cộng tác viên).
/// - Các trường hợp còn lại (nhân sự nội bộ với nhau): được phép.
/// </summary>
public static class MessagingPolicy
{
    public static bool CanMessage(IReadOnlyCollection<string> senderRoles, IReadOnlyCollection<string> recipientRoles)
    {
        if (senderRoles.Count == 0 || recipientRoles.Count == 0) return false;

        // Người nhận là Super Admin → ai cũng nhắn được.
        if (recipientRoles.Contains(RoleNames.SuperAdmin)) return true;

        // Người nhận là Giám đốc → chỉ Super Admin.
        if (recipientRoles.Contains(RoleNames.Director))
            return senderRoles.Contains(RoleNames.SuperAdmin);

        // Người nhận là Đại lý/CTV → chỉ các vai trò liên quan tuyển dụng.
        if (recipientRoles.Contains(RoleNames.Agent) || recipientRoles.Contains(RoleNames.Collaborator))
            return senderRoles.Contains(RoleNames.SuperAdmin)
                || senderRoles.Contains(RoleNames.Director)
                || senderRoles.Contains(RoleNames.RecruitmentManager)
                || senderRoles.Contains(RoleNames.Recruiter);

        // Nhân sự nội bộ với nhau.
        return true;
    }

    /// <summary>Nhãn vai trò chính (ưu tiên cao nhất) để hiển thị cạnh tên người dùng.</summary>
    public static string PrimaryRoleLabel(IReadOnlyCollection<string> roles)
    {
        foreach (var r in Priority)
            if (roles.Contains(r)) return RoleNames.All.GetValueOrDefault(r, r);
        return roles.Count > 0 ? RoleNames.All.GetValueOrDefault(roles.First(), roles.First()) : "—";
    }

    private static readonly string[] Priority =
    {
        RoleNames.SuperAdmin, RoleNames.Director, RoleNames.RecruitmentManager, RoleNames.Recruiter,
        RoleNames.Consultant, RoleNames.DocumentStaff, RoleNames.VisaStaff, RoleNames.Accountant,
        RoleNames.Agent, RoleNames.Collaborator
    };
}
