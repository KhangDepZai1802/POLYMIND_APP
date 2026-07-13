using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M20 — Security &amp; Deployment. Chốt các bất biến chống leo thang quyền dọc (vertical escalation)
/// ở tầng seed RBAC — nơi DUY NHẤT có thể kiểm bằng test project (không ref Web).
/// LƯU Ý PHẠM VI: security headers, cookie/JWT config, rate limit, ForwardedHeaders, Swagger gating
/// nằm ở Polymind.Web/Program.cs → xác minh bằng phân tích tĩnh (xem 05-automation-report.md).
/// TC_M20_012..018.
/// </summary>
public class M20_SecurityInvariantsTests
{
    // Partner (đại lý/CTV) và Portal (phụ huynh/học viên) TUYỆT ĐỐI không được có quyền
    // ghi tài chính / quản trị người dùng / role / audit / hoa hồng.
    public static readonly string[] SensitiveMutations =
    {
        "payments:create", "payments:update", "payments:delete", "payments:approve",
        "expenses:create", "expenses:update", "expenses:delete", "expenses:approve",
        "receipts:create", "receipts:update", "receipts:delete",
        "commissions:create", "commissions:update", "commissions:delete", "commissions:approve",
        "users:create", "users:update", "users:delete",
        "roles:create", "roles:update", "roles:delete",
        "audit:create", "audit:update", "audit:delete",
        "financial_reports:read",
    };

    [Theory] // TC_M20_012 — partner/portal không có quyền nhạy cảm nào
    [InlineData(RoleNames.Agent)]
    [InlineData(RoleNames.Collaborator)]
    [InlineData(RoleNames.Parent)]
    [InlineData(RoleNames.Student)]
    public void Partner_and_portal_roles_have_no_sensitive_mutation_permissions(string roleName)
    {
        foreach (var permission in SensitiveMutations)
            Assert.False(
                DbSeeder.RoleHasPermission(roleName, permission),
                $"{roleName} KHÔNG được có quyền nhạy cảm '{permission}'");
    }

    [Theory] // TC_M20_013 — chỉ role tài chính/quản trị mới có financial_reports:read
    [InlineData(RoleNames.Recruiter)]
    [InlineData(RoleNames.RecruitmentManager)]
    [InlineData(RoleNames.Consultant)]
    [InlineData(RoleNames.DocumentStaff)]
    [InlineData(RoleNames.VisaStaff)]
    public void Non_finance_staff_cannot_read_financial_reports(string roleName)
        => Assert.False(DbSeeder.RoleHasPermission(roleName, "financial_reports:read"));

    [Theory] // TC_M20_014 — role tài chính giữ financial_reports:read (không regressive)
    [InlineData(RoleNames.Director)]
    [InlineData(RoleNames.Accountant)]
    public void Finance_roles_keep_financial_reports_read(string roleName)
        => Assert.True(DbSeeder.RoleHasPermission(roleName, "financial_reports:read"));

    [Theory] // TC_M20_015 — quản trị người dùng/role chỉ dành cho Director (SuperAdmin bypass ở layer khác)
    [InlineData(RoleNames.Recruiter)]
    [InlineData(RoleNames.RecruitmentManager)]
    [InlineData(RoleNames.Accountant)]
    [InlineData(RoleNames.Agent)]
    public void User_and_role_administration_not_granted_to_non_admin_roles(string roleName)
    {
        Assert.False(DbSeeder.RoleHasPermission(roleName, "users:update"));
        Assert.False(DbSeeder.RoleHasPermission(roleName, "roles:update"));
    }

    [Fact] // TC_M20_016 — role không tồn tại → không có quyền (fail-closed)
    public void Unknown_role_has_no_permissions()
        => Assert.False(DbSeeder.RoleHasPermission("nonexistent_role", "candidates:read"));

    [Theory] // CR-M09-3 — doanh thu là số tài chính: đối tác (đại lý/CTV) không có financial_reports:read
    [InlineData(RoleNames.Agent)]
    [InlineData(RoleNames.Collaborator)]
    [InlineData(RoleNames.Parent)]
    [InlineData(RoleNames.Student)]
    public void Partner_and_portal_roles_cannot_read_financial_reports(string roleName)
        => Assert.False(DbSeeder.RoleHasPermission(roleName, "financial_reports:read"));

    [Fact] // CR-M08-2 — CTV KHÔNG được xem module Đào tạo (user chốt 2026-07-13)
    public void Collaborator_cannot_read_training()
        => Assert.False(DbSeeder.RoleHasPermission(RoleNames.Collaborator, "training:read"));

    [Theory] // CR-M08-2 — thu hẹp CTV không được làm mất training:read của các role đã chốt ở U-M08-1
    [InlineData(RoleNames.Recruiter)]
    [InlineData(RoleNames.DocumentStaff)]
    [InlineData(RoleNames.VisaStaff)]
    [InlineData(RoleNames.Accountant)]
    [InlineData(RoleNames.Agent)]
    public void Training_read_kept_for_roles_confirmed_earlier(string roleName)
        => Assert.True(DbSeeder.RoleHasPermission(roleName, "training:read"));

    [Fact] // CR-M08-2 — CTV giữ nguyên các quyền còn lại (không cắt nhầm)
    public void Collaborator_keeps_own_scope_permissions()
    {
        Assert.True(DbSeeder.RoleHasPermission(RoleNames.Collaborator, "candidates:read"));
        Assert.True(DbSeeder.RoleHasPermission(RoleNames.Collaborator, "commissions:read"));
        Assert.True(DbSeeder.RoleHasPermission(RoleNames.Collaborator, "messages:read"));
        Assert.True(DbSeeder.RoleHasPermission(RoleNames.Collaborator, "messages:create"));
    }
}
