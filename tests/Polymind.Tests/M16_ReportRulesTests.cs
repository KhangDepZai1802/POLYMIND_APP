using Polymind.Domain.Reporting;
using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>Regression cho BUG_M16_01 + CR-M16-1.</summary>
public class M16_ReportRulesTests
{
    [Fact]
    public void Date_range_is_inclusive_and_serializes_for_export_links()
    {
        Assert.True(ReportDateRange.TryCreate(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            out var range));

        Assert.True(range.Includes(new DateOnly(2026, 7, 1)));
        Assert.True(range.Includes(new DateOnly(2026, 7, 31)));
        Assert.False(range.Includes(new DateOnly(2026, 6, 30)));
        Assert.Equal("?from=2026-07-01&to=2026-07-31", range.ToQueryString());
    }

    [Fact]
    public void Invalid_reversed_range_is_rejected()
    {
        Assert.False(ReportDateRange.TryCreate(
            new DateOnly(2026, 7, 31),
            new DateOnly(2026, 7, 1),
            out _));
    }

    [Fact]
    public void All_time_range_keeps_backward_compatible_url()
    {
        Assert.Equal(string.Empty, ReportDateRange.All.ToQueryString());
        Assert.True(ReportDateRange.All.Includes(DateOnly.MinValue));
        Assert.True(ReportDateRange.All.Includes(DateOnly.MaxValue));
    }

    [Fact]
    public void Recruitment_manager_can_export_recruitment_but_not_financial_slugs()
    {
        Assert.All(ReportAccessRules.RecruitmentSlugs,
            slug => Assert.True(ReportAccessRules.CanExport(slug, canReadRecruitment: true, canReadFinance: false)));
        Assert.All(ReportAccessRules.FinancialSlugs,
            slug => Assert.False(ReportAccessRules.CanExport(slug, canReadRecruitment: true, canReadFinance: false)));
    }

    [Fact]
    public void Finance_roles_can_export_all_known_slugs()
    {
        var all = ReportAccessRules.RecruitmentSlugs.Concat(ReportAccessRules.FinancialSlugs);

        Assert.All(all, slug => Assert.True(ReportAccessRules.CanExport(slug, true, true)));
    }

    [Fact]
    public void Financial_permission_is_registered_for_dynamic_policy_provider()
    {
        Assert.Contains("financial_reports", PermissionRegistry.Resources);
        Assert.Contains(PermissionRegistry.All(), permission => permission.Name == ReportAccessRules.FinancialPermission);
    }
}
