using Polymind.Domain.Entities;
using Polymind.Domain.Security;
using Xunit;

namespace Polymind.Tests;

/// <summary>Regression cho BUG_M03_01 / TC_M03_015-016.</summary>
public class M03_CandidateAccountLinkRulesTests
{
    [Fact]
    public void Deleting_student_unlinks_owner_user_id()
    {
        var userId = Guid.NewGuid();
        var candidate = Candidate(ownerUserId: userId);

        var changed = CandidateAccountLinkRules.UnlinkUser(candidate, userId);

        Assert.True(changed);
        Assert.Null(candidate.OwnerUserId);
    }

    [Fact]
    public void Deleting_parent_unlinks_parent_user_id()
    {
        var userId = Guid.NewGuid();
        var candidate = Candidate(parentUserId: userId);

        var changed = CandidateAccountLinkRules.UnlinkUser(candidate, userId);

        Assert.True(changed);
        Assert.Null(candidate.ParentUserId);
    }

    [Fact]
    public void Same_user_in_both_links_is_fully_unlinked()
    {
        var userId = Guid.NewGuid();
        var candidate = Candidate(ownerUserId: userId, parentUserId: userId);

        var changed = CandidateAccountLinkRules.UnlinkUser(candidate, userId);

        Assert.True(changed);
        Assert.Null(candidate.OwnerUserId);
        Assert.Null(candidate.ParentUserId);
    }

    [Fact]
    public void Unrelated_candidate_links_are_preserved()
    {
        var ownerId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var candidate = Candidate(ownerUserId: ownerId, parentUserId: parentId);

        var changed = CandidateAccountLinkRules.UnlinkUser(candidate, Guid.NewGuid());

        Assert.False(changed);
        Assert.Equal(ownerId, candidate.OwnerUserId);
        Assert.Equal(parentId, candidate.ParentUserId);
    }

    private static Candidate Candidate(Guid? ownerUserId = null, Guid? parentUserId = null) => new()
    {
        Code = "UV-REGRESSION",
        FullName = "Regression candidate",
        OwnerUserId = ownerUserId,
        ParentUserId = parentUserId,
        CreatedBy = Guid.NewGuid(),
    };
}
