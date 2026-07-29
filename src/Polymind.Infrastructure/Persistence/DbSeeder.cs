using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        // 5 tư vấn viên — mỗi người 1 tài khoản, role Tư vấn viên (theo sát ứng viên).
        new SeedUser("tuvan1@polymind.local", "Nguyễn Thị Thu Trang", RoleNames.Consultant, "0905110001"),
        new SeedUser("tuvan2@polymind.local", "Trần Minh Đức", RoleNames.Consultant, "0905110002"),
        new SeedUser("tuvan3@polymind.local", "Lê Thị Ngọc Ánh", RoleNames.Consultant, "0905110003"),
        new SeedUser("tuvan4@polymind.local", "Phạm Hoàng Nam", RoleNames.Consultant, "0905110004"),
        new SeedUser("tuvan5@polymind.local", "Vũ Thị Mỹ Linh", RoleNames.Consultant, "0905110005"),
        new SeedUser("document.staff@polymind.local", "Bộ phận hồ sơ", RoleNames.DocumentStaff),
        new SeedUser("visa.staff@polymind.local", "Bộ phận visa", RoleNames.VisaStaff),
        new SeedUser("accountant@polymind.local", "Kế toán", RoleNames.Accountant),
        new SeedUser("agent@polymind.local", "Đại lý demo", RoleNames.Agent),
    };

    private static readonly IReadOnlyDictionary<string, string[]> RolePermissionMap = new Dictionary<string, string[]>
    {
        [RoleNames.Director] = Combine(
            Read("dashboard", "leads", "candidates", "job_orders", "payments", "expenses", "receipts",
                "agents", "collaborators", "commissions", "loans", "visas", "flights", "reports", "financial_reports", "users", "roles", "notifications", "audit", "training"),
            Actions("commissions", "approve"),
            Actions("reports", "create", "read"),
            Messaging()),

        [RoleNames.RecruitmentManager] = Combine(
            Crud("leads"),
            Actions("candidates", "create", "read", "update"),
            Actions("collaborators", "create", "read", "update"),
            Actions("loans", "create", "read", "update"),
            Crud("training"),
            Read("dashboard", "job_orders", "agents", "reports", "notifications"),
            Messaging()),

        [RoleNames.Recruiter] = Combine(
            Actions("leads", "create", "read", "update"),
            Actions("candidates", "create", "read", "update"),
            Actions("loans", "create", "read", "update"),
            Read("dashboard", "job_orders", "agents", "collaborators", "notifications", "training"),
            Messaging()),

        // Tư vấn viên: theo sát lead + ứng viên mình phụ trách (quyền tương đương Nhân viên tuyển dụng).
        [RoleNames.Consultant] = Combine(
            Actions("leads", "create", "read", "update"),
            Actions("candidates", "create", "read", "update"),
            Actions("loans", "create", "read", "update"),
            Crud("training"),
            Read("dashboard", "job_orders", "agents", "collaborators", "notifications"),
            Messaging()),

        [RoleNames.DocumentStaff] = Combine(
            Actions("leads", "read", "update", "delete"),
            Actions("candidates", "read", "update", "delete"),
            Read("dashboard", "job_orders", "collaborators", "loans", "visas", "notifications", "training"),
            Messaging()),

        [RoleNames.VisaStaff] = Combine(
            Actions("candidates", "read", "update"),
            AllActions("visas"),
            AllActions("flights"),
            Read("dashboard", "job_orders", "collaborators", "loans", "notifications", "training"),
            Messaging()),

        [RoleNames.Accountant] = Combine(
            AllActions("payments"),
            AllActions("expenses"),
            AllActions("receipts"),
            AllActions("commissions"),
            AllActions("loans"),
            Read("dashboard", "candidates", "job_orders", "agents", "collaborators", "reports", "financial_reports", "notifications", "training"),
            Messaging()),

        [RoleNames.Agent] = Combine(
            Read("agents", "candidates", "collaborators", "commissions", "loans", "notifications", "training"),
            Actions("collaborators", "update"),
            Messaging()),

        // CTV: chỉ ứng viên mình giới thiệu + phần hoa hồng của chính mình.
        // KHÔNG có "training" (không xem module Đào tạo) và KHÔNG có "financial_reports"
        // (không xem doanh thu/hoa hồng của đại lý).
        [RoleNames.Collaborator] = Combine(
            Read("agents", "candidates", "commissions", "notifications"),
            Messaging()),

        // Phụ huynh: cổng cá nhân hóa — chỉ xem hồ sơ của con/em (bó hẹp qua Candidate.ParentUserId).
        // Bỏ đại lý & hoa hồng; thêm tài chính + hỗ trợ vay + tin nhắn.
        [RoleNames.Parent] = Combine(
            Read("candidates", "training", "payments", "loans", "notifications"),
            Messaging()),

        // Học viên: cổng cá nhân hóa — chỉ xem hồ sơ của chính mình (bó hẹp qua Candidate.OwnerUserId).
        [RoleNames.Student] = Combine(
            Read("candidates", "training", "payments", "loans", "notifications"),
            Messaging()),
    };

    /// <summary>Đọc hợp đồng seed quyền theo role cho kiểm tra/diagnostic; SuperAdmin được xử lý riêng bởi authorization layer.</summary>
    public static bool RoleHasPermission(string roleName, string permissionName)
        => RolePermissionMap.TryGetValue(roleName, out var permissions)
            && permissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);

    /// <summary>Quyền nhắn tin nội bộ: ai cũng đọc + gửi được (phân quyền người-nhận xử lý ở tầng service/UI).</summary>
    private static string[] Messaging() => new[] { "messages:read", "messages:create" };

    /// <summary>
    /// CHỈ môi trường Development mới được seed 13 tài khoản mẫu dùng chung mật khẩu
    /// <see cref="DefaultAdminPassword"/>. Production tạo duy nhất super admin thật từ biến môi trường.
    /// Tách thành hàm có tên để test chốt hợp đồng — đổi nhầm nhánh này là lộ mật khẩu mặc định ra production.
    /// </summary>
    public static bool ShouldSeedSampleUsers(bool isDevelopment) => isDevelopment;

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

        // 4) Tài khoản.
        var env = s.GetRequiredService<IHostEnvironment>();
        if (ShouldSeedSampleUsers(env.IsDevelopment()))
        {
            // Dev: seed đủ 8 tài khoản mẫu (mật khẩu chung Admin@123) cho demo/test RBAC.
            foreach (var seedUser in SeedUsers)
                await EnsureSeedUserAsync(userManager, logger, seedUser, DefaultAdminPassword);
        }
        else
        {
            // Production: TUYỆT ĐỐI không tạo tài khoản mẫu/mật khẩu Admin@123.
            // Chỉ tạo duy nhất 1 super admin thật từ biến môi trường (SuperAdmin__Email / SuperAdmin__Password).
            var config = s.GetRequiredService<IConfiguration>();
            var adminEmail = config["SuperAdmin:Email"];
            var adminPassword = config["SuperAdmin:Password"];
            var adminName = config["SuperAdmin:FullName"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogError(
                    "Production thiếu SuperAdmin:Email / SuperAdmin:Password (env SuperAdmin__Email, SuperAdmin__Password). " +
                    "KHÔNG tạo bất kỳ tài khoản nào (tránh lộ mật khẩu mặc định). " +
                    "Hãy đặt 2 biến này với mật khẩu mạnh rồi khởi động lại để tạo super admin thật.");
            }
            else
            {
                var superAdmin = new SeedUser(
                    adminEmail.Trim(),
                    string.IsNullOrWhiteSpace(adminName) ? "Super Admin" : adminName.Trim(),
                    RoleNames.SuperAdmin);
                await EnsureSeedUserAsync(userManager, logger, superAdmin, adminPassword);
            }
        }
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

        var desiredIds = permissions.Select(p => p.Id).ToHashSet();
        var extra = await db.RolePermissions
            .Where(rp => rp.RoleId == role.Id && !desiredIds.Contains(rp.PermissionId))
            .ToListAsync();

        if (missing.Count == 0 && extra.Count == 0) return;

        if (missing.Count > 0) db.RolePermissions.AddRange(missing);
        if (extra.Count > 0) db.RolePermissions.RemoveRange(extra);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureSeedUserAsync(UserManager<ApplicationUser> userManager, ILogger logger, SeedUser seed, string password)
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
                PhoneNumber = seed.Phone,
                IsActive = true,
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Tạo user {Email} thất bại: {Errors}",
                    seed.Email,
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Đã tạo user {Email} / {Role}", seed.Email, seed.Role);
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
            if (string.IsNullOrWhiteSpace(user.PhoneNumber) && !string.IsNullOrWhiteSpace(seed.Phone))
            {
                user.PhoneNumber = seed.Phone;
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

    private sealed record SeedUser(string Email, string FullName, string Role, string? Phone = null);
}
