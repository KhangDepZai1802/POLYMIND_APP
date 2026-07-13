using Polymind.Domain.Entities;

namespace Polymind.Domain.Ai;

/// <summary>
/// Phạm vi dữ liệu được phép đưa vào ngữ cảnh AI. Tài khoản partner chưa được
/// gắn đại lý/CTV dùng <see cref="None"/> để fail-closed.
/// </summary>
public readonly record struct AiDataScope(AiDataScopeKind Kind, Guid? ScopeId = null)
{
    public static AiDataScope All { get; } = new(AiDataScopeKind.All);
    public static AiDataScope None { get; } = new(AiDataScopeKind.None);

    public static AiDataScope ForAgent(Guid agentId)
        => new(AiDataScopeKind.Agent, agentId);

    public static AiDataScope ForCollaborator(Guid collaboratorId)
        => new(AiDataScopeKind.Collaborator, collaboratorId);

    public IQueryable<Candidate> ApplyCandidates(IQueryable<Candidate> query) => Kind switch
    {
        AiDataScopeKind.All => query,
        AiDataScopeKind.Agent when ScopeId is Guid agentId
            => query.Where(candidate => candidate.AgentId == agentId),
        AiDataScopeKind.Collaborator when ScopeId is Guid collaboratorId
            => query.Where(candidate => candidate.CollaboratorId == collaboratorId),
        _ => query.Where(_ => false),
    };

    public IQueryable<Lead> ApplyLeads(IQueryable<Lead> query) => Kind switch
    {
        AiDataScopeKind.All => query,
        AiDataScopeKind.Agent when ScopeId is Guid agentId
            => query.Where(lead => lead.AgentId == agentId),
        AiDataScopeKind.Collaborator when ScopeId is Guid collaboratorId
            => query.Where(lead => lead.CollaboratorId == collaboratorId),
        _ => query.Where(_ => false),
    };

    public IQueryable<JobOrder> ApplyJobOrders(
        IQueryable<JobOrder> query,
        IQueryable<CandidateJobOrder> assignments) => Kind switch
    {
        AiDataScopeKind.All => query,
        AiDataScopeKind.Agent when ScopeId is Guid agentId
            => query.Where(job => assignments.Any(link =>
                link.JobOrderId == job.Id && link.Candidate.AgentId == agentId)),
        AiDataScopeKind.Collaborator when ScopeId is Guid collaboratorId
            => query.Where(job => assignments.Any(link =>
                link.JobOrderId == job.Id && link.Candidate.CollaboratorId == collaboratorId)),
        _ => query.Where(_ => false),
    };
}

public enum AiDataScopeKind
{
    None,
    All,
    Agent,
    Collaborator,
}
