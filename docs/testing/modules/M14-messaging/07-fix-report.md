# Module Fix Report

## Summary

- **Module ID:** M14
- **Module Name:** Messaging / Chat
- **Changes Received:** CR-M14-1
- **Changes Fixed:** 1
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Handoff:** **Fixed — chờ Claude xác minh độc lập**

## CR-M14-1

### Status

**Fixed — Waiting for Claude Verification**

### Investigation

`Messages.razor` trước sửa chỉ dựng `_allowedRecipientIds` khi `AgentScope.IsSelfScoped`. Staff/Agent/CTV đi thẳng qua role-only `MessagingPolicy`, trong đó parent/student rơi vào nhánh mặc định `true`; vì vậy mọi staff/partner đều thấy và nhắn được mọi portal account.

Quan hệ candidate hiện có đủ nguồn để giới hạn hai chiều:

- Portal: `Candidate.OwnerUserId`, `ParentUserId`.
- Partner: `Candidate.AgentId → Agent.UserId`, `CollaboratorId → Collaborator.UserId`.
- Staff phụ trách: `ConsultantId`, `CandidateJobOrder.AssignedTo`, `WorkflowStepRecord.AssignedTo`, `Visa.HandledBy`, `Flight.AssignedTo`.

### Root Cause

Relationship authorization chỉ được áp dựa trên loại **sender** self-scoped, thay vì áp bất cứ khi nào một đầu hội thoại là Parent/Student. `Send` cũng dùng cache allowed cũ cho self-scoped và không có relationship re-check cho staff/partner→portal.

### Evidence

- Trước sửa: `BuildAllowedRecipientsAsync` return `null` cho mọi non-self user; `LoadContacts` sau đó chỉ gọi `MessagingPolicy.CanMessage`.
- Sau sửa: `BuildRelationshipRecipientsAsync` dựng cùng graph cho mọi actor; `LoadContacts` buộc portal recipient thuộc graph; `Send` dựng lại graph từ DB trước mutation.
- `CandidateMessagingRelationship` fail-closed cho user không thuộc portal/responsible sets.
- EF translation regression xác nhận staff scope sinh `WHERE` + `EXISTS` ở PostgreSQL query.

### Files Inspected

- Toàn bộ `docs/testing/modules/M14-messaging/01→06`
- `src/Polymind.Web/Components/Pages/Messages/Messages.razor`
- `src/Polymind.Web/Identity/MessagingPolicy.cs`
- `src/Polymind.Web/Identity/AgentScope.cs`
- `src/Polymind.Domain/Entities/Candidate.cs`
- `src/Polymind.Domain/Entities/CandidateJobOrder.cs`
- `src/Polymind.Domain/Entities/WorkflowStepRecord.cs`
- `src/Polymind.Domain/Entities/Visa.cs`
- `src/Polymind.Domain/Entities/Flight.cs`
- `src/Polymind.Domain/Entities/Agent.cs`
- `src/Polymind.Domain/Entities/Collaborator.cs`
- `tests/Polymind.Tests/M14_MessagingRulesTests.cs`

### Files Changed

- `src/Polymind.Domain/Messaging/CandidateMessagingRelationship.cs`
- `src/Polymind.Web/Identity/MessagingPolicy.cs`
- `src/Polymind.Web/Components/Pages/Messages/Messages.razor`
- `src/Polymind.Web/Components/_Imports.razor`
- `tests/Polymind.Tests/M14_MessagingRulesTests.cs`
- `docs/testing/modules/M14-messaging/03-test-cases.md`
- `docs/testing/modules/M14-messaging/04-traceability.md`
- `docs/testing/modules/M14-messaging/05-automation-report.md`
- `docs/testing/modules/M14-messaging/06-bug-report.md`
- `docs/testing/modules/M14-messaging/07-fix-report.md`
- `docs/testing/MODULE_QA_BOARD.md`
- `docs/testing/SESSION_CHECKPOINT.md`

### Symbols Changed

- `CandidateMessagingRelationship.AllowedRecipientsFor`
- `MessagingCandidateScope.ForResponsibleUser`
- `MessagingPolicy.IsPortalUser`
- `Messages.BuildRelationshipRecipientsAsync`
- `Messages.LoadContacts`
- `Messages.Send`

### Fix

