using Microsoft.AspNetCore.Identity;

namespace Polymind.Infrastructure.Identity;

/// <summary>Vai trò (8 roles nghiệp vụ). Gắn với Permission qua RolePermission.</summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
