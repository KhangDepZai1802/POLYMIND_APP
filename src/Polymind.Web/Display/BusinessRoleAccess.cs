using System.Security.Claims;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Display;

/// <summary>
/// Lớp chặn nghiệp vụ cho các thao tác sửa/xóa dữ liệu lõi.
/// Permission vẫn là điều kiện bắt buộc; helper này siết thêm theo vai trò trực tiếp liên quan.
/// </summary>
public static class BusinessRoleAccess
{
    public static bool CanEditLead(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.RecruitmentManager, RoleNames.Recruiter, RoleNames.Consultant, RoleNames.DocumentStaff);

    public static bool CanDeleteLead(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.RecruitmentManager, RoleNames.DocumentStaff);

    public static bool CanEditCandidateProfile(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.RecruitmentManager, RoleNames.Recruiter, RoleNames.Consultant, RoleNames.DocumentStaff);

    public static bool CanDeleteCandidate(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.DocumentStaff);

    public static bool CanEditJobOrder(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.RecruitmentManager);

    public static bool CanDeleteJobOrder(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.RecruitmentManager);

    public static bool CanEditLoan(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.Accountant, RoleNames.RecruitmentManager, RoleNames.Recruiter, RoleNames.Consultant);

    public static bool CanDeleteLoan(ClaimsPrincipal user)
        => HasAnyRole(user, RoleNames.SuperAdmin, RoleNames.Accountant);

    private static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
        => roles.Any(user.IsInRole);
}
