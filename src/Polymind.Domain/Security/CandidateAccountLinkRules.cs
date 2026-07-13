using Polymind.Domain.Entities;

namespace Polymind.Domain.Security;

/// <summary>Quy tắc gỡ mọi liên kết tài khoản khỏi hồ sơ ứng viên khi user bị xóa.</summary>
public static class CandidateAccountLinkRules
{
    public static bool UnlinkUser(Candidate candidate, Guid userId)
    {
        var changed = false;
        if (candidate.OwnerUserId == userId)
        {
            candidate.OwnerUserId = null;
            changed = true;
        }
        if (candidate.ParentUserId == userId)
        {
            candidate.ParentUserId = null;
            changed = true;
        }
        return changed;
    }
}
