# Module Fix Report

## Summary

- **Module ID:** M09
- **Module Name:** Agents & Commissions
- **Bugs/Changes Received:** BUG_M09_01/02 + CR-M09-1/2
- **Bugs Verified Previously:** 2 (Claude; không làm lại)
- **Changes Implemented This Session:** 2
- **Cannot Reproduce:** 0
- **Blocked:** 2 change requests đang chờ restore + final regression
- **Needs Clarification:** 0
- **Verification:** BUG_M09_01/02 đã Claude verify. CR-M09-1/2 chưa handoff Fixed vì final regression bị blocker môi trường.

## BUG_M09_01

### Status

Fixed — chờ Claude xác minh độc lập.

### Investigation

- Đọc toàn bộ context package M09 (`01`–`06`) và lần theo cả hai caller `CandidateDetail.AdvanceStep` và `Finance.MarkStagePaid`.
- Xác nhận `EnsureAsync` dùng mẫu `AnyAsync` rồi `Add`, trong khi model chỉ có index rời theo `AgentId` và `Status`. Hai DbContext đồng thời có thể cùng đọc “chưa tồn tại”.
- Xác nhận transaction boundary cũ khác nhau: `CandidateDetail` lưu workflow và commission chung một lần; `Finance` lưu Payment trước rồi lưu commission lần hai. Vì vậy chỉ bắt exception ở caller sẽ dễ làm lỗi thao tác chính hoặc lặp logic.
- Truy vấn PostgreSQL local trước migration: `duplicate_groups=0`, `total_rows=20`. Không sửa/xóa dữ liệu hiện có.

### Root Cause

Idempotency chỉ được bảo vệ ở application read-before-write, không có unique constraint trên khóa nghiệp vụ `(AgentId, CandidateId, Milestone)`. `EnsureAsync` cũng không sở hữu bước lưu nên không thể xử lý riêng conflict do commission mà không ảnh hưởng các entity khác đang được track.

### Evidence

- Source cũ: `CommissionEngine.EnsureAsync` kiểm tra tồn tại rồi stage entity; `ApplicationDbContext` không có unique composite index.
- Hai trigger dùng DbContext độc lập: workflow advance và payment paid.
- PostgreSQL race probe sau fix: **12 caller đồng thời → `rows=1`, `audits=1`, `returned=1`**.
- Migration được chạy từ đầu trên database tạm `polymind_m09_test`; database tạm được xóa sau probe.

### Files Inspected

- `docs/testing/modules/M09-agents-commissions/01-analysis.md` → `06-bug-report.md`
- `src/Polymind.Web/Commissions/CommissionEngine.cs`
- `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor`
- `src/Polymind.Web/Components/Pages/Finance/Finance.razor`
- `src/Polymind.Web/Components/Pages/Agents/AgentDetail.razor`
- `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- `src/Polymind.Domain/Entities/AgentCommission.cs`, `AgentCommissionConfig.cs`, `Agent.cs`, `Candidate.cs`, `CandidateJobOrder.cs`, `JobOrder.cs`, `Payment.cs`, `AuditLog.cs`
- `src/Polymind.Web/Auditing/AuditLogHelpers.cs`
- `src/Polymind.Web/Notifications/NotificationService.cs`
- `src/Polymind.Web/Reporting/CsvExportEndpoints.cs`
- `tests/Polymind.Tests/M09_CommissionRatesTests.cs`

### Files Changed

- `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/20260710161103_EnforceAgentCommissionIdempotency.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/20260710161103_EnforceAgentCommissionIdempotency.Designer.cs`
- `src/Polymind.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- `src/Polymind.Web/Commissions/CommissionEngine.cs`
- `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor`
- `src/Polymind.Web/Components/Pages/Finance/Finance.razor`
- `tests/Polymind.Tests/M09_CommissionRatesTests.cs`

### Symbols Changed

- `ApplicationDbContext.OnModelCreating`
- `CommissionEngine.EnsureAsync`
- `CommissionEngine.PersistAsync`
- `CommissionEngine.IsIdempotencyConflict`
- `CommissionEngine.DetachGeneratedEntries`
- `CandidateDetail.AdvanceStep`
- `Finance.MarkStagePaid`
- `M09_CommissionRatesTests.Agent_commission_model_has_unique_idempotency_index`

### Fix

