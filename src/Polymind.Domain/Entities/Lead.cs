using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Lead — khách hàng tiềm năng (Module 1 / mục 3 đặc tả).</summary>
public class Lead : BaseEntity
{
    public string Code { get; set; } = default!; // LD-YYYYMMDD-XXXX
    public string FullName { get; set; } = default!;
    public DateOnly? Dob { get; set; }
    public Gender? Gender { get; set; }
    public string? Cccd { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? Occupation { get; set; }
    public string? EducationLevel { get; set; }
    public string? WorkExperience { get; set; }
    public string? Languages { get; set; }
    public string? TargetCountry { get; set; }
    public Guid? InterestedJobOrderId { get; set; }
    public LeadSource Source { get; set; }
    public Guid? AgentId { get; set; }
    /// <summary>CTV trực tiếp giới thiệu lead này (chỉ khi Source = Referral). Hiển thị "CTV-&lt;tên&gt;" ở cột Nguồn.</summary>
    public Guid? CollaboratorId { get; set; }
    /// <summary>Tư vấn viên phụ trách lead (tài khoản role consultant). Lưu trong AssignedTo.</summary>
    public Guid? AssignedTo { get; set; }
    public DateTimeOffset? AppointmentAt { get; set; } // lịch hẹn tư vấn sắp tới
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string? Notes { get; set; }

    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
}
