using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Nhật ký kiểm toán mọi thao tác CRUD/đăng nhập (yêu cầu phi chức năng).</summary>
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = default!; // create/update/delete/approve/login/logout
    public string Resource { get; set; } = default!;
    public Guid? ResourceId { get; set; }
    public string? OldValue { get; set; } // JSONB
    public string? NewValue { get; set; } // JSONB
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