- Thêm unique index `(agent_id, candidate_id, milestone)` làm chốt idempotency cuối ở PostgreSQL.
- Migration kiểm tra duplicate trước khi tạo index; nếu có dữ liệu trùng thì dừng với lỗi rõ ràng để đối soát thủ công, không tự xóa/gộp dữ liệu tiền.
- `EnsureAsync` nay tự lưu commission và audit của chính nó. Caller lưu thao tác chính trước, tránh conflict hoa hồng rollback workflow/payment.
- Khi PostgreSQL báo unique violation đúng constraint idempotency, engine detach đúng commission/audit vừa stage, nạp lại state và retry các mốc còn thiếu. Các `DbUpdateException` khác vẫn được ném ra, không bị nuốt.

### Why This Fix Is Correct

- BF-M09-01 và TC_M09_003 yêu cầu tối đa một commission cho mỗi `(Agent, Candidate, Milestone)` kể cả khi hai trigger chạy đồng thời; constraint DB bảo đảm invariant tại nơi duy nhất có thể phân xử race.
- Retry theo state DB sau conflict giữ được các mốc khác trong cùng lần Ensure và chỉ tạo audit cho row thực sự insert thành công.
- RB-2/U2 vẫn giữ nguyên: khóa không chứa JobOrder, nên đổi đơn không tái sinh mốc đã hưởng và không hoàn/hủy commission cũ.

### Alternatives Considered

- Chỉ giữ `AnyAsync`: không đóng race.
- Chỉ thêm unique index và để exception nổi lên: ngăn duplicate nhưng làm request lỗi, có thể ảnh hưởng thao tác workflow/payment.
- Nuốt mọi `DbUpdateException`: che lỗi DB không liên quan. Bản sửa chỉ nhận đúng SQLSTATE unique violation và đúng constraint.
- Tự deduplicate trong migration: không chọn vì có thể làm mất/sai dữ liệu tiền mà chưa đối soát.

### Impact

- **API impact:** không đổi contract; module không có REST endpoint.
- **Database impact:** thêm một unique index; migration fail-safe nếu dữ liệu cũ trùng.
- **UI impact:** thông báo số commission mới phản ánh số row do lượt gọi đó thực sự tạo.
- **Security impact:** không đổi permission/scope.
- **Backward compatibility:** dữ liệu không trùng tương thích; DB có trùng phải đối soát trước deploy.
- **Data compatibility:** không đổi entity columns hay giá trị enum.

### Regression Risks

