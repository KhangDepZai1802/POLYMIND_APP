using Microsoft.AspNetCore.Identity;

namespace Polymind.Infrastructure.Identity;

/// <summary>Tài khoản người dùng hệ thống (mở rộng IdentityUser, khóa Guid).</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
