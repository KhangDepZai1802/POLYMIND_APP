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
    // Thông tin nhạy cảm — chỉ super_admin xem/sửa (đồng bộ từ Lead khi convert).
    public string? Email { get; set; }
    public string? Occupation { get; set; }
    public string? EducationLevel { get; set; }
    public string? WorkExperience { get; set; }
    public string? Languages { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    // Cha/Mẹ/Người bảo hộ — nền cho cổng phụ huynh sau này (NHÓM 1.3). Tách khỏi "liên hệ khẩn cấp".
    public string? GuardianName { get; set; }
    public string? GuardianRelation { get; set; }
    public string? GuardianPhone { get; set; }
    public string? GuardianCccd { get; set; }
    public string? GuardianAddress { get; set; }
    public string? GuardianOccupation { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public Guid? AgentId { get; set; }
    public Guid? CollaboratorId { get; set; } // CTV trực tiếp giới thiệu (thuộc đại lý AgentId) — chỉ ứng viên do CTV giới thiệu
    public Guid? ConsultantId { get; set; } // Tư vấn viên theo sát ứng viên (tài khoản role consultant) — 1 TVV : nhiều ứng viên
    public Guid CreatedBy { get; set; }

    public ICollection<CandidateDocument> Documents { get; set; } = new List<CandidateDocument>();
    public ICollection<CandidateJobOrder> JobOrders { get; set; } = new List<CandidateJobOrder>();
}
