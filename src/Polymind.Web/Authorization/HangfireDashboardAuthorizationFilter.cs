using Hangfire.Dashboard;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Authorization;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true
               && (user.IsInRole(RoleNames.SuperAdmin) || user.IsInRole(RoleNames.Director));
    }
}
