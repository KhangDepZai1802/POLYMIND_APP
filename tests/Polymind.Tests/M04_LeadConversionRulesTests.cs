using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Domain.Leads;
using Xunit;

namespace Polymind.Tests;

/// <summary>Regression cho BUG_M04_01 / TC_M04_007-008.</summary>
public class M04_LeadConversionRulesTests
{
    [Fact]
    public void Converted_candidate_is_attributed_to_the_actor()
    {
        var actorId = Guid.NewGuid();
        var lead = Lead();

        var candidate = LeadConversionRules.CreateCandidate(lead, actorId, "UV-REGRESSION");

        Assert.Equal(actorId, candidate.CreatedBy);
        Assert.Equal(lead.Id, candidate.LeadId);
    }

    [Fact]
    public void Conversion_preserves_candidate_profile_and_assignment_fields()
    {
        var lead = Lead();

        var candidate = LeadConversionRules.CreateCandidate(lead, Guid.NewGuid(), "UV-REGRESSION");

        Assert.Equal(lead.FullName, candidate.FullName);
        Assert.Equal(lead.Phone, candidate.Phone);
        Assert.Equal(lead.Province, candidate.Province);
        Assert.Equal(lead.Address, candidate.Address);
        Assert.Equal(lead.Gender, candidate.Gender);
        Assert.Equal(lead.Dob, candidate.Dob);
        Assert.Equal(lead.Cccd, candidate.CccdNumber);
        Assert.Equal(lead.Email, candidate.Email);
        Assert.Equal(lead.Occupation, candidate.Occupation);
        Assert.Equal(lead.EducationLevel, candidate.EducationLevel);
        Assert.Equal(lead.WorkExperience, candidate.WorkExperience);
        Assert.Equal(lead.Languages, candidate.Languages);
        Assert.Equal(lead.AgentId, candidate.AgentId);
        Assert.Equal(lead.CollaboratorId, candidate.CollaboratorId);
        Assert.Equal(lead.AssignedTo, candidate.ConsultantId);
    }

    [Fact]
    public void Empty_actor_is_rejected()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            LeadConversionRules.CreateCandidate(Lead(), Guid.Empty, "UV-REGRESSION"));

        Assert.Equal("actorId", error.ParamName);
    }

    private static Lead Lead() => new()
    {
        Code = "LD-REGRESSION",
        FullName = "Regression lead",
        Phone = "0900000000",
        Province = "Hà Nội",
        Address = "Địa chỉ test",
        Gender = Gender.Female,
        Dob = new DateOnly(2000, 1, 2),
        Cccd = "001200000000",
        Email = "lead@example.test",
        Occupation = "Test",
        EducationLevel = "THPT",
        WorkExperience = "1 năm",
        Languages = "Tiếng Nhật",
        AgentId = Guid.NewGuid(),
        CollaboratorId = Guid.NewGuid(),
        AssignedTo = Guid.NewGuid(),
    };
}
