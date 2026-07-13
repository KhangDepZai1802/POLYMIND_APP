# M09 — Agents & Commissions · Verification Report

> Xác minh độc lập của Claude sau khi Codex sửa (`07-fix-report.md`). Không sửa business logic; chỉ đọc source, chạy test, đánh giá.
> **Ngày:** 2026-07-11 · **AI:** Claude (Independent Verification Engineer) · **Môi trường:** Local (build + unit + model metadata; runtime race probe do Codex đo trên PostgreSQL, Claude chưa dựng lại harness DB).

## Phạm vi xác minh

| Nguồn | Đã đọc |
|---|---|
| `06-bug-report.md` (BUG_M09_01 Medium, BUG_M09_02 Low) | ✔ |
| `07-fix-report.md` | ✔ |
| `CommissionEngine.cs` (`EnsureAsync`/`PersistAsync`/`IsIdempotencyConflict`/`DetachGeneratedEntries`) | ✔ |
| `ApplicationDbContext.OnModelCreating` (unique index) | ✔ |
| Migration `20260710161103_EnforceAgentCommissionIdempotency.cs` | ✔ |
| `AgentDetail.razor` `ApproveCommission`/`MarkCommissionPaid` | ✔ |
| `Domain/Commissions/AgentCommissionTransitions.cs` | ✔ |
| Callers `EnsureAsync`: `CandidateDetail.AdvanceStep`, `PaymentPostingService.MarkPaidAsync` | ✔ |
| `M09_CommissionRatesTests.cs` | ✔ |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed! Failed: 0, Passed: 52, Skipped: 0
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo
# Build succeeded — 0 Warning(s), 0 Error(s)
```

---

## BUG_M09_01 — Idempotency race tạo hoa hồng trùng (thiếu unique index)

**Kết luận: Verified Fixed (code-level).** Race probe 12-worker do Codex chạy trên PostgreSQL thật (1 row/1 audit); Claude xác minh cấu trúc constraint + retry logic ở source, chưa dựng lại DB harness → không tuyên bố tự đo runtime.

### Bằng chứng đã kiểm

1. **Chốt cuối ở DB:** `ApplicationDbContext` (dòng 152) `e.HasIndex(x => new { x.AgentId, x.CandidateId, x.Milestone }).IsUnique();` — đúng ba cột nghiệp vụ. Unit test `Agent_commission_model_has_unique_idempotency_index` (Passed) kiểm cả IsUnique lẫn tên index DB.
2. **Migration fail-safe, KHÔNG tự xóa dữ liệu:** `Up` chạy khối `DO $$` `RAISE EXCEPTION` nếu tồn tại nhóm `(agent_id, candidate_id, milestone)` trùng, TRƯỚC khi `CreateIndex(..., unique: true)`. Tên index `ix_agent_commissions_agent_id_candidate_id_milestone` khớp hằng `CommissionEngine.IdempotencyIndexName`. Không có logic dedupe/gộp/xóa.
3. **`EnsureAsync` sở hữu save của chính nó:** đọc candidate/agent/cjo/jo/paidStages `AsNoTracking`, stage các mốc còn thiếu, rồi gọi `PersistAsync`. Không còn để caller lưu commission.
4. **Retry đúng và hẹp:** `PersistAsync` lặp tối đa `Map.Length+1` lần; mỗi vòng nạp lại `existing` milestones, chỉ add `remaining`. Bắt `DbUpdateException` **chỉ khi** `IsIdempotencyConflict` = `PostgresException { SqlState: UniqueViolation, ConstraintName: IdempotencyIndexName }`. Conflict → `DetachGeneratedEntries` (detach đúng commission + AuditLog vừa stage theo `Id`/`ResourceId`) → reload → retry mốc còn thiếu; hết attempt thì `throw`. **Mọi `DbUpdateException` khác vẫn nổi lên, không bị nuốt.**
5. **Save ordering ở cả 2 caller:** `CandidateDetail.AdvanceStep` `SaveChangesAsync()` bước workflow (dòng 1826) TRƯỚC `EnsureAsync` (dòng 1830); `PaymentPostingService.MarkPaidAsync` `SaveChangesAsync()` payment (dòng 50) TRƯỚC `EnsureAsync` (dòng 54). Conflict hoa hồng không rollback thao tác chính.
6. **U2/RB-2 giữ nguyên:** khóa idempotency KHÔNG chứa `JobOrderId` → đổi đơn hàng (reset workflow) không tái sinh mốc đã hưởng, không hoàn/hủy commission cũ. `exists`-guard (`AnyAsync`) trong `EnsureAsync` vẫn chặn tạo lại.

### Không tìm thấy hành vi né bug
- Không sửa expected result; test model metadata kiểm đúng bản chất (unique 3 cột).
- Không swallow lỗi DB không liên quan; không auto-delete dữ liệu tiền trong migration.

### Residual risk (đo lường được)
- Claude chưa dựng lại DB race probe; dựa vào evidence Codex (12 worker → 1 row/1 audit) + phân tích tĩnh constraint/retry. Cần Testcontainers/`polymind_test` để tự đo lại.
- Deploy có duplicate lịch sử sẽ dừng migration có chủ đích (đúng thiết kế) — vận hành phải đối soát trước.

---

## BUG_M09_02 — Approve/Pay hoa hồng không guard status (stale-UI revert)

**Kết luận: Verified Fixed (code-level).** UI 2-admin đồng thời còn chờ bUnit/E2E harness.

### Bằng chứng đã kiểm

1. **Domain transition rule:** `AgentCommissionTransitions.CanApprove` = `current == Pending`; `CanMarkPaid` = `current == Approved`. Unit test `Approve_transition_is_guarded` (Pending true; Approved/Paid false) và `Mark_paid_transition_is_guarded` (Approved true; Pending/Paid false) đều Passed.
2. **Atomic conditional update + transaction:** `ApproveCommission` (AgentDetail:353) và `MarkCommissionPaid` (:392) re-check permission → reload status `AsNoTracking` → `CanApprove/CanMarkPaid` → `BeginTransactionAsync` → `ExecuteUpdateAsync` với predicate `Id == id && Status == Pending/Approved`. `affected == 0` ⇒ `RollbackAsync` + cảnh báo + `Load()`. Không thể `Paid→Approved` hay `Pending→Paid`.
3. **Audit chỉ khi transition thật:** `AddAudit` + `SaveChangesAsync` + `CommitAsync` chỉ chạy sau khi `affected == 1`. Stale action không sinh audit giả.
4. **Permission trước mọi mutation:** `commissions:approve` / `commissions:update` re-check ở đầu method, đúng quyết định user (accountant/director approve; accountant/super_admin pay).

### Residual risk (đo lường được)
- Hai action cạnh tranh trên UI thật (2 tab/2 admin) chưa đo bằng harness; predicate ở câu UPDATE đã đóng TOCTOU ở tầng DB theo phân tích tĩnh.

---

## Kết luận module

| Bug | Severity | Verdict |
|---|---|---|
| BUG_M09_01 | Medium | **Verified Fixed** (code-level; race probe của Codex trên PostgreSQL, Claude chưa dựng lại DB harness) |
| BUG_M09_02 | Low | **Verified Fixed** (code-level; UI 2-admin pending harness) |

- **QA Status:** Completed
- **Codex Status:** Fixed
- **Verification Status:** Verified (code-level) — runtime race/UI concurrency chưa Claude tự đo, không tuyên bố 100%.
- **Observations:** OBS-M09-01/02 nay đã đóng qua CR-M09-1/2 (bên dưới); OBS-M09-03 (CTV DB default 50≠35), OBS-M09-04 (agent/CTV save không audit) vẫn mở.

---

# Verification — CR-M09-1 & CR-M09-2 (Claude phiên #8, 2026-07-11)

> Bổ sung: hai change request trước đó **Blocked before final regression** (offline restore hỏng). Phiên #8 môi trường đã khôi phục (restore + build + test + migration apply đều chạy được) nên hoàn tất xác minh độc lập.

## Bối cảnh unblock

- Blocker cũ (offline `dotnet restore` NU1101) **đã hết**: full restore/build/test chạy sạch phiên này.
- Full suite **122/122** (Failed 0, Skipped 0); M09 filter **17/17**; Web build `.qa/build/session8-web` **0 warning / 0 error**.
- **Runtime migration PoC** (giống M11 phiên #6): DB test sạch `polymind_m09_verify`, `dotnet ef database update` áp toàn bộ tới `20260711170000_SnapshotCollaboratorCommissionShare`. `\d agent_commissions` xác nhận cột `collaborator_id` (uuid, null), `collaborator_share_percentage` (numeric(5,2), null) và index `ix_agent_commissions_collaborator_id`; unique index idempotency BUG_M09_01 vẫn còn. DB test đã DROP.
- **Lưu ý kỹ thuật:** migration `20260711170000` KHÔNG có file `.Designer.cs` (Codex đặt `[Migration]`/`[DbContext]` inline trong `.cs`). `dotnet ef ... --no-build` với binaries cũ **không** thấy migration; build tươi thì `migrations list` hiện `(Pending)` và apply thành công. → Không blocker cho apply, nhưng nên bổ sung Designer khi generate migration kế tiếp (residual R-M09-D).

## CR-M09-1 — Snapshot phần chia CTV (U-M09-1)

| Kiểm tra | Bằng chứng | Verdict |
|---|---|---|
| Schema snapshot | `AgentCommission.CollaboratorId`/`CollaboratorSharePercentage`; migration additive + index; ModelSnapshot khớp; DB PoC áp sạch | **Verified** |
| Ghi snapshot lúc phát sinh | `CommissionEngine.cs:61` resolve `collaboratorSnapshot` từ `candidate.CollaboratorId`; `:98-99` set `CollaboratorId`/`CollaboratorSharePercentage` mỗi mốc; audit `:144-145` ghi cả hai | **Verified** |
| Portal đọc theo snapshot | `MyCommissions.razor:311` lọc `c.CollaboratorId == scopedCollaboratorId` (recipient snapshot); `:325-327` `collaboratorAmount` tính từ `c.CollaboratorSharePercentage` snapshot — KHÔNG theo config CTV hiện tại | **Verified** |
| Notification theo snapshot | `NotificationService.cs:420-465` dùng `CollaboratorId`/`CollaboratorSharePercentage` snapshot; CTV trực tiếp; `shareAmount` từ % snapshot (nhất quán BUG_M13_01 đã verify phiên #7) | **Verified** |
| Test | `Collaborator_share_uses_snapshot_from_commission_history` (350k vs 400k), `Commission_model_persists_collaborator_snapshot_and_indexes_recipient` (precision 5/scale 2/index) | **Pass** |
| Backfill an toàn | Migration SQL freeze theo assignment tại thời điểm migration, clamp 30..40 (tránh default cũ 50), không xóa row | **Verified** |

**Verdict CR-M09-1: Verified Fixed (code + runtime migration).** Thay đổi cấu hình/đổi CTV chỉ ảnh hưởng commission phát sinh SAU đó; lịch sử bất biến.

## CR-M09-2 — Ẩn doanh số đối thủ với partner (U-M09-2)

| Kiểm tra | Bằng chứng | Verdict |
|---|---|---|
| Domain rule | `PartnerLeaderboardVisibility.CanSeeAgentData(isPartnerOnly, currentAgentId, dataAgentId)` = staff thấy tất cả; partner chỉ agency mình; null → false | **Verified** |
| Rank toàn cục giữ nguyên | `Agents.razor:362-369` rank tính trên danh sách ĐẦY ĐỦ trước khi lọc → partner thấy đúng thứ hạng thật của mình | **Verified** |
| Agent board lọc partner | `:370-374` `_isPartnerOnly` → chỉ row `CanSeeAgentData(true,...)`; staff → `TakeTopWithPinned(...,3)` đầy đủ | **Verified** |
| CTV board lọc partner | `:395-401` lọc theo `CanSeeAgentData(_isPartnerOnly, _currentAgentId, row.AgentId)`; staff (`false`) thấy tất cả | **Verified** |
| Fail-closed | Partner chưa map (`_currentAgentId == null`) → board rỗng, không lộ dữ liệu | **Verified** |
| Không dựa CSS | Lọc ở tầng dữ liệu (`_agentBoard`/`_ctvBoard`), không ẩn bằng CSS | **Verified** |
| Test | `Partner_only_sees_own_agency_data` (matrix), `Unmapped_partner_cannot_see_any_agency_data` | **Pass** |

**Verdict CR-M09-2: Verified Fixed (code-level).** Đại lý chỉ thấy thứ hạng + dữ liệu agency mình; role khác (staff) không bị ẩn. Không đổi quyền staff.

## Residual / Not Measured (CR-M09-1/2)
- **R-M09-D (Low):** migration `20260711170000` thiếu `.Designer.cs`/`BuildTargetModel`; apply OK nhưng nên bổ sung để migration kế tiếp diff đúng. Không ảnh hưởng runtime hiện tại (ModelSnapshot là nguồn chuẩn).
- Backfill chỉ freeze được trạng thái quan hệ HIỆN CÓ tại thời điểm migration (dữ liệu cũ không lưu lịch sử % — giới hạn cố hữu).
- E2E UI thật (partner mở `/agents`, đổi CTV rồi phát sinh commission mới) chưa chạy bằng harness; đã đo qua unit + static + migration PoC.

## Kết luận M09 (cập nhật phiên #8)
- BUG_M09_01/02: **Verified Fixed** (giữ verdict phiên #4).
- CR-M09-1: **Verified Fixed (code + runtime migration)**.
- CR-M09-2: **Verified Fixed (code-level)**.
- → `QA Status = Completed`, `Codex Status = Fixed`, `Verification Status = Verified`. M09 rời trạng thái Blocked.
