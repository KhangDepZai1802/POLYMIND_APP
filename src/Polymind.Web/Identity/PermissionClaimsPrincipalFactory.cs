using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Authorization;

namespace Polymind.Web.Identity;

public sealed class PermissionClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor,
    ApplicationDbContext db)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var roleIds = await db.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == user.Id)
            .Select(userRole => userRole.RoleId)
            .ToListAsync();

        var permissions = await db.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
            .Select(rolePermission => rolePermission.Permission.Name)
            .Distinct()
            .OrderBy(permission => permission)
            .ToListAsync();

        foreach (var permission in permissions)
        {
            if (!identity.HasClaim(PermissionClaimTypes.Permission, permission))
                identity.AddClaim(new Claim(PermissionClaimTypes.Permission, permission));
        }

        return identity;
    }
}
