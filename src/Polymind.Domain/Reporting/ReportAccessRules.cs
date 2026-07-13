namespace Polymind.Domain.Reporting;

public readonly record struct ReportDateRange(DateOnly? From, DateOnly? To)
{
    public static ReportDateRange All { get; } = new(null, null);

    public static bool TryCreate(DateOnly? from, DateOnly? to, out ReportDateRange range)
    {
        range = new ReportDateRange(from, to);
        return from is null || to is null || from <= to;
    }

    public bool Includes(DateOnly date)
        => (From is null || date >= From.Value) && (To is null || date <= To.Value);

    public bool Includes(DateTimeOffset dateTime)
        => Includes(DateOnly.FromDateTime(dateTime.UtcDateTime));

    public string ToQueryString()
    {
        var parts = new List<string>();
        if (From is DateOnly from) parts.Add($"from={from:yyyy-MM-dd}");
        if (To is DateOnly to) parts.Add($"to={to:yyyy-MM-dd}");
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}

public static class ReportAccessRules
{
    public const string RecruitmentPermission = "reports:read";
    public const string FinancialPermission = "financial_reports:read";

    public static IReadOnlySet<string> RecruitmentSlugs { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "lead-by-province",
        "recruitment-funnel",
    };

    public static IReadOnlySet<string> FinancialSlugs { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "finance-monthly",
        "commissions",
        "overdue-payments",
        "revenue-by-country",
        "revenue-by-job-order",
        "top-agents",
    };

    public static bool RequiresFinancialPermission(string slug)
        => FinancialSlugs.Contains(slug);

    public static bool CanExport(string slug, bool canReadRecruitment, bool canReadFinance)
        => RecruitmentSlugs.Contains(slug)
            ? canReadRecruitment
            : FinancialSlugs.Contains(slug) && canReadRecruitment && canReadFinance;
}
