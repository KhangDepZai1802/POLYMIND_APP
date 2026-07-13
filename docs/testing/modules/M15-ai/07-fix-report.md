# Module Fix Report

## Summary

- **Module ID:** M15
- **Module Name:** AI Assistant
- **Bugs Received:** 1
- **Bugs Fixed:** 1
- **Cannot Reproduce:** 0
- **Blocked:** 0
- **Needs Clarification:** 0
- **Handoff:** **Fixed — chờ Claude xác minh độc lập**

## BUG_M15_01

### Status

**Fixed — Waiting for Claude Verification**

### Investigation

`AiAssistant.OnInitializedAsync` chỉ tách self-scoped (parent/student) khỏi phần còn lại. Vì Agent/CTV đều có `IsSelfScoped=false`, họ đi vào `BuildDataContextAsync`. Method này truy vấn trực tiếp toàn bộ `Candidates`, `Leads` và `JobOrders`, không dùng `AgentScopeInfo.AgentId`/`CollaboratorId` đã resolve sẵn.

Đối chiếu với `Candidates.razor`, `Training.razor` và `MyCommissions.razor` xác nhận quy tắc partner hiện hữu: đại lý thấy ứng viên có cùng `AgentId`; CTV chỉ thấy ứng viên có đúng `CollaboratorId`. User đã chốt U-M15-1: partner vẫn được dùng AI nhưng context phải tuân theo phạm vi đó.

### Root Cause

Logic dựng prompt AI không có abstraction data-scope và mặc định mọi user không phải parent/student là staff. Đây là **Authorization / Data Scope** defect, không phải lỗi Gemini hay prompt.

### Evidence

- Trước sửa: `BuildDataContextAsync` gọi thẳng `db.Candidates`, `db.Leads`, `db.JobOrders`; `OnInitializedAsync` chỉ xét `_selfScoped`.
- Sau sửa: `AiDataScope` áp cùng scope lên candidate, lead và job order liên kết; `None` fail-closed nếu partner chưa có mapping.
- PostgreSQL translation regression xác nhận query Agent và CTV đều sinh `WHERE`; job-order query sinh `EXISTS`.
- Test module: 6/6 pass. Toàn suite: 94/94 pass. Web build: 0 warning, 0 error.
- Không ghi database, không migration, không gửi dữ liệu production tới Gemini trong quá trình kiểm thử.

### Files Inspected

- `src/Polymind.Web/Components/Pages/Ai/AiAssistant.razor`
- `src/Polymind.Web/Identity/AgentScope.cs`
- `src/Polymind.Domain/Entities/Candidate.cs`
- `src/Polymind.Domain/Entities/Lead.cs`
- `src/Polymind.Domain/Entities/JobOrder.cs`
- `src/Polymind.Domain/Entities/CandidateJobOrder.cs`
- `src/Polymind.Domain/Security/CandidateAccessScope.cs`
- `src/Polymind.Web/Components/Pages/Candidates/Candidates.razor`
- `src/Polymind.Web/Components/Pages/Training/Training.razor`
- `src/Polymind.Web/Components/Pages/Portal/MyCommissions.razor`
- `tests/Polymind.Tests/M02_CandidateAccessScopeTests.cs`

### Files Changed

- `src/Polymind.Domain/Ai/AiDataScope.cs`
- `src/Polymind.Web/Components/Pages/Ai/AiAssistant.razor`
- `src/Polymind.Web/Components/_Imports.razor`
- `tests/Polymind.Tests/M15_AiDataScopeTests.cs`
- `docs/testing/modules/M15-ai/03-test-cases.md`
- `docs/testing/modules/M15-ai/04-traceability.md`
- `docs/testing/modules/M15-ai/05-automation-report.md`
- `docs/testing/modules/M15-ai/06-bug-report.md`
- `docs/testing/modules/M15-ai/07-fix-report.md`
- `docs/testing/MODULE_QA_BOARD.md`
- `docs/testing/SESSION_CHECKPOINT.md`

### Symbols Changed

- `AiDataScope`, `AiDataScopeKind`
- `AiDataScope.ApplyCandidates`
- `AiDataScope.ApplyLeads`
- `AiDataScope.ApplyJobOrders`
- `AiAssistant.OnInitializedAsync`
- `AiAssistant.BuildDataContextAsync`

