using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Polymind.Infrastructure.Identity;
using System.Security.Claims;

namespace Polymind.Web.Identity;

/// <summary>
/// Cung cấp AuthenticationState cho Blazor Server từ cookie Identity và định kỳ
/// kiểm tra lại security stamp (đăng xuất tự động nếu tài khoản đổi mật khẩu/khóa).
/// </summary>
public sealed class IdentityRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> options)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);
    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null) return false;
        if (!userManager.SupportsUserSecurityStamp) return true;

        var principalStamp = principal.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
        var userStamp = await userManager.GetSecurityStampAsync(user);
        return principalStamp == userStamp;
    }
}
