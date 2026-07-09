using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Identity;

/// <summary>Thong tin pham vi du lieu cua tai khoan doi tac dang dang nhap.</summary>
/// <param name="IsAgentOnly">User chi thuoc role dai ly, khong kem role nhan su noi bo.</param>
/// <param name="AgentId">Dai ly tuong ung tai khoan; voi CTV la dai ly chu quan.</param>
/// <param name="AgentName">Ten dai ly tuong ung tai khoan; voi CTV la ten dai ly chu quan.</param>
/// <param name="IsCollaboratorOnly">User chi thuoc role CTV.</param>
/// <param name="CollaboratorId">CTV tuong ung tai khoan, null neu khong phai CTV hoac chua gan.</param>
/// <param name="CollaboratorName">Ten CTV tuong ung tai khoan, null neu khong phai CTV hoac chua gan.</param>
public readonly record struct AgentScopeInfo(
    bool IsAgentOnly,
    Guid? AgentId,
    string? AgentName,
    bool IsCollaboratorOnly,
    Guid? CollaboratorId,
    string? CollaboratorName,
    bool IsSelfScoped = false,
    Guid? OwnedCandidateId = null,
    bool IsParent = false,
    bool IsStudent = false,
    bool OwnedCandidateHasLoan = false)
{
    public bool IsPartnerOnly => IsAgentOnly || IsCollaboratorOnly;
}

/// <summary>
/// Giai quyet pham vi du lieu cho Portal doi tac: dai ly thay toan bo CTV/ung vien cua minh,
/// CTV chi thay ung vien do chinh CTV do gioi thieu va hoa hong duoc chia.
/// </summary>
public class AgentScope(
    AuthenticationStateProvider authStateProvider,
    IDbContextFactory<ApplicationDbContext> dbFactory)
{
    private static readonly string[] StaffRoles =
    {
        RoleNames.SuperAdmin, RoleNames.Director, RoleNames.RecruitmentManager, RoleNames.Recruiter,
        RoleNames.Consultant, RoleNames.DocumentStaff, RoleNames.VisaStaff, RoleNames.Accountant
    };

    private AgentScopeInfo? _cached;

    public async ValueTask<AgentScopeInfo> GetAsync()
    {
        if (_cached is { } cached) return cached;

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var hasStaffRole = StaffRoles.Any(user.IsInRole);
        var isAgentOnly = user.IsInRole(RoleNames.Agent) && !hasStaffRole;
        var isCollaboratorOnly = !isAgentOnly && user.IsInRole(RoleNames.Collaborator) && !hasStaffRole;
        // Phụ huynh / Học viên: chỉ được xem đúng ứng viên gắn với tài khoản (Candidate.OwnerUserId).
        var isStudent = user.IsInRole(RoleNames.Student);
        var isParent = user.IsInRole(RoleNames.Parent);
        var isSelfScoped = !isAgentOnly && !isCollaboratorOnly && !hasStaffRole
            && (isParent || isStudent);
        Guid? agentId = null;
        string? agentName = null;
        Guid? collaboratorId = null;
        string? collaboratorName = null;
        Guid? ownedCandidateId = null;
        var ownedHasLoan = false;

        if ((isAgentOnly || isCollaboratorOnly || isSelfScoped) && Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            if (isSelfScoped)
            {
                // Ứng viên gắn với tài khoản: Học sinh qua OwnerUserId, Phụ huynh qua ParentUserId.
                ownedCandidateId = await db.Candidates
                    .Where(c => c.OwnerUserId == userId || c.ParentUserId == userId)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync();
                if (ownedCandidateId is Guid cid)
                    ownedHasLoan = await db.Loans.AnyAsync(l => l.CandidateId == cid);
            }
            else if (isAgentOnly)
            {
                var agent = await db.Agents
                    .Where(a => a.UserId == userId)
                    .Select(a => new { a.Id, a.Name })
                    .FirstOrDefaultAsync();
                if (agent is not null)
                {
                    agentId = agent.Id;
                    agentName = agent.Name;
                }
            }
            else
            {
                var collaborator = await db.Collaborators
                    .Where(c => c.UserId == userId)
                    .Select(c => new { c.Id, c.FullName, c.AgentId, AgentName = c.Agent.Name })
                    .FirstOrDefaultAsync();
                if (collaborator is not null)
                {
                    collaboratorId = collaborator.Id;
                    collaboratorName = collaborator.FullName;
                    agentId = collaborator.AgentId;
                    agentName = collaborator.AgentName;
                }
            }
        }

        var info = new AgentScopeInfo(isAgentOnly, agentId, agentName, isCollaboratorOnly, collaboratorId, collaboratorName,
            isSelfScoped, ownedCandidateId, isParent && isSelfScoped, isStudent && isSelfScoped, ownedHasLoan);
        _cached = info;
        return info;
    }
}
