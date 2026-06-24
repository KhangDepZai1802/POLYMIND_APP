using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Lịch sử chăm sóc / đổi trạng thái Lead.</summary>
public class LeadActivity : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = default!;
    public LeadActivityType ActivityType { get; set; }
    public string? Content { get; set; }
    public LeadStatus? OldStatus { get; set; }
    public LeadStatus? NewStatus { get; set; }
    public Guid CreatedBy { get; set; }
}
