using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Tin nhắn nội bộ giữa hai tài khoản người dùng (nhắn tin theo role, có phân quyền ở tầng UI/service).</summary>
public class Message : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Body { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
