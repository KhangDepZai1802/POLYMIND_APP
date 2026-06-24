using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Thông báo tự động (Module 8 / mục 13).</summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; } // người nhận
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string? Body { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsRead { get; set; }
    public string? ReferenceType { get; set; } // candidate/lead/agent
    public Guid? ReferenceId { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
