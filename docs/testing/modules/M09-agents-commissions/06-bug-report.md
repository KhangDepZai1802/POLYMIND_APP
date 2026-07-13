# M09 — Agents & Commissions · 06 Bug Report

> QA: Claude · 2026-07-10. Chỉ ghi bug có bằng chứng (code-level). Không sửa business logic.

## BUG_M09_01 — Sinh hoa hồng thiếu chốt idempotency ở DB (race → hoa hồng trùng)

- **Module:** M09 · **Severity:** Medium · **Priority:** Medium · **Confidence:** Medium (code-level; runtime cần integration parallel — Blocked harness)
- **Business Flow:** BF-M09-01 · **Test Case:** TC_M09_003 · **Automated Test:** (integration parallel — chưa có)
- **Environment:** code (mọi môi trường) · **Role:** system (trigger staff/accountant)
- **Preconditions:** ứng viên có agent + config; ≥1 giai đoạn đóng tiền vừa Paid; hai trigger `CommissionEngine.EnsureAsync` chạy gần đồng thời cho **cùng ứng viên** (VD kế toán đánh dấu Payment Paid ở `Finance.razor:691` **đồng thời** nhân sự chuyển bước ở `CandidateDetail.razor:1818`; hoặc double-submit).
- **Steps to Reproduce:**
  1. Ứng viên A đã đóng đủ giai đoạn Deposit (Payment.Stage=Deposit, Status=Paid) nhưng chưa có commission Deposit.
  2. Đồng thời: (t1) kế toán mark payment Paid → EnsureAsync; (t2) nhân sự advance workflow → EnsureAsync.
  3. Cả hai `db.AgentCommissions.AnyAsync(...)` đọc **trước** khi bên kia SaveChanges → đều thấy "chưa tồn tại" → đều Add.
- **Expected:** mỗi (Agent, Candidate, Milestone) tối đa **1** commission (idempotent kể cả concurrency).
- **Actual (code-level):** tạo **2** commission cho cùng mốc → đại lý được **chi trùng** (sai tiền). Không có ràng buộc DB chặn.
- **API/DB Evidence:**
  - `CommissionEngine.cs:62-64` — `exists = await db.AgentCommissions.AnyAsync(c => c.AgentId==agentId && c.CandidateId==candidateId && c.Milestone==milestone)` rồi `Add` (read-then-write), mỗi caller DbContext riêng.
  - `ApplicationDbContext.cs:148-154` — `AgentCommission` chỉ có index `AgentId` và `Status`, **KHÔNG** unique index trên `(AgentId, CandidateId, Milestone)`.
  - 2 call site: `CandidateDetail.razor:1818`, `Finance.razor:691`.
- **Suspected Source Area:** thiếu unique constraint + không bắt `DbUpdateException`.
- **Required Files for Codex:** `src/Polymind.Web/Commissions/CommissionEngine.cs`, `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs`, `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor` (caller), `src/Polymind.Web/Components/Pages/Finance/Finance.razor` (caller) + migration mới.
- **Đề xuất fix (Codex quyết định):** thêm **unique index** `(AgentId, CandidateId, Milestone)` trên `agent_commissions` + migration; trong `EnsureAsync` bắt `DbUpdateException`/`UniqueConstraint` để bỏ qua bản trùng (giữ idempotent). *Lưu ý dữ liệu cũ: kiểm tra chưa có trùng trước khi tạo unique index.*
- **Dependencies / Regression Risk:** ảnh hưởng M10 Finance (tổng công nợ/chi), M16 Reports (tổng hoa hồng Paid). Regression cần chạy: sinh hoa hồng tuần tự (không hồi quy), U2 no-refund, report tổng.
- **Status:** Verified Fixed (code-level) — Claude 2026-07-11 (`08-verification-report.md`); runtime race/UI concurrency pending harness

> **Ghi chú:** giải quyết **OBS-M07-02** (M07 yêu cầu verify idempotency ở M09). Kết luận: idempotency **đúng ở tuần tự** (case thường — advance gọi lặp an toàn), **hở ở concurrency** (thiếu chốt DB).

## BUG_M09_02 — Duyệt/Chi hoa hồng không guard trạng thái phía server (stale-UI đảo trạng thái)