### Fix

1. Resolve một `AiDataScope` từ `AgentScopeInfo` khi mở trang:
   - staff → `All`;
   - đại lý có mapping → `ForAgent(AgentId)`;
   - CTV có mapping → `ForCollaborator(CollaboratorId)`;
   - partner thiếu mapping → `None`.
2. Áp scope vào danh sách ứng viên và thống kê lead.
3. Với partner, chỉ nạp/đếm job order có liên kết tới candidate trong chính scope; staff vẫn thấy toàn bộ job order.
4. Giữ nguyên nhánh self-scoped parent/student và quyền dùng AI/CV đã được user chốt.

### Why This Fix Is Correct

- Khớp BF-M15-04 và TC_M15_022/023: partner được dùng AI nhưng không có dữ liệu của partner khác trong prompt.
- Khớp data-scope đã dùng ở M05/M08/M09: Agent theo `Candidate.AgentId`, CTV theo `Candidate.CollaboratorId` trực tiếp.
- Cô lập nằm ở query trước khi tạo prompt, nên prompt injection không thể lấy dữ liệu ngoài scope.
- Fail-closed khi mapping partner thiếu, tránh biến lỗi cấu hình tài khoản thành quyền xem toàn công ty.
- Không làm yếu authorization, không đổi expected result và không sửa test để né lỗi.

### Alternatives Considered

- Chặn hoàn toàn partner khỏi `/ai`: không chọn vì trái U-M15-1.
- Chỉ thêm chỉ dẫn vào system prompt: không an toàn vì dữ liệu ngoài scope vẫn bị gửi tới Gemini.
- Chỉ lọc candidate nhưng giữ thống kê lead/job toàn công ty: không chọn vì vẫn lộ dữ liệu cạnh tranh và tạo context không nhất quán.

### Impact

- **API impact:** không đổi contract; M15 không có REST endpoint nội bộ.
- **Database impact:** chỉ thay đổi query đọc; không schema/migration/write.
- **UI impact:** partner vẫn vào AI và dùng UI như trước; câu trả lời chỉ dựa trên dữ liệu được phép.
- **Security impact:** đóng information disclosure giữa các đại lý/CTV.
- **Backward compatibility:** staff và parent/student giữ hành vi cũ.
- **Data compatibility:** không biến đổi dữ liệu.

### Regression Risks

- Query job order partner dùng `EXISTS` qua `CandidateJobOrder`; đã có test dịch SQL PostgreSQL nhưng chưa chạy E2E với Blazor circuit + DB thật.
- Partner không có mapping nay nhận context rỗng (fail-closed); đây là hành vi bảo mật chủ đích nhưng nên kiểm tra thông điệp UX ở runtime.
- Gemini E2E chưa chạy vì không dùng API key production trong test.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `dotnet test ... --filter FullyQualifiedName~M15_AiDataScopeTests` | Unit + EF translation | Passed 6/6 | Agent, CTV, staff, missing mapping; PostgreSQL SQL translation |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore` | Full regression | Passed 94/94 | Failed 0, Skipped 0 |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath=.../.qa/build/m15/` | Build | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** 94
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime UI/Gemini E2E (chưa có harness/key test)

### Verification Instructions for Claude

1. Chạy lại `M15_AiDataScopeTests` và toàn suite.
2. Build Web ra output riêng nếu dev server đang giữ DLL.
3. Đọc `AiAssistant.OnInitializedAsync`: xác nhận partner thiếu mapping dùng `AiDataScope.None`, không fallback `All`.
4. Đọc `BuildDataContextAsync`: xác nhận cả candidate, lead và job order đều lấy từ query đã scope.
5. Nếu có harness DB/UI, tạo hai đại lý và hai CTV; đăng nhập từng tài khoản, hỏi AI liệt kê ứng viên/lead/job và xác nhận không xuất hiện dữ liệu ngoài scope.
6. Kiểm tra staff vẫn thấy toàn bộ context; parent/student vẫn chỉ thấy `OwnedCandidateId` và tab CV vẫn ẩn.
7. Chú ý residual: E2E Gemini/circuit và UX khi partner chưa mapping chưa được Codex đo runtime.

