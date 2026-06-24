using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Identity;

/// <summary>Thông tin phạm vi dữ liệu của đại lý đang đăng nhập.</summary>
/// <param name="IsAgentOnly">User chỉ thuộc role đại lý (không kèm role nhân sự nội bộ).</param>
/// <param name="AgentId">Đại lý tương ứng tài khoản, null nếu chưa gắn.</param>
public readonly record struct AgentScopeInfo(bool IsAgentOnly, Guid? AgentId);

/// <summary>
/// Giải quyết phạm vi dữ liệu cho Portal đại lý (spec §2.8): đại lý chỉ thấy ứng viên mình
/// giới thiệu + hoa hồng của mình. Dùng để lọc/chặn ở các trang dùng chung.
/// </summary>
public class AgentScope(
    AuthenticationStateProvider authStateProvider,
    IDbContextFactory<ApplicationDbContext> dbFactory)
{
    // Các role nội bộ có quyền xem dữ liệu rộng — nếu user có thêm bất kỳ role này thì KHÔNG bị bó hẹp.
    private static readonly string[] StaffRoles =
    {
        RoleNames.SuperAdmin, RoleNames.Director, RoleNames.RecruitmentManager, RoleNames.Recruiter,
        RoleNames.DocumentStaff, RoleNames.VisaStaff, RoleNames.Accountant
    };

    private AgentScopeInfo? _cached;

    public async ValueTask<AgentScopeInfo> GetAsync()
    {
        if (_cached is { } cached) return cached;

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var isAgentOnly = user.IsInRole(RoleNames.Agent) && !StaffRoles.Any(user.IsInRole);
        Guid? agentId = null;

        if (isAgentOnly && Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            agentId = await db.Agents.Where(a => a.UserId == userId)
                .Select(a => (Guid?)a.Id).FirstOrDefaultAsync();
        }

        var info = new AgentScopeInfo(isAgentOnly, agentId);
        _cached = info;
        return info;
    }
}
