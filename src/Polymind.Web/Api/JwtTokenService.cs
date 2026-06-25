using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Authorization;

namespace Polymind.Web.Api;

/// <summary>Cấu hình ký JWT cho REST API. Khóa lấy từ <c>Jwt:Key</c> (production qua env <c>Jwt__Key</c>).</summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Polymind";
    public string Audience { get; set; } = "PolymindApi";
    public string Key { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 240;
}

public sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAt,
    IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

/// <summary>
/// Sinh JWT mang theo claim <c>permission</c> (giống <see cref="Identity.PermissionClaimsPrincipalFactory"/>)
/// để dùng lại nguyên policy <c>resource:action</c> hiện có; role đặt ở <see cref="ClaimTypes.Role"/>.
/// </summary>
public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    UserManager<ApplicationUser> userManager,
    IDbContextFactory<ApplicationDbContext> dbFactory)
{
    private readonly JwtOptions _options = options.Value;

    public async Task<IssuedToken> CreateAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        await using var db = await dbFactory.CreateDbContextAsync();
        var roleIds = await db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();
        var permissions = await db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var permission in permissions)
            claims.Add(new Claim(PermissionClaimTypes.Permission, permission));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new IssuedToken(accessToken, expires, roles.ToList(), permissions);
    }
}