- **Module:** M09 · **Severity:** Low · **Priority:** Low · **Confidence:** Medium (code-level; cần 2 admin + UI cũ)
- **Business Flow:** BF-M09-02/03 · **Test Case:** TC_M09_015, TC_M09_016
- **Preconditions:** 2 người có quyền cùng mở `/agents/{id}`; hoặc UI một người cũ (chưa reload sau khi bên kia thao tác).
- **Steps to Reproduce:**
  1. Commission ở trạng thái Approved rồi Paid (bởi admin A).
  2. Admin B (UI cũ vẫn thấy nút "Duyệt") bấm Duyệt → `ApproveCommission` set `Status=Approved` **vô điều kiện** → **revert Paid→Approved** (mất dấu đã chi) + đổi ApprovedBy.
  3. Tương tự `MarkCommissionPaid` set Paid không kiểm `Status==Approved` → có thể Paid một commission chưa duyệt.
- **Expected:** chuyển trạng thái phải hợp lệ (Pending→Approved→Paid); không revert; không chi khi chưa duyệt.
- **Actual (code-level):** không guard trạng thái hiện tại phía server.
- **Evidence:** `AgentDetail.razor:361` `c.Status = CommissionStatus.Approved;` (không kiểm `== Pending`); `:378` `c.Status = CommissionStatus.Paid;` (không kiểm `== Approved`). UI chỉ gate hiển thị nút (`:218`, `:222`).
- **Required Files for Codex:** `src/Polymind.Web/Components/Pages/Agents/AgentDetail.razor`.
- **Đề xuất fix:** guard đầu mỗi method — `if (c.Status != CommissionStatus.Pending) { snackbar "đã đổi trạng thái, tải lại"; return; }` (approve) và `if (c.Status != CommissionStatus.Approved) { ...; return; }` (pay).
- **Regression Risk:** thấp; chạy lại approve/pay happy path.
- **Status:** Verified Fixed (code-level) — Claude 2026-07-11 (`08-verification-report.md`); runtime race/UI concurrency pending harness

## Observations (không phải bug chặn)

| ID | Severity | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|
| OBS-M09-01 / CR-M09-1 | Info/Req | CTV share trước đây tính động. | Snapshot CTV + % vào AgentCommission, backfill lịch sử. | **Implemented by Codex; final regression Blocked (restore environment)** |
| OBS-M09-02 / CR-M09-2 | Info/Req | Leaderboard trước đây lộ doanh số đối thủ cho partner. | Partner chỉ nhận row đại lý mình; staff giữ full. | **Implemented by Codex; final regression Blocked (restore environment)** |
| OBS-M09-03 | Low | `Collaborator.CommissionSharePercentage` DB default **50** (`ApplicationDbContext.cs:133 HasDefaultValue(50m)`) ≠ C# default **35** + hằng số Vietgroup 35. Đường tạo qua C# luôn set giá trị nên không ảnh hưởng thực tế; chỉ lệch nếu insert thô. | Đồng bộ DB default = 35. | Theo dõi |
| OBS-M09-04 | Low | `AgentDialog.Save` / `CollaboratorDialog.Save` **không** ghi audit (khác config/commission có audit). | Thêm AddAudit cho create/update agent + collaborator. | Theo dõi |

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M09_01 | Medium | TC_M09_003 | BF-M09-01 | Thiếu unique index (Agent,Candidate,Milestone) + không bắt DbUpdateException | `CommissionEngine.cs`, `ApplicationDbContext.cs`, `CandidateDetail.razor`, `Finance.razor` + migration | sinh hoa hồng tuần tự, U2 no-refund, report tổng | Verified Fixed (code) — Claude 2026-07-11 |
| 2 | BUG_M09_02 | Low | TC_M09_015/016 | BF-M09-02/03 | Không guard status server ở approve/pay | `AgentDetail.razor` | approve/pay happy path | Verified Fixed (code) — Claude 2026-07-11 |
| 3 | CR-M09-1 | Change | TC_M09_034 | BF-M09-06 | Share CTV động | entity/engine/portal/notification + migration | snapshot/history/model | **Blocked:** source build + M09 16/16 pass trước migration; cần restore và final rerun |
| 4 | CR-M09-2 | Change | TC_M09_035 | BF-M09-07 | Partner leaderboard leak | Agents.razor + visibility rule | staff/partner matrix | **Blocked:** source build + rule tests pass; cần final rerun/E2E |

> **Codex Status M09 = Blocked**, `Verification = Blocked`: implementation đã hoàn tất nhưng không thể chạy final suite/migration compile vì offline restore làm hỏng `project.assets`; restore ngoài sandbox bị từ chối do giới hạn usage. Không đánh dấu Fixed/Verified Fixed.
