using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Lịch sử tiến độ từng bước workflow của một (ứng viên, đơn hàng).</summary>
public class WorkflowStepRecord : BaseEntity
{
    public Guid CandidateJobOrderId { get; set; }
    public CandidateJobOrder CandidateJobOrder { get; set; } = default!;
    public WorkflowStep Step { get; set; }
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;
    public Guid? AssignedTo { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
}
