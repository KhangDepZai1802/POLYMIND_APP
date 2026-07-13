using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M02 — Authorization, Roles &amp; Permissions.
/// Kiểm chứng hợp đồng của <see cref="PermissionRegistry"/> — bộ từ vựng RBAC mà toàn hệ thống
/// (DbSeeder role map, PermissionPolicyProvider, [Authorize(Policy)]) phụ thuộc.
/// TC_M02_001..006.
/// </summary>
public class M02_PermissionRegistryTests
{
    [Fact] // TC_M02_001
    public void Generates_21_resources_times_5_actions_105_permissions()
    {
        Assert.Equal(21, PermissionRegistry.Resources.Length);
        Assert.Equal(5, PermissionRegistry.Actions.Length);
        Assert.Equal(105, PermissionRegistry.All().Count());
    }

    [Fact] // TC_M02_002
    public void All_permission_names_are_wellformed_resource_colon_action()
    {
        foreach (var (name, resource, action) in PermissionRegistry.All())
        {
            Assert.Equal($"{resource}:{action}", name);
            Assert.Contains(resource, PermissionRegistry.Resources);
            Assert.Contains(action, PermissionRegistry.Actions);
        }
    }

    [Fact] // TC_M02_003
    public void Permission_names_are_unique()
    {
        var names = PermissionRegistry.All().Select(p => p.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact] // TC_M02_004 — actions chuẩn RBAC
    public void Actions_are_exactly_crud_plus_approve()
    {
        Assert.Equal(
            new[] { "create", "read", "update", "delete", "approve" },
            PermissionRegistry.Actions);
    }

    [Fact] // TC_M02_005 — mọi resource mà DbSeeder/UI tham chiếu phải có mặt
    public void Registry_contains_all_resources_referenced_by_role_map()
    {
        // Bộ resource được DbSeeder.RolePermissionMap và [Authorize] dùng trong app.
        string[] referenced =
        {
            "leads", "candidates", "job_orders", "payments", "expenses", "receipts",
            "agents", "collaborators", "commissions", "loans", "visas", "flights",
            "reports", "financial_reports", "dashboard", "users", "roles", "notifications", "messages",
            "audit", "training"
        };
        foreach (var r in referenced)
            Assert.Contains(r, PermissionRegistry.Resources);
    }

    [Fact] // TC_M02_006 — không có resource trùng lặp trong danh mục
    public void Resources_are_unique()
    {
        Assert.Equal(
            PermissionRegistry.Resources.Length,
            PermissionRegistry.Resources.Distinct().Count());
    }
}
