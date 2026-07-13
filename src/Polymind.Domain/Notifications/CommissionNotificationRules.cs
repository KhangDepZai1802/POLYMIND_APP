using Polymind.Domain.Enums;

namespace Polymind.Domain.Notifications;

/// <summary>Ánh xạ vòng đời hoa hồng sang notification và recipient chắc chắn theo RB-7.</summary>
public static class CommissionNotificationRules
{
    public static NotificationType? TypeFor(CommissionStatus status) => status switch
    {
        CommissionStatus.Pending => NotificationType.CommissionPending,
        CommissionStatus.Approved => NotificationType.CommissionPayment,
        CommissionStatus.Paid => NotificationType.CommissionPaid,
        _ => null,
    };

    public static List<Guid> Recipients(IEnumerable<Guid> financeRecipients, Guid? agentUserId)
        => agentUserId is Guid userId
            ? financeRecipients.Append(userId).Distinct().ToList()
            : financeRecipients.Distinct().ToList();

    /// <summary>Dùng cùng công thức làm tròn đang hiển thị ở cổng /my-commissions.</summary>
    public static decimal CollaboratorShareAmount(decimal commissionAmount, decimal sharePercentage)
        => Math.Round(
            commissionAmount * sharePercentage / 100m,
            0,
            MidpointRounding.AwayFromZero);

    /// <summary>
    /// Nội dung riêng cho CTV trực tiếp: chỉ nêu phần share của họ, tuyệt đối không đưa tổng
    /// CommissionAmount của đại lý vào title/body.
    /// </summary>
    public static CommissionNotificationText CollaboratorTextFor(
        CommissionStatus status,
        string candidateName,
        decimal collaboratorShareAmount,
        DateOnly? paidDate = null) => status switch
    {
        CommissionStatus.Pending => new(
            $"Hoa hồng của bạn chờ duyệt: {candidateName}",
            $"Phần hoa hồng của bạn {collaboratorShareAmount:N0} đ vừa phát sinh, đang chờ duyệt."),
        CommissionStatus.Approved => new(
            $"Hoa hồng của bạn chờ chi: {candidateName}",
            $"Phần hoa hồng của bạn {collaboratorShareAmount:N0} đ đã được duyệt, đang chờ thanh toán."),
        CommissionStatus.Paid => new(
            $"Hoa hồng của bạn đã chi: {candidateName}",
            $"Phần hoa hồng của bạn {collaboratorShareAmount:N0} đ đã được thanh toán{(paidDate is DateOnly paid ? $" ngày {paid:dd/MM/yyyy}" : string.Empty)}."),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Trạng thái không phát notification hoa hồng."),
    };
}

public sealed record CommissionNotificationText(string Title, string Body);
