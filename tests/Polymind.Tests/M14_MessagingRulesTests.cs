using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Domain.Messaging;
using Polymind.Infrastructure.Persistence;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M14 — Messaging / Chat. `MessagingPolicy.CanMessage` (ma trận role) là pure static nhưng nằm ở
/// `Polymind.Web` → test project không ref Web (blocker chung) → ma trận role kiểm thủ công ở
/// `03-test-cases.md` (TC_M14_001..013). Các test dưới chốt hợp đồng Message entity mà UI dựa vào,
/// và quan hệ nhắn tin quanh ứng viên (CR-M14-1, thu hẹp bởi CR-M14-2).
/// TC_M14_041,042.
/// </summary>
public class M14_MessagingRulesTests
{
    [Fact] // TC_M14_041 — tin mới mặc định chưa đọc
    public void New_message_defaults_to_unread()
    {
        var m = new Message { SenderId = Guid.NewGuid(), RecipientId = Guid.NewGuid(), Body = "hi" };

        Assert.False(m.IsRead);
        Assert.Null(m.ReadAt);
    }

    [Fact] // TC_M14_042 — sender/recipient/body giữ đúng giá trị gán (không auto-thay)
    public void Message_retains_participants_and_body()
    {
        var sender = Guid.NewGuid();
        var recipient = Guid.NewGuid();

        var m = new Message { SenderId = sender, RecipientId = recipient, Body = "polymind-message-v1" };

        Assert.Equal(sender, m.SenderId);
        Assert.Equal(recipient, m.RecipientId);
        Assert.Equal("polymind-message-v1", m.Body);
    }

    [Fact] // CR-M14-2 — Học viên chỉ nhắn được CTV, TVV và phụ huynh của mình
    public void Student_can_only_message_collaborator_consultant_and_parent()
    {
        var student = Guid.NewGuid();
        var parent = Guid.NewGuid();
        var consultant = Guid.NewGuid();
        var collaborator = Guid.NewGuid();

        var allowed = CandidateMessagingRelationship
            .ForCandidate(student, parent, consultant, collaborator)
            .AllowedRecipientsFor(student);

        Assert.Equal(3, allowed.Count);
        Assert.Contains(parent, allowed);
        Assert.Contains(consultant, allowed);
        Assert.Contains(collaborator, allowed);
        Assert.DoesNotContain(student, allowed);
    }

    [Fact] // CR-M14-2 — Phụ huynh chỉ nhắn được CTV, TVV và học viên của mình
    public void Parent_can_only_message_collaborator_consultant_and_student()
    {
        var student = Guid.NewGuid();
        var parent = Guid.NewGuid();
        var consultant = Guid.NewGuid();
        var collaborator = Guid.NewGuid();

        var allowed = CandidateMessagingRelationship
            .ForCandidate(student, parent, consultant, collaborator)
            .AllowedRecipientsFor(parent);

        Assert.Equal(3, allowed.Count);
        Assert.Contains(student, allowed);
        Assert.Contains(consultant, allowed);
        Assert.Contains(collaborator, allowed);
        Assert.DoesNotContain(parent, allowed);
    }

    [Fact] // CR-M14-2 — đại lý / nhân sự hồ sơ / visa / workflow KHÔNG thuộc quan hệ nhắn tin portal
    public void Agent_and_workflow_staff_are_excluded_from_portal_relationship()
    {
        var student = Guid.NewGuid();
        var parent = Guid.NewGuid();
        var consultant = Guid.NewGuid();
        var collaborator = Guid.NewGuid();
        var agent = Guid.NewGuid();
        var visaStaff = Guid.NewGuid();

        var relationship = CandidateMessagingRelationship
            .ForCandidate(student, parent, consultant, collaborator);

        // Học viên không thấy đại lý / nhân sự visa.
        var studentAllowed = relationship.AllowedRecipientsFor(student);
        Assert.DoesNotContain(agent, studentAllowed);
        Assert.DoesNotContain(visaStaff, studentAllowed);

        // Chiều ngược lại đối xứng: họ cũng không nhắn được Phụ huynh/Học viên.
        Assert.Empty(relationship.AllowedRecipientsFor(agent));
        Assert.Empty(relationship.AllowedRecipientsFor(visaStaff));
    }

    [Fact] // CR-M14-2 — TVV/CTV chỉ thấy Phụ huynh/Học viên của ứng viên mình, không thấy nhau qua kênh này
    public void Consultant_and_collaborator_only_reach_candidate_portal_accounts()
    {
        var student = Guid.NewGuid();
        var parent = Guid.NewGuid();
        var consultant = Guid.NewGuid();
        var collaborator = Guid.NewGuid();
        var relationship = CandidateMessagingRelationship
            .ForCandidate(student, parent, consultant, collaborator);

        var consultantAllowed = relationship.AllowedRecipientsFor(consultant);
        Assert.Equal(2, consultantAllowed.Count);
        Assert.Contains(student, consultantAllowed);
        Assert.Contains(parent, consultantAllowed);
        Assert.DoesNotContain(collaborator, consultantAllowed);

        var collaboratorAllowed = relationship.AllowedRecipientsFor(collaborator);
        Assert.Equal(2, collaboratorAllowed.Count);
        Assert.Contains(student, collaboratorAllowed);
        Assert.Contains(parent, collaboratorAllowed);
        Assert.DoesNotContain(consultant, collaboratorAllowed);
    }

    [Fact] // CR-M14-1 — staff/CTV/Agent không liên quan fail-closed
    public void Unrelated_user_cannot_message_candidate_portal_accounts()
    {
        var unrelated = Guid.NewGuid();
        var relationship = CandidateMessagingRelationship.ForCandidate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Empty(relationship.AllowedRecipientsFor(unrelated));
    }

    [Fact] // CR-M14-1 — candidate chưa gắn portal account không mở quyền ngoài ý muốn
    public void Missing_portal_links_fail_closed()
    {
        var consultant = Guid.NewGuid();
        var relationship = CandidateMessagingRelationship.ForCandidate(null, null, consultant, null);

        Assert.Empty(relationship.AllowedRecipientsFor(consultant));
    }

    [Fact] // CR-M14-2 — ứng viên chưa gắn CTV thì học viên chỉ còn TVV + phụ huynh (không mở rộng)
    public void Candidate_without_collaborator_keeps_scope_tight()
    {
        var student = Guid.NewGuid();
        var parent = Guid.NewGuid();
        var consultant = Guid.NewGuid();

        var allowed = CandidateMessagingRelationship
            .ForCandidate(student, parent, consultant, null)
            .AllowedRecipientsFor(student);

        Assert.Equal(2, allowed.Count);
        Assert.Contains(parent, allowed);
        Assert.Contains(consultant, allowed);
    }

    [Fact] // CR-M14-2 — scope TVV chạy ở SQL, không nạp toàn bộ candidate rồi lọc client
    public void Consultant_scope_translates_for_postgresql()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=translation_only;Username=test;Password=test")
            .Options;
        using var db = new ApplicationDbContext(options);

        var sql = MessagingCandidateScope
            .ForConsultant(db.Candidates.AsNoTracking(), Guid.NewGuid())
            .ToQueryString();

        Assert.Contains("WHERE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("consultant", sql, StringComparison.OrdinalIgnoreCase);
    }
}
