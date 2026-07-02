using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Polymind.Infrastructure.Identity;

namespace Polymind.Web.Identity;

/// <summary>
/// 2FA bắt buộc cho MỌI tài khoản (quyết định user 02/07/2026).
/// Cache trạng thái "đã bật 2FA" 60 giây/user để middleware không phải query DB mỗi request;
/// sau khi bật/tắt 2FA phải gọi <see cref="Set"/>/<see cref="Invalidate"/> để cache cập nhật ngay.
/// </summary>
public class TwoFactorStatusCache(IMemoryCache cache)
{
    public async Task<bool> IsEnabledAsync(UserManager<ApplicationUser> userManager, string userId)
    {
        if (cache.TryGetValue(Key(userId), out bool enabled)) return enabled;
        var user = await userManager.FindByIdAsync(userId);
        enabled = user is not null && await userManager.GetTwoFactorEnabledAsync(user);
        cache.Set(Key(userId), enabled, TimeSpan.FromSeconds(60));
        return enabled;
    }

    public void Set(string userId, bool enabled) => cache.Set(Key(userId), enabled, TimeSpan.FromSeconds(60));

    public void Invalidate(string userId) => cache.Remove(Key(userId));

    private static string Key(string userId) => $"2fa-enabled:{userId}";
}

/// <summary>
/// Quy tắc đường dẫn cho middleware bắt buộc 2FA: user cookie chưa bật 2FA bị chuyển về
/// /account/2fa-setup, trừ các đường dẫn hạ tầng/đăng nhập/đăng xuất/API (API dùng JWT riêng).
/// </summary>
public static class TwoFactorEnforcement
{
    public const string SetupPath = "/account/2fa-setup";

    private static readonly string[] AllowedPrefixes =
    {
        SetupPath, "/account/logout", "/login", "/login-2fa", "/access-denied", "/not-found", "/error",
        "/_blazor", "/_framework", "/_content", "/api", "/swagger", "/health", "/hangfire",
        "/img", "/css", "/js", "/lib", "/favicon",
    };

    public static bool AppliesTo(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true) return false;
        // Chỉ áp cho phiên cookie của web app; API xác thực bằng JWT Bearer không đi qua đây.
        if (context.User.Identity.AuthenticationType != IdentityConstants.ApplicationScheme) return false;

        var path = context.Request.Path.Value ?? "/";
        if (Path.HasExtension(path)) return false; // static assets
        return !AllowedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
