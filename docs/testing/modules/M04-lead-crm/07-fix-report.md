# Module Fix Report

## Summary

- **Module ID:** M04
- **Module Name:** Lead CRM
- **Bugs Received:** 1
- **Bugs Fixed:** 1
- **Cannot Reproduce:** 0
- **Blocked:** 0 bug; runtime Blazor/PostgreSQL convert verification chờ harness
- **Needs Clarification:** 0 (user chốt `CreatedBy=actor`)

## BUG_M04_01

### Status

- Fixed

### Investigation

Đã đọc toàn bộ nhánh `LeadDetail.Convert`, các pattern lấy actor trong cùng component, helper audit/auth, entity Lead/Candidate, `CandidateDialog`, demo seeder và tìm mọi nơi đọc/ghi `CreatedBy`. Chỉ nhánh convert dùng user đầu tiên; các thao tác nghiệp vụ khác đều lấy actor qua `GetRequiredUserIdAsync`.

### Root Cause

Code convert dùng biến `adminId = db.Users.FirstOrDefault()` như fallback seed thay vì principal đang xác thực, dù component đã inject `AuthenticationStateProvider` và dùng đúng actor ở các method lân cận.

### Evidence

- Trước sửa: dòng convert query user đầu tiên không `OrderBy`, rồi gán `Candidate.CreatedBy=adminId`.
- Cùng file, delete/assign/appointment/revert đều gọi `AuthStateProvider.GetRequiredUserIdAsync(db)`.
- Không có consumer nào yêu cầu `Candidate.CreatedBy` phải là admin seed; Candidate edit bảo toàn field hiện tại.
- Regression mới xác nhận actor attribution và toàn bộ profile/assignment mapping.

### Files Inspected

- `src/Polymind.Web/Components/Pages/Leads/LeadDetail.razor`
- `src/Polymind.Web/Auditing/AuditLogHelpers.cs`
- `src/Polymind.Web/Components/Pages/Candidates/CandidateDialog.razor`
- `src/Polymind.Domain/Entities/Lead.cs`
- `src/Polymind.Domain/Entities/Candidate.cs`
- `src/Polymind.Infrastructure/Persistence/DemoDataSeeder.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/20260624034033_InitialCreate.cs`

### Files Changed

- `src/Polymind.Domain/Leads/LeadConversionRules.cs`
- `src/Polymind.Web/Components/Pages/Leads/LeadDetail.razor`
- `tests/Polymind.Tests/M04_LeadConversionRulesTests.cs`

### Symbols Changed

- `LeadConversionRules.CreateCandidate`
- `LeadDetail.Convert`

### Fix

`Convert` lấy actor thật từ authentication state. Mapping Lead→Candidate được tách thành quy tắc thuần nhận bắt buộc `actorId` và `candidateCode`; factory gán `CreatedBy=actorId`, từ chối GUID rỗng và giữ nguyên toàn bộ mapping profile/agent/CTV/TVV cũ.

### Why This Fix Is Correct

Fix đáp ứng BF-M04-05, TC_M04_007 và TC_M04_008 theo quyết định user. Principal đã qua `[Authorize(leads:read)]` và kiểm `leads:update` + `candidates:create`; actor lấy tại server nên không thể giả mạo từ client. Không thay đổi state transition Converted hoặc validation/authorization.

### Alternatives Considered

- Chỉ thay một dòng `adminId` bằng `actorId`: sửa được lỗi nhưng không có điểm unit-test ổn định do logic nằm trong Razor component.
- Ghi actor vào audit riêng nhưng giữ `CreatedBy` admin: trái nghĩa field và quyết định user.
- Dùng `lead.AssignedTo`: đó là TVV phụ trách, không nhất thiết là người bấm convert.

### Impact

- **API:** không đổi.
- **Database:** Candidate mới có `created_by` đúng actor; không migration, không sửa dữ liệu cũ.
- **UI:** không đổi.
- **Security:** actor lấy server-side sau authorization.
- **Backward compatibility:** toàn bộ mapping Lead→Candidate và navigation giữ nguyên.
- **Data compatibility:** dữ liệu Candidate cũ không bị rewrite.

### Regression Risks

- Factory mới phải tiếp tục được cập nhật nếu Lead/Candidate bổ sung field cần copy; test mapping hiện tại giúp phát hiện hồi quy.
- Race tạo hai Candidate từ một Lead vẫn là rủi ro đã ghi riêng, không thuộc bug attribution.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `Converted_candidate_is_attributed_to_the_actor` | Unit regression | Passed | BUG_M04_01 / TC_M04_008 |
| `Conversion_preserves_candidate_profile_and_assignment_fields` | Unit regression | Passed | TC_M04_007 mapping |
| `Empty_actor_is_rejected` | Unit negative | Passed | Fail-fast attribution |
| Toàn bộ `Polymind.Tests` | Regression | Passed 29/29 | Failed 0, Skipped 0 |
| Build `Polymind.Web` ra `C:\tmp\polymind-codex-build` | Compile | Passed | 0 warning, 0 error |
| Convert UI + DB attribution | Integration/manual | Blocked | Chưa có test DB/harness; host hiện chạy code cũ |

### Test Results

- **Passed:** 29 + build
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime UI/DB convert

### Verification Instructions for Claude

1. Đăng nhập bằng recruiter/consultant không phải user đầu tiên trong DB và có đủ `leads:update` + `candidates:create`.
2. Convert một Lead chưa có Candidate; xác nhận state Lead=Converted và chỉ tạo một Candidate.
3. Truy vấn Candidate mới: `created_by` phải đúng user đang thao tác, không phải super admin seed.
4. So sánh các field profile, `AgentId`, `CollaboratorId`, `ConsultantId` với Lead nguồn.
5. Convert lại cùng Lead và xác nhận nhánh chống trùng vẫn điều hướng Candidate cũ; không đánh dấu `Verified Fixed` chỉ dựa vào test Codex.
