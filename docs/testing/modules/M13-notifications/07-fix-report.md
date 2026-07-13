# Module Fix Report

## Summary

- **Module ID:** M13
- **Module Name:** Notifications
- **Bugs/Changes Received:** BUG_M13_01, CR-M13-1
- **Bugs/Changes Fixed:** 2
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Handoff:** **Fixed — chờ Claude xác minh độc lập**

## BUG_M13_01

### Status

**Fixed — Waiting for Claude Verification**

### Investigation

Session trước đã sửa đúng phần Agent owner + ba lifecycle Pending/Approved/Paid, nhưng cố ý chưa gửi CTV vì U-M13-2 chưa chốt. User nay chốt: chỉ CTV trực tiếp tại `Candidate.CollaboratorId` nhận và nội dung chỉ được nêu phần share của CTV.

`AgentCommission` có `CandidateId` nhưng không có `CollaboratorId`; quan hệ đúng phải resolve `AgentCommission.CandidateId → Candidate.CollaboratorId → Collaborator`. Cổng `/my-commissions` đang tính share bằng `Math.Round(CommissionAmount * CommissionSharePercentage / 100, 0, AwayFromZero)`.

### Root Cause

- Reminder commission trước session này chỉ có một payload chung chứa tổng `CommissionAmount` cho finance + Agent.
- Chưa resolve CTV trực tiếp từ candidate và chưa có payload least-privilege riêng cho CTV.
- RB-6 route `commission → /agents/{id}` không phù hợp với partner vì `AgentDetail` redirect partner ra `/agents`.

### Evidence

- Source trước hoàn tất: comment tại commission block ghi CTV không nhận; query commission không select `CandidateId`.
- Source sau sửa: candidate→collaborator dictionary, kiểm `Collaborator.AgentId == AgentCommission.AgentId`, yêu cầu `Collaborator.UserId`, sau đó tạo event `collaborator_commission` riêng trước event tổng.
- `CollaboratorTextFor` chỉ nhận `collaboratorShareAmount`, không nhận tổng commission nên không thể format tổng vào title/body.
- 3 regression rows Pending/Approved/Paid xác nhận body chứa share 350.000 và không chứa tổng 1.000.000.

### Files Inspected

- Toàn bộ `docs/testing/modules/M13-notifications/01→07`
- `src/Polymind.Web/Notifications/NotificationService.cs`
- `src/Polymind.Web/Notifications/NotificationJob.cs`
- `src/Polymind.Web/Components/Pages/Agents/AgentDetail.razor`
- `src/Polymind.Web/Components/Pages/Portal/MyCommissions.razor`
- `src/Polymind.Domain/Notifications/CommissionNotificationRules.cs`
- `src/Polymind.Domain/Notifications/FinancialNotificationRules.cs`
- `src/Polymind.Domain/Entities/AgentCommission.cs`
- `src/Polymind.Domain/Entities/Candidate.cs`
- `src/Polymind.Domain/Entities/Collaborator.cs`
- `src/Polymind.Domain/Entities/Agent.cs`
- `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`
- `tests/Polymind.Tests/M13_NotificationRulesTests.cs`
- `WORKLOG.md` RB-7

### Files Changed

- `src/Polymind.Domain/Notifications/CommissionNotificationRules.cs`
- `src/Polymind.Domain/Notifications/FinancialNotificationRules.cs`
- `src/Polymind.Web/Notifications/NotificationService.cs`
- `tests/Polymind.Tests/M13_NotificationRulesTests.cs`
- `docs/testing/modules/M13-notifications/03-test-cases.md`
- `docs/testing/modules/M13-notifications/04-traceability.md`
- `docs/testing/modules/M13-notifications/05-automation-report.md`
- `docs/testing/modules/M13-notifications/06-bug-report.md`
- `docs/testing/modules/M13-notifications/07-fix-report.md`
- `docs/testing/MODULE_QA_BOARD.md`
- `docs/testing/SESSION_CHECKPOINT.md`

### Symbols Changed

- `CommissionNotificationRules.CollaboratorShareAmount`
- `CommissionNotificationRules.CollaboratorTextFor`
- `CommissionNotificationText`
- `NotificationService.BuildReminderEventsAsync`
- `NotificationService.ResolveTargetUrlAsync` (`collaborator_commission`)

### Fix

1. Giữ nguyên lifecycle mapping và Agent owner đã sửa đúng ở session trước.
2. Nạp `Candidate.CollaboratorId` chỉ cho candidate có commission, rồi nạp metadata đúng các collaborator đó.
3. Chỉ gửi khi collaborator:
   - đúng ID trực tiếp trên candidate;
   - thuộc cùng `AgentId` với commission;
   - có `UserId` để nhận notification.
4. Tính share đúng công thức `/my-commissions` và tạo title/body riêng không chứa tổng commission.
5. Tạo event CTV trước event tổng. Vì unique key không chứa `ReferenceType`, thứ tự này fail-safe theo least privilege nếu dữ liệu gắn chồng cùng một UserId.
6. Route `collaborator_commission` tới `/my-commissions`; Agent/finance vẫn dùng `commission → /agents/{agentId}`.

### Why This Fix Is Correct

