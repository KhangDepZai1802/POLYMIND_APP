using Polymind.Domain.Entities;

namespace Polymind.Domain.Leads;

/// <summary>Ánh xạ dữ liệu khi chuyển Lead thành Candidate.</summary>
public static class LeadConversionRules
{
    public static Candidate CreateCandidate(Lead lead, Guid actorId, string candidateCode)
    {
        ArgumentNullException.ThrowIfNull(lead);
        if (actorId == Guid.Empty)
            throw new ArgumentException("Actor thực hiện chuyển Lead là bắt buộc.", nameof(actorId));
        if (string.IsNullOrWhiteSpace(candidateCode))
            throw new ArgumentException("Mã ứng viên là bắt buộc.", nameof(candidateCode));

        return new Candidate
        {
            Code = candidateCode,
            LeadId = lead.Id,
            FullName = lead.FullName,
            Phone = lead.Phone,
            Province = lead.Province,
            Address = lead.Address,
            Gender = lead.Gender,
            Dob = lead.Dob,
            CccdNumber = lead.Cccd,
            Email = lead.Email,
            Occupation = lead.Occupation,
            EducationLevel = lead.EducationLevel,
            WorkExperience = lead.WorkExperience,
            Languages = lead.Languages,
            AgentId = lead.AgentId,
            CollaboratorId = lead.CollaboratorId,
            ConsultantId = lead.AssignedTo,
            CreatedBy = actorId,
        };
    }
}
