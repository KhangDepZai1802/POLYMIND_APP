using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;

namespace Polymind.Web.Notifications;

public sealed record NotificationSendResult(bool Success, string? Message = null);

public interface INotificationSender
{
    NotificationChannel Channel { get; }
    Task<NotificationSendResult> SendAsync(Notification notification, CancellationToken cancellationToken = default);
}

public sealed class NotificationOptions
{
    public EmailNotificationOptions Email { get; set; } = new();
    public ChannelFallbackOptions Sms { get; set; } = new();
    public ChannelFallbackOptions Zalo { get; set; } = new();
}

public sealed class EmailNotificationOptions
{
    public bool Enabled { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "no-reply@polymind.local";
    public string FromName { get; set; } = "POLYMIND";
}

public sealed class ChannelFallbackOptions
{
    public bool Enabled { get; set; }
}

public sealed class InAppNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    public Task<NotificationSendResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
        => Task.FromResult(new NotificationSendResult(true, "In-app ready"));
}

public sealed class SmtpEmailNotificationSender(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IOptions<NotificationOptions> options,
    ILogger<SmtpEmailNotificationSender> logger) : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<NotificationSendResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var emailOptions = options.Value.Email;
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var user = await db.Users.Where(u => u.Id == notification.UserId)
            .Select(u => new { u.Email, u.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (user?.Email is null)
            return new NotificationSendResult(false, "Recipient email not found");

        if (!emailOptions.Enabled || string.IsNullOrWhiteSpace(emailOptions.Host))
        {
            logger.LogInformation("Email notification queued/logged for {Email}: {Title}", user.Email, notification.Title);
            return new NotificationSendResult(true, "SMTP disabled; logged as queued");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.FromEmail, emailOptions.FromName),
            Subject = notification.Title,
            Body = notification.Body ?? notification.Title,
            IsBodyHtml = false,
        };
        message.To.Add(new MailAddress(user.Email, user.FullName));

        using var client = new SmtpClient(emailOptions.Host, emailOptions.Port)
        {
            EnableSsl = emailOptions.UseSsl,
        };
        if (!string.IsNullOrWhiteSpace(emailOptions.Username))
            client.Credentials = new NetworkCredential(emailOptions.Username, emailOptions.Password);

        await client.SendMailAsync(message, cancellationToken);
        return new NotificationSendResult(true, "Email sent");
    }
}

public sealed class LoggingSmsNotificationSender(
    IOptions<NotificationOptions> options,
    ILogger<LoggingSmsNotificationSender> logger) : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public Task<NotificationSendResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("SMS {Mode} for user {UserId}: {Title}",
            options.Value.Sms.Enabled ? "queued" : "logged", notification.UserId, notification.Title);
        return Task.FromResult(new NotificationSendResult(true, "SMS adapter logged/queued"));
    }
}

public sealed class LoggingZaloNotificationSender(
    IOptions<NotificationOptions> options,
    ILogger<LoggingZaloNotificationSender> logger) : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Zalo;

    public Task<NotificationSendResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Zalo {Mode} for user {UserId}: {Title}",
            options.Value.Zalo.Enabled ? "queued" : "logged", notification.UserId, notification.Title);
        return Task.FromResult(new NotificationSendResult(true, "Zalo adapter logged/queued"));
    }
}
