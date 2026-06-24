using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Ứng viên (Module 2 / mục 4).</summary>
public class Candidate : BaseEntity
{
    public string Code { get; set; } = default!; // UV-YYYYMMDD-XXXX
    public Guid? LeadId { get; set; }
    public string FullName { get; set; } = default!;
    public string? CccdNumber { get; set; }
    public DateOnly? CccdIssueDate { get; set; }
    public string? CccdIssuePlace { get; set; }
    public string? PassportNumber { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public DateOnly? Dob { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public MaritalStatus? MaritalStatus { get; set; }
    public string? Phone { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public Guid? AgentId { get; set; }
    public Guid CreatedBy { get; set; }

    public ICollection<CandidateDocument> Documents { get; set; } = new List<CandidateDocument>();
    public ICollection<CandidateJobOrder> JobOrders { get; set; } = new List<CandidateJobOrder>();
}