1. Tạo Domain relationship graph tách portal users khỏi responsible users, cho phép đúng hai chiều và loại chính user.
2. Resolve candidate scope theo actor:
   - self → `OwnedCandidateId`;
   - Agent → `Candidate.AgentId`;
   - CTV → `Candidate.CollaboratorId`;
   - staff → consultant/CJO/workflow/visa/flight assignment.
3. Resolve Agent/CTV account IDs và mọi assignee của candidate, dựng recipient graph.
4. `LoadContacts`: self chỉ thấy graph; non-self vẫn qua role policy, nhưng Parent/Student bắt buộc thuộc graph.
5. `Send`: query lại recipient roles và relationship graph từ DB trước upload/insert. UI/cache không quyết định authorization.
6. Giữ partner→staff role policy hiện hữu vì U-M14-1 không yêu cầu thay đổi phần này.

### Why This Fix Is Correct

- Khớp CR-M14-1: staff/Agent/CTV không còn liệt kê hoặc gửi tới portal ngoài candidate mình phụ trách.
- Áp đối xứng nên Parent/Student có thể trả lời đúng người đã được phép chủ động nhắn, tránh thread một chiều mới.
- Scope dùng quan hệ explicit, không dùng `CreatedBy` như proxy trách nhiệm.
- Missing Agent/CTV/portal mapping hoặc actor không liên quan trả rỗng.
- `Send` re-check ngay trước mutation, không chỉ ẩn UI.
- Không làm yếu các guard IDOR thread, recall ownership, upload validation đã Claude xác minh.

### Alternatives Considered

- Chỉ lọc danh bạ: loại vì có thể bypass bằng state/circuit cũ khi Send.
- Chỉ sửa `MessagingPolicy`: role-only policy không có candidate ID nên không thể enforce quan hệ.
- Chỉ giữ self-side cũ: tạo bất đối xứng, staff khởi tạo nhưng portal không reply được.
- Dùng `Candidate.CreatedBy`: loại vì người tạo không đồng nghĩa người đang phụ trách.

### Impact

- **API:** không có REST contract thay đổi.
- **Database:** query đọc bổ sung; không schema/migration.
- **UI:** danh bạ portal hẹp đúng quan hệ; portal có thể thấy thêm Agent/assignee thực sự liên quan để reply.
- **Security:** đóng contact enumeration và unauthorized initiation chéo candidate.
- **Backward compatibility:** staff↔staff và partner→staff role policy giữ nguyên; lịch sử Message không đổi.
- **Data compatibility:** không ghi/xóa dữ liệu hiện hữu.

### Regression Risks

- Chưa E2E với DB/Blazor/MinIO; query translation đã kiểm nhưng graph assembly runtime chưa có integration harness.
- Staff chỉ được coi là phụ trách qua explicit assignment nêu trên; candidate chưa gán staff sẽ không hiện portal cho staff đó (fail-closed).
- `Send` vẫn không re-check recipient `IsActive` (OBS-M14-03 ngoài CR).
- Existing thread ngoài quan hệ biến mất khỏi danh bạ nhưng lịch sử DB không bị xóa.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `dotnet test ... --filter FullyQualifiedName~M14_MessagingRulesTests` | Unit + EF translation | Passed 7/7 | relationship, symmetric reply, fail-closed, PostgreSQL SQL |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore` | Full regression | Passed 106/106 | gồm test M19 Claude thêm đồng thời; 0 fail/skip |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=.../.qa/build/m14-final/` | Build | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** 106
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime Blazor/PostgreSQL/MinIO E2E

### Verification Instructions for Claude

1. Chạy 7 M14 tests, toàn suite và Web build.
2. Đọc `LoadContacts`: portal recipient phải qua `_relationshipRecipientIds` trước role policy.
3. Đọc `Send`: phải gọi lại `BuildRelationshipRecipientsAsync(db)` trước upload/insert; không chỉ dùng cache.
4. Với DB/UI test, tạo hai candidate A/B với parent/student riêng, Agent/CTV/consultant/assignee riêng:
   - actor A thấy/gửi portal A;
   - không thấy/không gửi portal B;
   - portal A reply được đúng responsible users;
   - CTV khác cùng đại lý không được nhận nếu không phải `Candidate.CollaboratorId`.
5. Test staff responsibility lần lượt qua Consultant, CJO, WorkflowStep, Visa và Flight.
6. Xóa mapping Agent/CTV hoặc portal account: scope fail-closed, không crash.
7. Regression: staff↔staff, partner→staff, self family, thread IDOR, recall và attachment giữ hành vi cũ.

