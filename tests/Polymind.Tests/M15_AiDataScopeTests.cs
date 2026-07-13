using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Ai;
using Polymind.Domain.Entities;
using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>Regression cho BUG_M15_01 / TC_M15_022 / TC_M15_023.</summary>
public class M15_AiDataScopeTests
{
    private readonly Guid _agentId = Guid.NewGuid();
    private readonly Guid _otherAgentId = Guid.NewGuid();
    private readonly Guid _collaboratorId = Guid.NewGuid();
    private readonly Guid _otherCollaboratorId = Guid.NewGuid();

    [Fact]
    public void Agent_scope_only_exposes_its_candidates_leads_and_linked_jobs()
    {
        var data = Data();
        var scope = AiDataScope.ForAgent(_agentId);

        var candidates = scope.ApplyCandidates(data.Candidates.AsQueryable()).ToList();
        var leads = scope.ApplyLeads(data.Leads.AsQueryable()).ToList();
        var jobs = scope.ApplyJobOrders(data.Jobs.AsQueryable(), data.Assignments.AsQueryable()).ToList();

        Assert.NotEmpty(candidates);
        Assert.All(candidates, candidate => Assert.Equal(_agentId, candidate.AgentId));
        Assert.NotEmpty(leads);
        Assert.All(leads, lead => Assert.Equal(_agentId, lead.AgentId));
        Assert.Equal(2, jobs.Count);
        Assert.Contains(jobs, job => job.Id == data.AgentJobId);
        Assert.Contains(jobs, job => job.Id == data.CollaboratorJobId);
    }

    [Fact]
    public void Collaborator_scope_only_exposes_direct_candidates_leads_and_linked_jobs()
    {
        var data = Data();
        var scope = AiDataScope.ForCollaborator(_collaboratorId);

        var candidates = scope.ApplyCandidates(data.Candidates.AsQueryable()).ToList();
        var leads = scope.ApplyLeads(data.Leads.AsQueryable()).ToList();
        var jobs = scope.ApplyJobOrders(data.Jobs.AsQueryable(), data.Assignments.AsQueryable()).ToList();

        Assert.Single(candidates);
        Assert.Equal(_collaboratorId, candidates[0].CollaboratorId);
        Assert.Single(leads);
        Assert.Equal(_collaboratorId, leads[0].CollaboratorId);
        Assert.Single(jobs);
        Assert.Equal(data.CollaboratorJobId, jobs[0].Id);
    }

    [Fact]
    public void Missing_partner_mapping_exposes_no_ai_data()
    {
        var data = Data();

        Assert.Empty(AiDataScope.None.ApplyCandidates(data.Candidates.AsQueryable()));
        Assert.Empty(AiDataScope.None.ApplyLeads(data.Leads.AsQueryable()));
        Assert.Empty(AiDataScope.None.ApplyJobOrders(data.Jobs.AsQueryable(), data.Assignments.AsQueryable()));
    }

    [Fact]
    public void Staff_scope_preserves_full_ai_context()
    {
        var data = Data();

        Assert.Equal(data.Candidates.Count, AiDataScope.All.ApplyCandidates(data.Candidates.AsQueryable()).Count());
        Assert.Equal(data.Leads.Count, AiDataScope.All.ApplyLeads(data.Leads.AsQueryable()).Count());
        Assert.Equal(data.Jobs.Count, AiDataScope.All.ApplyJobOrders(data.Jobs.AsQueryable(), data.Assignments.AsQueryable()).Count());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Partner_scope_queries_translate_for_postgresql(bool agentScope)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=translation_only;Username=test;Password=test")
            .Options;
        using var db = new ApplicationDbContext(options);
        var scope = agentScope
            ? AiDataScope.ForAgent(_agentId)
            : AiDataScope.ForCollaborator(_collaboratorId);

        var candidateSql = scope.ApplyCandidates(db.Candidates.AsNoTracking()).ToQueryString();
        var leadSql = scope.ApplyLeads(db.Leads.AsNoTracking()).ToQueryString();
        var jobSql = scope.ApplyJobOrders(
            db.JobOrders.AsNoTracking(),
            db.CandidateJobOrders.AsNoTracking()).ToQueryString();

        Assert.Contains("WHERE", candidateSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", leadSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", jobSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EXISTS", jobSql, StringComparison.OrdinalIgnoreCase);
    }

    private TestData Data()
    {
        var agentCandidate = NewCandidate(_agentId, _otherCollaboratorId);
        var collaboratorCandidate = NewCandidate(_agentId, _collaboratorId);
        var otherCandidate = NewCandidate(_otherAgentId, _otherCollaboratorId);
        var agentJob = NewJob();
        var collaboratorJob = NewJob();
        var otherJob = NewJob();

        return new TestData(
            [agentCandidate, collaboratorCandidate, otherCandidate],
            [
                NewLead(_agentId, _otherCollaboratorId),
                NewLead(_agentId, _collaboratorId),
                NewLead(_otherAgentId, _otherCollaboratorId),
            ],
            [agentJob, collaboratorJob, otherJob],
            [
                Link(agentCandidate, agentJob),
                Link(collaboratorCandidate, collaboratorJob),
                Link(otherCandidate, otherJob),
            ],
            agentJob.Id,
            collaboratorJob.Id);
    }

    private static Candidate NewCandidate(Guid agentId, Guid collaboratorId) => new()
    {
        Id = Guid.NewGuid(),
        Code = $"UV-{Guid.NewGuid():N}"[..16],
        FullName = "AI scope regression candidate",
        AgentId = agentId,
        CollaboratorId = collaboratorId,
        CreatedBy = Guid.NewGuid(),
    };

    private static Lead NewLead(Guid agentId, Guid collaboratorId) => new()
    {
        Id = Guid.NewGuid(),
        Code = $"LD-{Guid.NewGuid():N}"[..16],
        FullName = "AI scope regression lead",
        AgentId = agentId,
        CollaboratorId = collaboratorId,
    };

    private static JobOrder NewJob() => new()
    {
        Id = Guid.NewGuid(),
        Code = $"JO-{Guid.NewGuid():N}"[..16],
        Country = "Japan",
        CreatedBy = Guid.NewGuid(),
    };

    private static CandidateJobOrder Link(Candidate candidate, JobOrder job) => new()
    {
        Id = Guid.NewGuid(),
        CandidateId = candidate.Id,
        Candidate = candidate,
        JobOrderId = job.Id,
        JobOrder = job,
    };

    private sealed record TestData(
        List<Candidate> Candidates,
        List<Lead> Leads,
        List<JobOrder> Jobs,
        List<CandidateJobOrder> Assignments,
        Guid AgentJobId,
        Guid CollaboratorJobId);
}
