namespace Polymind.Web.Notifications;

public sealed class NotificationJob(NotificationService notificationService, ILogger<NotificationJob> logger)
{
    public async Task RunAsync()
    {
        var created = await notificationService.GenerateRemindersForAllUsersAsync();
        var sent = await notificationService.SendPendingAsync();
        logger.LogInformation("Notification job done. Created={Created}, Sent={Sent}", created, sent);
    }

    public Task SendPendingAsync() => notificationService.SendPendingAsync();
}
