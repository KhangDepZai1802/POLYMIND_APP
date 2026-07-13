using Polymind.Domain.Entities;

namespace Polymind.Domain.Messaging;

public static class MessagingCandidateScope
{
    /// <summary>
    /// Ứng viên mà <paramref name="userId"/> là TVV phụ trách — vai trò nhân sự duy nhất còn nằm
    /// trong quan hệ nhắn tin với Phụ huynh/Học viên. Nhân sự hồ sơ/visa/workflow và đại lý KHÔNG
    /// nằm trong quan hệ này nên không cần quét.
    /// </summary>
    public static IQueryable<Candidate> ForConsultant(IQueryable<Candidate> candidates, Guid userId)
        => candidates.Where(candidate => candidate.ConsultantId == userId);
}

/// <summary>
/// Quan hệ nhắn tin quanh một ứng viên. Phụ huynh/Học viên chỉ trao đổi với nhau và với đúng
/// CTV giới thiệu + TVV phụ trách ứng viên đó; chiều ngược lại cũng vậy (đối xứng, fail-closed).
/// Đại lý và nhân sự hồ sơ/visa/workflow KHÔNG thuộc quan hệ này.
/// </summary>
public sealed class CandidateMessagingRelationship
{
    private readonly HashSet<Guid> _portalUsers;
    private readonly HashSet<Guid> _counterparts;

    public CandidateMessagingRelationship(
        Guid? studentUserId,
        Guid? parentUserId,
        IEnumerable<Guid> counterpartUserIds)
    {
        _portalUsers = new[] { studentUserId, parentUserId }
            .OfType<Guid>()
            .ToHashSet();
        _counterparts = counterpartUserIds.ToHashSet();
    }

    /// <summary>Quan hệ của một ứng viên: chỉ TVV phụ trách và CTV giới thiệu đối thoại với Phụ huynh/Học viên.</summary>
    public static CandidateMessagingRelationship ForCandidate(
        Guid? studentUserId,
        Guid? parentUserId,
        Guid? consultantUserId,
        Guid? collaboratorUserId)
        => new(
            studentUserId,
            parentUserId,
            new[] { consultantUserId, collaboratorUserId }.OfType<Guid>());

    public HashSet<Guid> AllowedRecipientsFor(Guid userId)
    {
        var allowed = new HashSet<Guid>();
        if (_portalUsers.Contains(userId))
        {
            allowed.UnionWith(_portalUsers);
            allowed.UnionWith(_counterparts);
        }
        else if (_counterparts.Contains(userId))
        {
            allowed.UnionWith(_portalUsers);
        }

        allowed.Remove(userId);
        return allowed;
    }
}
