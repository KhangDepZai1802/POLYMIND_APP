using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Domain.Notifications;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M13 — Notifications. Pin hợp đồng Domain mà NotificationService + Labels + RB-6/RB-7 dựa vào.
/// Logic điều phối (ResolveTargetUrlAsync RB-6, ChannelsFor, PersistEventsAsync dedup/revive,
/// recipient routing RB-7) nằm trong Polymind.Web → cần integration/UI harness, không unit-test
/// được từ test project (không ref Web). Các test dưới chốt enum/default/nullable contract.
/// TC_M13_022,036,037,038,039,040.
/// </summary>
public class M13_NotificationRulesTests
{
    [Fact] // TC_M13_037 — NotificationType đủ 11 giá trị (gồm vòng đời hoa hồng RB-7)
    public void NotificationType_contains_all_expected_values()
    {
        var all = Enum.GetValues<NotificationType>();

        Assert.Contains(NotificationType.ReminderDocument, all);
        Assert.Contains(NotificationType.ReminderPayment, all);
        Assert.Contains(NotificationType.ReminderInterview, all);
        Assert.Contains(NotificationType.ReminderVisa, all);
        Assert.Contains(NotificationType.ReminderDeparture, all);
        Assert.Contains(NotificationType.CommissionPayment, all);
        Assert.Contains(NotificationType.ReminderLeadCare, all);
        // RB-7 bổ sung — nếu thiếu, reminder tài chính/hoa hồng tương ứng không phát được.
        Assert.Contains(NotificationType.ReminderLoanRepayment, all);
        Assert.Contains(NotificationType.ExpenseApproval, all);
        Assert.Contains(NotificationType.CommissionPending, all);
        Assert.Contains(NotificationType.CommissionPaid, all);
        Assert.Equal(11, all.Length);
    }

    [Fact] // TC_M13_038 — NotificationChannel đủ 4 kênh
    public void NotificationChannel_contains_four_channels()
    {
        var all = Enum.GetValues<NotificationChannel>();

        Assert.Contains(NotificationChannel.InApp, all);
        Assert.Contains(NotificationChannel.Email, all);
        Assert.Contains(NotificationChannel.Sms, all);
        Assert.Contains(NotificationChannel.Zalo, all);
        Assert.Equal(4, all.Length);
    }

    [Fact] // TC_M13_039 — Notification mới mặc định chưa đọc/chưa gửi
    public void New_notification_defaults_to_unread_unsent()
    {
        var n = new Notification();

        Assert.False(n.IsRead);
        Assert.Null(n.SentAt);
        Assert.Null(n.ReadAt);
    }

    [Fact] // TC_M13_040/022 — Preference mặc định chỉ bật In-app
    public void New_preference_defaults_to_inapp_only()
    {
        var pref = new NotificationPreference();

        Assert.True(pref.InAppEnabled);
        Assert.False(pref.EmailEnabled);
        Assert.False(pref.SmsEnabled);
        Assert.False(pref.ZaloEnabled);
    }

    [Fact] // TC_M13_036 — reference tùy chọn (RB-6 điều hướng cần cả 2, thiếu thì không điều hướng)
    public void New_notification_has_no_reference_by_default()
    {
        var n = new Notification();

        Assert.Null(n.ReferenceType);
        Assert.Null(n.ReferenceId);
    }

    [Theory] // BUG_M13_01 — phát sinh → chờ chi → đã chi có type riêng
    [InlineData(CommissionStatus.Pending, NotificationType.CommissionPending)]
    [InlineData(CommissionStatus.Approved, NotificationType.CommissionPayment)]
    [InlineData(CommissionStatus.Paid, NotificationType.CommissionPaid)]
    public void Commission_lifecycle_maps_to_notification_type(CommissionStatus status, NotificationType expected)
    {
        Assert.Equal(expected, CommissionNotificationRules.TypeFor(status));
    }

    [Fact] // BUG_M13_01 — đại lý sở hữu commission được thêm, null account được bỏ an toàn
    public void Commission_recipients_include_agent_account_without_duplicates()
    {
        var accountant = Guid.NewGuid();
        var agentUser = Guid.NewGuid();

        var recipients = CommissionNotificationRules.Recipients([accountant, accountant], agentUser);
        var withoutAgentAccount = CommissionNotificationRules.Recipients([accountant], null);

        Assert.Equal(2, recipients.Count);
        Assert.Contains(accountant, recipients);
        Assert.Contains(agentUser, recipients);
        Assert.Equal(new[] { accountant }, withoutAgentAccount);
    }

    [Fact] // U-M13-1 — tài chính luôn tới finance roles và thêm owner ứng viên, không thay thế nhau
    public void Financial_recipients_union_finance_roles_and_candidate_owners()
    {
        var accountant = Guid.NewGuid();
        var superAdmin = Guid.NewGuid();
        var owner = Guid.NewGuid();

        var recipients = FinancialNotificationRules.Recipients(
            [accountant, superAdmin],
            [owner, accountant]);

        Assert.Equal(3, recipients.Count);
        Assert.Contains(accountant, recipients);
        Assert.Contains(superAdmin, recipients);
        Assert.Contains(owner, recipients);
    }

    [Fact] // U-M13-1 — nguồn không gắn ứng viên vẫn gửi đủ finance roles
    public void Financial_recipients_keep_finance_roles_when_candidate_has_no_owner()
    {
        var finance = new[] { Guid.NewGuid(), Guid.NewGuid() };

        Assert.Equal(finance, FinancialNotificationRules.Recipients(finance));
    }

    [Fact] // CR-M13-1 — Director không còn là recipient tài chính
    public void Financial_recipient_roles_are_accountant_and_super_admin_only()
    {
        Assert.Equal(new[] { "accountant", "super_admin" }, FinancialNotificationRules.RecipientRoleNames);
        Assert.DoesNotContain("director", FinancialNotificationRules.RecipientRoleNames);
    }

    [Theory] // BUG_M13_01 / U-M13-2 — CTV chỉ thấy phần share của mình ở đủ 3 lifecycle
    [InlineData(CommissionStatus.Pending)]
    [InlineData(CommissionStatus.Approved)]
    [InlineData(CommissionStatus.Paid)]
    public void Collaborator_notification_contains_share_but_not_agent_total(CommissionStatus status)
    {
        const decimal agentTotal = 1_000_000m;
        var share = CommissionNotificationRules.CollaboratorShareAmount(agentTotal, 35m);

        var text = CommissionNotificationRules.CollaboratorTextFor(
            status,
            "Ứng viên trực tiếp",
            share,
            new DateOnly(2026, 7, 11));

        Assert.Equal(350_000m, share);
        Assert.Contains($"{share:N0}", text.Body);
        Assert.DoesNotContain($"{agentTotal:N0}", text.Title);
        Assert.DoesNotContain($"{agentTotal:N0}", text.Body);
    }
}
