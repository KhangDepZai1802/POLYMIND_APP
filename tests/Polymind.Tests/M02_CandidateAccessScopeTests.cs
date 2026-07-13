using Polymind.Domain.Entities;
using Polymind.Domain.Security;
using Xunit;

namespace Polymind.Tests;

/// <summary>Regression cho BUG_M02_02 / TC_M02_022.</summary>
public class M02_CandidateAccessScopeTests
{
    private readonly Guid _agentId = Guid.NewGuid();
    private readonly Guid _otherAgentId = Guid.NewGuid();
    private readonly Guid _collaboratorId = Guid.NewGuid();
    private readonly Guid _otherCollaboratorId = Guid.NewGuid();
    private readonly Guid _studentUserId = Guid.NewGuid();
    private readonly Guid _parentUserId = Guid.NewGuid();

    [Fact]
    public void Staff_scope_can_read_all_candidates()
    {
        var candidates = Candidates();

        var result = CandidateAccessScope.All.Apply(candidates.AsQueryable()).ToList();

        Assert.Equal(candidates.Count, result.Count);
    }

    [Fact]
    public void Agent_scope_only_reads_candidates_of_that_agent()
    {
        var result = CandidateAccessScope.ForAgent(_agentId)
            .Apply(Candidates().AsQueryable()).ToList();

        Assert.NotEmpty(result);
        Assert.All(result, candidate => Assert.Equal(_agentId, candidate.AgentId));
    }

    [Fact]
    public void Collaborator_scope_only_reads_candidates_introduced_by_that_collaborator()
    {
        var result = CandidateAccessScope.ForCollaborator(_collaboratorId)
            .Apply(Candidates().AsQueryable()).ToList();

        Assert.NotEmpty(result);
        Assert.All(result, candidate => Assert.Equal(_collaboratorId, candidate.CollaboratorId));
    }

    [Fact]
    public void Self_scope_reads_candidates_linked_as_student_or_parent()
    {
        var candidates = Candidates().AsQueryable();

        var studentResult = CandidateAccessScope.ForUser(_studentUserId).Apply(candidates).ToList();
        var parentResult = CandidateAccessScope.ForUser(_parentUserId).Apply(candidates).ToList();

        Assert.Single(studentResult);
        Assert.Equal(_studentUserId, studentResult[0].OwnerUserId);
        Assert.Single(parentResult);
        Assert.Equal(_parentUserId, parentResult[0].ParentUserId);
    }

    [Fact]
    public void Missing_scope_mapping_fails_closed()
    {
        var result = CandidateAccessScope.None.Apply(Candidates().AsQueryable()).ToList();

        Assert.Empty(result);
    }

    private List<Candidate> Candidates() =>
    [
        NewCandidate(agentId: _agentId, collaboratorId: _collaboratorId),
        NewCandidate(agentId: _agentId, collaboratorId: _otherCollaboratorId),
        NewCandidate(agentId: _otherAgentId, collaboratorId: _otherCollaboratorId),
        NewCandidate(ownerUserId: _studentUserId),
        NewCandidate(parentUserId: _parentUserId),
        NewCandidate(),
    ];

    private static Candidate NewCandidate(
        Guid? agentId = null,
        Guid? collaboratorId = null,
        Guid? ownerUserId = null,
        Guid? parentUserId = null) => new()
    {
        Id = Guid.NewGuid(),
        Code = $"UV-{Guid.NewGuid():N}"[..16],
        FullName = "Regression candidate",
        AgentId = agentId,
        CollaboratorId = collaboratorId,
        OwnerUserId = ownerUserId,
        ParentUserId = parentUserId,
        CreatedBy = Guid.NewGuid(),
    };
}
