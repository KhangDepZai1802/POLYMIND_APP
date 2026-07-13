namespace Polymind.Domain.Notifications;

/// <summary>Recipient tài chính luôn gồm bộ phận tài chính và thêm người phụ trách ứng viên nếu có.</summary>
public static class FinancialNotificationRules
{
    /// <summary>U-M13-1: tài chính chỉ gửi Kế toán + super_admin, không gửi Director.</summary>
    public static IReadOnlyList<string> RecipientRoleNames { get; } = ["accountant", "super_admin"];

    public static List<Guid> Recipients(
        IEnumerable<Guid> financeRecipients,
        IEnumerable<Guid>? candidateOwners = null)
        => financeRecipients
            .Concat(candidateOwners ?? [])
            .Distinct()
            .ToList();
}
