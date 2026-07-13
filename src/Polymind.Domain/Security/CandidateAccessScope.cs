using Polymind.Domain.Entities;

namespace Polymind.Domain.Security;

/// <summary>
/// Phạm vi dữ liệu ứng viên đã được resolve cho một principal. Giá trị <see cref="None"/>
/// cố ý fail-closed khi tài khoản phạm vi hẹp chưa được gắn với đại lý/CTV/hồ sơ.
/// </summary>
public readonly record struct CandidateAccessScope(CandidateAccessScopeKind Kind, Guid? ScopeId = null)
{
    public static CandidateAccessScope All { get; } = new(CandidateAccessScopeKind.All);
    public static CandidateAccessScope None { get; } = new(CandidateAccessScopeKind.None);

    public static CandidateAccessScope ForAgent(Guid agentId)
        => new(CandidateAccessScopeKind.Agent, agentId);

    public static CandidateAccessScope ForCollaborator(Guid collaboratorId)
        => new(CandidateAccessScopeKind.Collaborator, collaboratorId);

    public static CandidateAccessScope ForUser(Guid userId)
        => new(CandidateAccessScopeKind.Self, userId);

    public IQueryable<Candidate> Apply(IQueryable<Candidate> query) => Kind switch
    {
        CandidateAccessScopeKind.All => query,
        CandidateAccessScopeKind.Agent when ScopeId is Guid agentId
            => query.Where(candidate => candidate.AgentId == agentId),
        CandidateAccessScopeKind.Collaborator when ScopeId is Guid collaboratorId
            => query.Where(candidate => candidate.CollaboratorId == collaboratorId),
        CandidateAccessScopeKind.Self when ScopeId is Guid userId
            => query.Where(candidate => candidate.OwnerUserId == userId || candidate.ParentUserId == userId),
        _ => query.Where(_ => false),
    };
}

public enum CandidateAccessScopeKind
{
    None,
    All,
    Agent,
    Collaborator,
    Self,
}