- Khớp U-M13-2 và TC_M13_041: không gửi cả cây CTV; chỉ quan hệ trực tiếp từ candidate.
- CTV không thể thấy tổng vì helper nội dung chỉ nhận numeric share.
- Agent vẫn thấy tổng commission của chính Agent; Accountant/SuperAdmin giữ payload tài chính đầy đủ.
- Ba trạng thái Pending/Approved/Paid dùng cùng notification type đã chốt, dedup vẫn theo `(UserId,Type,ReferenceId,Channel)`.
- Không đổi schema, không sửa expected result để né lỗi, không làm yếu authorization.

### Alternatives Considered

- Gửi cùng payload tổng cho CTV: loại vì vi phạm U-M13-2.
- Gửi mọi CTV thuộc Agent: loại vì lộ dữ liệu cho CTV không giới thiệu candidate.
- Không nêu số tiền: loại vì user đã chốt CTV thấy phần share của mình.
- Route CTV về `/agents/{id}`: loại vì partner bị redirect và không tới đúng màn hoa hồng cá nhân.

### Impact

- **API:** không đổi contract/endpoint.
- **Database:** chỉ thêm query đọc; không schema/migration/write nguồn.
- **UI:** CTV nhận notification riêng và click tới `/my-commissions`.
- **Security:** không lộ tổng commission đại lý; fail-closed khi thiếu/mismatch mapping.
- **Backward compatibility:** notification Agent/finance và enum string giữ nguyên.
- **Data compatibility:** notification cũ không bị sửa/xóa.

### Regression Risks

- Runtime Hangfire/PostgreSQL chưa có integration harness.
- Share hiện dùng tỷ lệ CTV hiện tại, giống portal. U-M09-1 snapshot tỷ lệ tại thời điểm phát sinh chưa được thực thi; nếu đổi tỷ lệ sau khi commission phát sinh, notification mới có thể dùng tỷ lệ mới.
- Notification đã dedup từ trước không được rewrite payload; đây là hành vi dedup hiện hữu.

## CR-M13-1

### Status

**Fixed — Waiting for Claude Verification**

### Investigation / Root Cause

`financeRecipients` đã union owner + finance nhưng vẫn gọi `RoleUsers(Accountant, Director, SuperAdmin)`. Quyết định cuối U-M13-1 yêu cầu chỉ Accountant + SuperAdmin; Director phải bị bỏ khỏi mọi nhánh dùng finance recipient.

### Fix

- Chốt danh sách role nghiệp vụ trong `FinancialNotificationRules.RecipientRoleNames = ["accountant", "super_admin"]`.
- `NotificationService` dùng chính danh sách này cho payment, repayment, expense và commission finance recipients.
- Giữ union candidate owner cho payment/repayment; khoản chi không gắn candidate chỉ gửi finance roles.
- Không xóa Director khỏi visa/flight fallback hoặc quyền generate-all vì các nhánh đó không phải finance recipient và ngoài CR-M13-1.

### Why This Fix Is Correct

- Exact-role regression khẳng định Director không có trong danh sách.
- Source guard xác nhận `RoleNames.Director` còn lại chỉ ở `canSeeAll` và Visa/Flight fallback, không nằm trong `financeRecipients`.
- SuperAdmin được giữ, Accountant được giữ, candidate owner được cộng thêm chứ không thay thế finance.

### Impact / Risks

- Director không nhận notification tài chính mới sau deploy; notification cũ trong DB vẫn còn (không xóa dữ liệu lịch sử).
- Không thay đổi quyền xem trang/notification đã sở hữu.
- Runtime role routing cần Claude kiểm với DB/Hangfire.

## Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `dotnet test ... --filter FullyQualifiedName~M13_NotificationRulesTests` | Unit regression | Passed 15/15 | lifecycle, recipients, exact finance roles, CTV share content |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore` | Full regression | Passed 98/98 | Failed 0, Skipped 0 |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=.../.qa/build/m13-final/` | Build | Passed | 0 warning, 0 error |

## Test Results

- **Passed:** 98
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime Hangfire/PostgreSQL/UI verification (chưa có harness)

## Verification Instructions for Claude

1. Chạy `M13_NotificationRulesTests`, toàn suite và Web build.
2. Đọc `financeRecipients`: xác nhận chỉ dùng `FinancialNotificationRules.RecipientRoleNames`; Director còn lại không thuộc nhánh tài chính.
3. Với DB test, tạo Candidate có CTV trực tiếp A và CTV cùng đại lý B; sinh commission đủ Pending/Approved/Paid:
   - Agent nhận tổng;
   - CTV A nhận đủ ba mốc, chỉ thấy share;
   - CTV B không nhận;
   - Accountant + SuperAdmin nhận;
   - Director không nhận.
4. Đổi `Collaborator.UserId=null` hoặc làm mismatch `Collaborator.AgentId`: không tạo event CTV, job không crash.
5. Click notification CTV phải tới `/my-commissions`; Agent/finance vẫn tới `/agents/{agentId}`.
6. Chạy job lặp lại, xác nhận unique/dedup không tạo trùng.
7. Kiểm residual U-M09-1: tỷ lệ share chưa snapshot; không đánh giá BUG_M13_01 thành fail chỉ vì backlog snapshot chưa được triển khai, nhưng ghi rõ rủi ro lịch sử.

