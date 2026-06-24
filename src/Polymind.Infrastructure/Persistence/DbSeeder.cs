using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polymind.Domain.Entities;
using Polymind.Infrastructure.Identity;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Infrastructure.Persistence;

/// <summary>Áp migration + seed roles, permissions và tài khoản super_admin lúc khởi động.</summary>
public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@polymind.local";
    public const string DefaultAdminPassword = "Admin@123";

    private static readonly IReadOnlyList<SeedUser> SeedUsers = new[]
    {
        new SeedUser(DefaultAdminEmail, "Super Admin", RoleNames.SuperAdmin),
        new SeedUser("director@polymind.local", "Giám đốc", RoleNames.Director),
        new SeedUser("recruitment.manager@polymind.local", "Trưởng phòng tuyển dụng", RoleNames.RecruitmentManager),
        new SeedUser("recruiter@polymind.local", "Nhân viên tuyển dụng", RoleNames.Recruiter),
        new SeedUser("document.staff@polymind.local", "Bộ phận hồ sơ", RoleNames.DocumentStaff),
        new SeedUser("visa.staff@polymind.local", "Bộ phận visa", RoleNames.VisaStaff),
        new SeedUser("accountant@polymind.local", "Kế toán", RoleNames.Accountant),
        new SeedUser("agent@polymind.local", "Đại lý / CTV", RoleNames.Agent),
    };

    private static readonly IReadOnlyDictionary<string, string[]> RolePermissionMap = new Dictionary<string, string[]>
    {
        [RoleNames.Director] = Combine(
            Read("dashboard", "leads", "candidates", "job_orders", "payments", "expenses", "receipts",
                "agents", "commissions", "visas", "flights", "reports", "users", "roles", "notifications", "audit"),
            Actions("payments", "approve"),
            Actions("expenses", "approve"),
            Actions("commissions", "approve"),
            Actions("reports", "create", "read")),

        [RoleNames.RecruitmentManager] = Combine(
            Crud("leads"),
            Actions("candidates", "create", "read", "update"),
            Read("dashboard", "job_orders", "agents", "reports", "notifications")),

        [RoleNames.Recruiter] = Combine(
            Actions("leads", "create", "read", "update"),
            Actions("candidates", "create", "read", "update"),
            Read("dashboard", "job_orders", "agents", "notifications")),

        [RoleNames.DocumentStaff] = Combine(
            Actions("candidates", "read", "update"),
            Read("dashboard", "leads", "job_orders", "visas", "notifications")),

        [RoleNames.VisaStaff] = Combine(
            Actions("candidates", "read", "update"),
            AllActions("visas"),
            AllActions("flights"),
            Read("dashboard", "job_orders", "notifications")),

        [RoleNames.Accountant] = Combine(
            AllActions("payments"),
            AllActions("expenses"),
            AllActions("receipts"),
            AllActions("commissions"),
            Read("dashboard", "candidates", "job_orders", "agents", "reports", "notifications")),

        [RoleNames.Agent] = Combine(
            Read("dashboard", "candidates", "commissions", "reports", "notifications")),
    };

    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var s = scope.ServiceProvider;
        var db = s.GetRequiredService<ApplicationDbContext>();
        var roleManager = s.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = s.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = s.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync();

        // 1) Roles
        foreach (var (name, desc) in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(name))
                await roleManager.CreateAsync(new ApplicationRole(name) { Description = desc });
        }

        // 2) Permissions
        var existing = await db.Permissions.Select(p => p.Name).ToListAsync();
        var toAdd = PermissionRegistry.All()
            .Where(p => !existing.Contains(p.Name))
            .Select(p => new Permission { Name = p.Name, Resource = p.Resource, Action = p.Action })
            .ToList();
        if (toAdd.Count > 0)
        {
            db.Permissions.AddRange(toAdd);
            await db.SaveChangesAsync();
        }

        // 3) Gán permissions cho 8 vai trò.
        var allPermissionNames = await db.Permissions.Select(p => p.Name).ToListAsync();
        await AssignRolePermissionsAsync(db, roleManager, logger, RoleNames.SuperAdmin, allPermissionNames);
        foreach (var (roleName, permissionNames) in RolePermissionMap)
            await AssignRolePermissionsAsync(db, roleManager, logger, roleName, permissionNames);

        // 4) Tài khoản mẫu cho từng vai trò.
        foreach (var seedUser in SeedUsers)
            await EnsureSeedUserAsync(userManager, logger, seedUser);
    }

    private static async Task AssignRolePermissionsAsync(
        ApplicationDbContext db,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger,
        string roleName,
        IEnumerable<string> permissionNames)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            logger.LogWarning("Không tìm thấy role để gán permission: {Role}", roleName);
            return;
        }

        var desired = permissionNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var permissions = await db.Permissions
            .Where(p => desired.Contains(p.Name))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        var missingNames = desired.Except(permissions.Select(p => p.Name), StringComparer.OrdinalIgnoreCase).ToList();
        if (missingNames.Count > 0)
            logger.LogWarning("Role {Role} có permission chưa tồn tại: {Permissions}", roleName, string.Join(", ", missingNames));

        var assigned = await db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var missing = permissions
            .Where(p => !assigned.Contains(p.Id))
            .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id })
            .ToList();

        if (missing.Count == 0) return;

        db.RolePermissions.AddRange(missing);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureSeedUserAsync(UserManager<ApplicationUser> userManager, ILogger logger, SeedUser seed)
    {
        var user = await userManager.FindByEmailAsync(seed.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = seed.Email,
                Email = seed.Email,
                EmailConfirmed = true,
                FullName = seed.FullName,
                IsActive = true,
            };

            var createResult = await userManager.CreateAsync(user, DefaultAdminPassword);
            if (!createResult.Succeeded)
            {
                logger.LogError("Tạo user mẫu {Email} thất bại: {Errors}",
                    seed.Email,
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Đã tạo user mẫu {Email} / {Role}", seed.Email, seed.Role);
        }
        else
        {
            var changed = false;
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                user.FullName = seed.FullName;
                changed = true;
            }
            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (changed)
                await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, seed.Role))
            await userManager.AddToRoleAsync(user, seed.Role);
    }

    private static string[] Read(params string[] resources)
        => resources.Select(resource => $"{resource}:read").ToArray();

    private static string[] Actions(string resource, params string[] actions)
        => actions.Select(action => $"{resource}:{action}").ToArray();

    private static string[] Crud(string resource)
        => Actions(resource, "create", "read", "update", "delete");

    private static string[] AllActions(string resource)
        => PermissionRegistry.Actions.Select(action => $"{resource}:{action}").ToArray();

    private static string[] Combine(params IEnumerable<string>[] groups)
        => groups.SelectMany(group => group).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private sealed record SeedUser(string Email, string FullName, string Role);
}
