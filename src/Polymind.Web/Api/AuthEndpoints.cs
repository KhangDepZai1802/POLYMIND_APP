using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Polymind.Infrastructure.Identity;
using Polymind.Web.Authorization;

namespace Polymind.Web.Api;

/// <summary>Endpoint đăng nhập cấp JWT và xem thông tin tài khoản hiện tại.</summary>
public static class AuthEndpoints
{
    public static void MapAuthApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest request,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { message = "Email và mật khẩu là bắt buộc." });

            var user = await userManager.FindByEmailAsync(request.Email.Trim());
            if (user is null)
                return Results.Json(new { message = "Email hoặc mật khẩu không đúng." }, statusCode: StatusCodes.Status401Unauthorized);

            if (!user.IsActive)
                return Results.Json(new { message = "Tài khoản đã bị khóa." }, statusCode: StatusCodes.Status403Forbidden);

            // Dùng SignInManager để áp đúng chính sách lockout như đăng nhập web.
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
                return Results.Json(new { message = "Tài khoản tạm khóa do đăng nhập sai nhiều lần. Thử lại sau ít phút." }, statusCode: StatusCodes.Status423Locked);
            if (!result.Succeeded)
                return Results.Json(new { message = "Email hoặc mật khẩu không đúng." }, statusCode: StatusCodes.Status401Unauthorized);

            var issued = await tokenService.CreateAsync(user);
            user.LastLoginAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);

            var info = new UserInfo(user.Id, user.Email ?? string.Empty, user.FullName, issued.Roles, issued.Permissions);
            return Results.Ok(new TokenResponse(issued.AccessToken, "Bearer", issued.ExpiresAt, info));
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Đăng nhập, trả về JWT access token + quyền của tài khoản.");

        group.MapGet("/me", (ClaimsPrincipal principal) =>
        {
            var id = principal.UserId();
            if (id is null) return Results.Unauthorized();

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = principal.FindAll(PermissionClaimTypes.Permission).Select(c => c.Value).OrderBy(p => p).ToList();
            var info = new UserInfo(
                id.Value,
                principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                roles,
                permissions);
            return Results.Ok(info);
        })
        .RequireAuthorization(ApiAuth.Bearer())
        .WithName("Me")
        .WithSummary("Thông tin tài khoản đang đăng nhập (theo JWT).");
    }
}
