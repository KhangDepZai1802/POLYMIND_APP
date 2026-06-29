namespace Polymind.Infrastructure.Persistence;

/// <summary>Sinh danh sách quyền chuẩn theo (resource, action) cho RBAC.</summary>
public static class PermissionRegistry
{
    public static readonly string[] Resources =
    {
        "leads", "candidates", "job_orders", "payments", "expenses", "receipts",
        "agents", "collaborators", "commissions", "loans", "visas", "flights", "reports", "dashboard",
        "users", "roles", "notifications", "messages", "audit"
    };

    public static readonly string[] Actions = { "create", "read", "update", "delete", "approve" };

    /// <summary>Trả về toàn bộ cặp (name, resource, action). Vd: leads:create.</summary>
    public static IEnumerable<(string Name, string Resource, string Action)> All()
    {
        foreach (var r in Resources)
            foreach (var a in Actions)
                yield return ($"{r}:{a}", r, a);
    }
}
