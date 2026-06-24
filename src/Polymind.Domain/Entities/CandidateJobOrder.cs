using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Bảng nối ứng viên ↔ đơn hàng; theo dõi bước workflow hiện tại.</summary>
public class CandidateJobOrder : BaseEntity
{
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = default!;
    public Guid JobOrderId { get; set; }
    public JobOrder JobOrder { get; set; } = default!;
    public WorkflowStep CurrentStep { get; set; } = WorkflowStep.Lead;
    public CandidateJobOrderStatus Status { get; set; } = CandidateJobOrderStatus.Active;
    public Guid? AssignedTo { get; set; }
    public string? Notes { get; set; }

    public ICollection<WorkflowStepRecord> Steps { get; set; } = new List<WorkflowStepRecord>();
}
