using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Hồ sơ Visa của ứng viên (Module 6 / mục 9).</summary>
public class Visa : BaseEntity
{
    public Guid CandidateId { get; set; }
    public Guid JobOrderId { get; set; }
    public string? VisaType { get; set; }
    public string Country { get; set; } = default!;
    public DateOnly? SubmittedDate { get; set; }
    public DateOnly? InterviewDate { get; set; }
    public DateOnly? ResultDate { get; set; }
    public VisaStatus Status { get; set; } = VisaStatus.NotSubmitted;
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public Guid? HandledBy { get; set; }
}