- Deployment có duplicate lịch sử sẽ dừng migration có chủ đích.
- `EnsureAsync` nay sở hữu `SaveChanges`; caller mới trong tương lai phải lưu thay đổi nghiệp vụ khác trước khi gọi, giống hai caller đã sửa.
- Cần chú ý báo cáo M16 vẫn tổng hợp đúng một row/mốc.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| Query duplicate dữ liệu local | PostgreSQL read-only | Passed | 0 nhóm trùng / 20 rows |
| `Agent_commission_model_has_unique_idempotency_index` | Unit/model metadata | Passed | Kiểm tra unique và đúng database index name |
| Race probe 12 workers | PostgreSQL integration probe | Passed | 1 commission, 1 audit, tổng return 1 |
| `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --no-restore --nologo` | Regression | Passed | 48 passed, 0 failed, 0 skipped |
| `dotnet build src/Polymind.Web/Polymind.Web.csproj ... OutputPath=C:\tmp\polymind-m09-web-build\` | Build | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** model constraint, race runtime, module/shared unit suite, Web build.
- **Failed:** 0.
- **Skipped:** 0.
- **Blocked:** UI/E2E full flow chưa có harness; race lõi đã được đo trên PostgreSQL thật.

### Verification Instructions for Claude

- Chạy lại full test suite và Web/solution build.
- Kiểm tra migration có preflight duplicate và unique index đúng ba cột, không có logic tự xóa dữ liệu.
- Trên DB test, gọi đồng thời `EnsureAsync` cho cùng candidate/stage; xác nhận đúng 1 commission và 1 audit create.
- Gọi Ensure tuần tự lần hai; xác nhận trả 0 và không thêm row.
- Kiểm U2: đổi JobOrder sau khi đã có Deposit commission không tái sinh/hoàn commission.
- Kiểm report tổng hoa hồng không nhân đôi.

## BUG_M09_02

### Status

Fixed — chờ Claude xác minh độc lập.

### Investigation

- Xác nhận UI chỉ hiện Duyệt khi Pending và Đã chi khi Approved, nhưng hai method cũ load entity rồi gán status vô điều kiện.
- Guard đơn thuần sau query vẫn có cửa TOCTOU nếu một admin khác cập nhật giữa query và `SaveChanges`.

### Root Cause

State transition chỉ được kiểm soát ở UI; update DB không có predicate trên trạng thái nguồn. Entity tracking có thể ghi snapshot cũ đè trạng thái mới.

### Evidence

- Source cũ: `ApproveCommission` luôn set Approved; `MarkCommissionPaid` luôn set Paid.
- Source mới: update có điều kiện `Id && Status == Pending/Approved`; `affected == 0` thì rollback, cảnh báo và reload.
- Luật transition được tách thành Domain và phủ test matrix.

### Files Inspected

- `src/Polymind.Web/Components/Pages/Agents/AgentDetail.razor`
- `src/Polymind.Domain/Enums/Enums.cs`
- `src/Polymind.Domain/Entities/AgentCommission.cs`
- `src/Polymind.Web/Auditing/AuditLogHelpers.cs`

### Files Changed

- `src/Polymind.Web/Components/Pages/Agents/AgentDetail.razor`
- `src/Polymind.Domain/Commissions/AgentCommissionTransitions.cs`
- `tests/Polymind.Tests/M09_CommissionRatesTests.cs`

### Symbols Changed

- `AgentDetail.ApproveCommission`
- `AgentDetail.MarkCommissionPaid`
- `AgentCommissionTransitions.CanApprove`
- `AgentCommissionTransitions.CanMarkPaid`
- `M09_CommissionRatesTests.Approve_transition_is_guarded`
- `M09_CommissionRatesTests.Mark_paid_transition_is_guarded`

### Fix

- Re-check state DB hiện tại bằng Domain transition rule.
- Thực hiện atomic conditional `ExecuteUpdateAsync` trong transaction: Pending→Approved hoặc Approved→Paid.
- Chỉ ghi audit và commit khi đúng một row chuyển trạng thái; stale/concurrent action nhận cảnh báo và reload.

### Why This Fix Is Correct

- Khớp state machine BF-M09-02/03 và TC_M09_015/016.
- Predicate ở câu UPDATE đóng cả stale UI lẫn race xảy ra sau bước đọc.
- Permission re-check cũ vẫn nằm trước mọi DB mutation; audit và update cùng transaction.

### Alternatives Considered

- Chỉ thêm `if (c.Status != ...)`: đóng stale UI thông thường nhưng vẫn có TOCTOU.
- Thêm rowversion toàn module: rộng hơn phạm vi và cần thay đổi model/schema khác.

### Impact

- **API/database schema:** không đổi cho bug này.
- **UI:** stale action hiển thị warning và reload thay vì đảo state.
- **Security:** permission `commissions:approve/update` giữ nguyên.
- **Backward/data compatibility:** giữ nguyên state/enum hiện có.

### Regression Risks

- Hai thao tác gần đồng thời: một thao tác hợp lệ thắng, thao tác còn lại phải reload; đây là hành vi mong muốn.
- Audit phải chỉ xuất hiện cho transition thành công.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| `Approve_transition_is_guarded` | Unit matrix | Passed | Pending true; Approved/Paid false |
| `Mark_paid_transition_is_guarded` | Unit matrix | Passed | Approved true; Pending/Paid false |
| Shared test suite | Regression | Passed | 48/48 |
| Web build | Compile | Passed | 0 warning, 0 error |

### Test Results

- **Passed:** transition matrix + compile/regression.
- **Failed:** 0.
- **Skipped:** 0.
- **Blocked:** bUnit/E2E 2-admin UI chưa có harness.

### Verification Instructions for Claude

- Pending: accountant/director approve thành Approved, đúng `ApprovedBy`, có 1 audit.
- Approved: accountant/super_admin mark paid thành Paid, có `PaidDate`, có 1 audit.
- Thử approve row Approved/Paid và pay row Pending/Paid; state và audit không đổi, UI reload.
- Chạy hai action cạnh tranh; xác nhận conditional update không thể Paid→Approved hoặc Pending→Paid.
- Kiểm director vẫn chỉ approve, không có quyền pay; accountant vẫn approve/pay theo quyết định user.

## CR-M09-1 — Snapshot phần chia CTV

### Status

**Implemented — Blocked before final regression**

### Investigation / Root Cause

`MyCommissions` lấy `Candidate.CollaboratorId` và `Collaborator.CommissionSharePercentage` hiện tại để tính lại mọi dòng lịch sử. Đổi CTV hoặc tỷ lệ làm thay đổi người hưởng/số tiền quá khứ; NotificationService cũng dùng tỷ lệ hiện tại.

### Fix

- `AgentCommission` snapshot nullable `CollaboratorId` + `CollaboratorSharePercentage` khi `CommissionEngine` tạo từng mốc.
- Domain rule chuẩn hóa 30..40 và làm tròn share amount dùng chung.
- Portal CTV query hoa hồng theo recipient snapshot, không theo assignment candidate hiện tại; portal/notification đều tính từ % snapshot.
- Migration `20260711170000_SnapshotCollaboratorCommissionShare` thêm cột/index và backfill lịch sử theo assignment tại lúc migration; không xóa row.
- Audit create commission ghi cả hai snapshot field.

### Why Correct / Impact

Khớp U-M09-1: thay đổi cấu hình CTV chỉ ảnh hưởng commission phát sinh sau đó. Schema additive, dữ liệu không có CTV giữ null; agent amount và state machine/idempotency đã verified không đổi.

### Regression Risks

- Backfill chỉ có thể đóng băng trạng thái quan hệ hiện có tại thời điểm migration vì dữ liệu cũ không lưu lịch sử.
- Migration chưa compile/apply sau khi restore environment hỏng.
- Cần kiểm notification M13 vẫn share-only với snapshot.

## CR-M09-2 — Ẩn doanh số đối thủ với partner

### Status

**Implemented — Blocked before final regression**

### Investigation / Root Cause

`Agents.Load` xếp hạng toàn bộ rồi đưa Top3 agent/Top5 CTV vào component cho cả staff và partner. Partner vì vậy thấy doanh thu, hoa hồng và số ứng viên của đối thủ.

### Fix / Why Correct

- Vẫn tính rank toàn cục để đại lý biết đúng thứ hạng của mình.
- Với Agent/CTV partner, `_agentBoard` chỉ giữ row của đại lý gắn scope; CTV board chỉ giữ CTV thuộc cùng đại lý. Missing AgentId fail-closed.
- Staff không phải partner giữ Top3/Top5 đầy đủ.
- `PartnerLeaderboardVisibility` là Domain rule có matrix test; desktop/mobile dùng cùng collection đã lọc.

Khớp U-M09-2 và không thay đổi quyền staff. Không dựa vào CSS để che dữ liệu.

## Files Inspected / Changed for CR-M09-1/2

- Inspected: toàn bộ M09 `01→08`, entity/config/migration, CommissionEngine, Agents, MyCommissions, NotificationService và consumers.
- Changed: `AgentCommission`, `AgentCommissionRates`, `PartnerLeaderboardVisibility`, DbContext/snapshot/migration, CommissionEngine, Agents, MyCommissions, NotificationService, M09 tests và reports.
- Không sửa lại code BUG_M09_01/02 đã Claude xác minh ngoài việc giữ tương thích snapshot.

## Tests Run for CR-M09-1/2

| Test | Result | Notes |
|---|---|---|
| Web build `.qa/build/m09-pre-migration` | Passed | 0 warning, 0 error; sau toàn bộ source logic, trước migration file/model test cuối |
| M09 tests | Passed 16/16 | snapshot + visibility rules included |
| Static snapshot/migration/use-path audit | Passed | fields đi qua engine → portal/notification; partner collections filtered |
| `git diff --check` relevant files | Passed | no whitespace errors |
| Final M09/full suite/Web build | **Blocked** | offline restore rewrote assets; NU1101; external restore approval rejected due usage limit |

## Verification / Unblock Instructions

1. Khôi phục dependencies: `dotnet restore src/Polymind.Web/Polymind.Web.csproj` khi network/approval available.
2. Chạy M09 tests (bao gồm model contract), full suite và Web build.
3. Áp migration trên DB test sạch + DB có commission cũ; kiểm backfill CTV/% và index.
4. Phát sinh commission ở 35%, đổi CTV 40%: dòng cũ/notification vẫn 35%, dòng mới 40%.
5. Agent/CTV mở `/agents`: chỉ row đại lý mình với rank toàn cục; agent chỉ thấy CTV cùng đại lý. Staff vẫn Top3/Top5 đầy đủ.
6. Sau khi tất cả pass, Claude xác minh độc lập; Codex không đánh dấu `Verified Fixed`.
