namespace Polymind.Domain.Common;

/// <summary>
/// Base cho mọi entity nghiệp vụ: khóa chính UUID + dấu thời gian.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
