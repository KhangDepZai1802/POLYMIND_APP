# M09 — Agents & Commissions · 01 Analysis

> QA: Claude · 2026-07-10 · Không sửa business logic. Giải quyết **OBS-M07-02** (verify idempotency CommissionEngine) + cross-check **U2** (RB-2 reset không hoàn hoa hồng).

## 1. Module Overview

- **Module ID:** M09
- **Module name:** Agents & Commissions (Đại lý, Cộng tác viên, Hoa hồng)
- **Business purpose:** Quản lý mạng lưới đại lý + CTV giới thiệu ứng viên; sinh & duyệt & chi **hoa hồng theo giai đoạn đóng tiền** (Vietgroup 07/2026: đại lý 5% tổng chia 1%/1.5%/2.5% theo 3 mốc Đặt cọc/Trúng tuyển/Xuất cảnh; CTV hưởng 30-40% phần đại lý). Bảng thi đua doanh số. Portal đại lý/CTV.
- **Actor / Role:**
  - Quản lý đại lý/CTV/config: `super_admin` (all), `recruitment_manager`/`recruiter` (collaborators create/update — CTV), `director`/`accountant` read.
  - Duyệt hoa hồng (`commissions:approve`): super_admin, director, accountant.
  - Chi hoa hồng (`commissions:update`): super_admin, accountant.
  - Portal đọc (`commissions:read` + scope): agent, collaborator (+ staff read).
- **Dependencies:** M02 (RBAC `agents:*`, `collaborators:*`, `commissions:*`), M05 (Candidate.AgentId/CollaboratorId), M06 (JobOrder.CostAmount/Country nền tính hoa hồng), M10 (Payment.Stage=Paid kích hoạt hoa hồng), M07 (workflow advance gọi EnsureAsync).
- **Entry point:** `/agents` (list + leaderboard), `/agents/{id}` (detail: CTV + config + commission approve/pay), `/agents/tree` (sơ đồ cây), `/my-commissions` (portal đối tác). Trigger sinh hoa hồng: `CandidateDetail` (advance) + `Finance` (mark payment Paid).
- **Exit point:** AgentCommission rows (Pending→Approved→Paid) + audit; CommissionConfig; Collaborator.

## 2. Source Code Map

| # | File | Loại | Method | Mục đích |
|---|---|---|---|---|
| 1 | `Web/Commissions/CommissionEngine.cs` | Logic (Web) | `EnsureAsync`, `Map` | **Sinh hoa hồng idempotent theo (Agent,Candidate,Milestone)** khi Payment.Stage=Paid; amount = %config × JobOrder.CostAmount. **KHÔNG SaveChanges** (caller lưu). |
| 2 | `Web/Components/Pages/Agents/Agents.razor` | Page `/agents` | `Load`, `Filtered`, `TakeTopWithPinned` | Leaderboard đại lý (top3) + CTV (top5, ẩn với CTV-only) theo tháng; danh sách đại lý (ẩn với partner-only). |
| 3 | `Web/Components/Pages/Agents/AgentDetail.razor` | Page `/agents/{id}` | `Load`, `ApproveCommission`, `MarkCommissionPaid`, `OpenConfigDialog`, `OpenCtvDialog`, `OpenAgentAccount/OpenCtvAccount` | Chi tiết đại lý: ứng viên, CTV, config hoa hồng, bảng hoa hồng + duyệt/chi. Partner-only bị redirect. |
| 4 | `Web/Components/Pages/Agents/AgentsTree.razor` | Page `/agents/tree` | — | Sơ đồ cây đại lý→CTV. |
| 5 | `Web/Components/Pages/Agents/AgentDialog.razor` | Dialog | `Save` | Thêm/sửa đại lý; re-check `agents:create/update`. |
| 6 | `Web/Components/Pages/Agents/CollaboratorDialog.razor` | Dialog | `Save`, `ClampPercentage` | Thêm/sửa CTV; re-check `collaborators:create/update`; **clamp share 30-40%**. |
| 7 | `Web/Components/Pages/Agents/CommissionConfigDialog.razor` | Dialog | `Save` | Cấu hình %/fixed theo mốc/đơn/quốc gia; re-check `agents:update`; % ưu tiên hơn fixed; audit. |
| 8 | `Web/Components/Pages/Portal/MyCommissions.razor` | Page `/my-commissions` | `LoadRowsAsync`, `MaskPhone` | Portal đối tác: agent thấy CTV + ứng viên + hoa hồng; CTV chỉ ứng viên mình + **mask SĐT**; tính CTV share tại chỗ. |
| 9 | `Domain/Entities/Agent.cs`, `Collaborator.cs`, `AgentCommission.cs`, `AgentCommissionConfig.cs` | Entity | — | Model. `Collaborator.CommissionSharePercentage=35` default. `AgentCommission.Status=Pending`. |
| 10 | `Domain/Commissions/AgentCommissionRates.cs` | Const (Domain) | — | Deposit 1 / Selected 1.5 / Departure 2.5 / Total 5; CTV 30-35-40. |
| 11 | `Infrastructure/.../ApplicationDbContext.cs:123-154` | DbConfig | — | Agent.Code unique; Collaborator.Code unique + FK Agent Cascade; AgentCommission index AgentId+Status (**KHÔNG unique idempotency key** — xem BUG_M09_01). |
| 12 | `Web/Identity/AgentScope.cs` | Scope | `GetAsync` | Phân giải agentId/collaboratorId từ UserId; partner-only vs staff. |
| 13 | `Web/Notifications/NotificationService.cs:396` | Consumer | — | RB-7 nhắc hoa hồng `Approved`. |
| 14 | `Web/Reporting/CsvExportEndpoints.cs`, `Reports.razor` | Consumer | — | Tổng hợp hoa hồng Paid (M16). |

## 3. UI Inventory

- **`/agents`:** filter tháng thi đua (chặn tháng tương lai), 2 bảng leaderboard (đại lý top3 / CTV top5), danh sách đại lý (search, pager), nút Thêm đại lý (`agents:create`), Sơ đồ cây. Partner-only: chỉ leaderboard + label scope; không list/detail.
- **`/agents/{id}`:** thông tin đại lý + nút sửa (`agents:update`), tài khoản đại lý/CTV (`users:create`), danh sách CTV (thêm/sửa nếu `collaborators:create`), config hoa hồng (thêm/sửa nếu `agents:update`), bảng hoa hồng + nút **Duyệt** (khi Pending + `commissions:approve`) / **Đã chi** (khi Approved + `commissions:update`).
- **CollaboratorDialog:** FullName*, phone, email, **% CTV (Min30 Max40)**, active switch, address, note.
- **CommissionConfigDialog:** milestone*, đơn hàng (clearable), quốc gia, % (0-100) HOẶC fixed; % ưu tiên.
- **`/my-commissions`:** KPI (agent: CTV/ứng viên/tổng hoa hồng/còn lại; CTV: đại lý/ứng viên/hoa hồng gộp/CTV hưởng), bảng CTV (agent), bảng ứng viên (CTV mask SĐT), bảng chi tiết hoa hồng.

## 4. API Inventory

Không REST endpoint (Blazor Server + IDbContextFactory). CSV export ở M16.

| Thao tác | Gate UI | Re-check server | DB side effect | Notification |
|---|---|---|---|---|
| Sinh hoa hồng | (tự động khi advance/pay) | actor bắt buộc `GetRequiredUserIdAsync` | insert AgentCommission (Pending) idempotent-app-level + audit create | RB-7 khi Approved (M13) |
| Duyệt hoa hồng | nút khi Pending | `ApproveCommission` re-check `commissions:approve` | Status=Approved, ApprovedBy, audit approve | — |
| Chi hoa hồng | nút khi Approved | `MarkCommissionPaid` re-check `commissions:update` | Status=Paid, PaidDate, audit mark_paid | — |
| Config hoa hồng | nút khi `agents:update` | `CommissionConfigDialog.Save` re-check `agents:update` | insert/update config + audit | — |
| Thêm/sửa CTV | nút khi `collaborators:*` | `CollaboratorDialog.Save` re-check + clamp 30-40 | insert/update collaborator | — |
| Thêm/sửa đại lý | `agents:create/update` | `AgentDialog.Save` re-check | insert/update agent | — |

## 5. Database Impact

- **agents**: Code unique; UserId (portal link).
- **collaborators**: Code unique; FK AgentId **Cascade** (xóa đại lý → xóa CTV); `CommissionSharePercentage` precision(5,2) **DB default 50** (⚠ khác C# default 35 → OBS-M09-03); UserId.
- **agent_commission_configs**: %/fixed precision; AgentId; JobOrderId?/Country? (null = mọi đơn/quốc gia).
- **agent_commissions**: index AgentId, Status; precision BaseAmount/CommissionAmount. **KHÔNG unique** trên (AgentId,CandidateId,Milestone) → idempotency chỉ ở app-level (BUG_M09_01). Status Pending→Approved→Paid; ApprovedBy, PaidDate, ReceiptId, Stage.
- **Audit:** create (engine), approve/mark_paid (AgentDetail), create/update config, (collaborator/agent save không audit — obs nhẹ).

## 6. Roles & Permissions

| Action | Role | Nguồn |
|---|---|---|
| agents read | super_admin, director, recruiter, recruitment_manager, accountant, agent, collaborator (+ scope) | DbSeeder |
| agents create/update | super_admin (all) | DbSeeder (chỉ SuperAdmin có agents:create/update trong map; RM/Recruiter chỉ collaborators) |
| collaborators create/update | super_admin, RM, recruiter (create+update); agent (update); director (không) | DbSeeder |
| commissions approve | super_admin, director, accountant | DbSeeder |
| commissions update (chi) | super_admin, accountant | DbSeeder |
| commissions read (portal) | super_admin, director, RM, accountant, agent, collaborator | DbSeeder |

> **Lưu ý:** `director` có `commissions:approve` nhưng KHÔNG có `commissions:update` → duyệt được, chi phải accountant/super_admin. Khớp quyết định user 2026-07-10 (accountant chi hoa hồng).

## 7. Risk Analysis

| Rủi ro | Đánh giá | Kết luận |
|---|---|---|
| **Idempotency hoa hồng dưới concurrency** | `EnsureAsync` AnyAsync-then-Add, KHÔNG unique constraint. 2 trigger (Finance mark-paid + workflow advance) cùng ứng viên/đồng thời → duplicate commission (double pay). | **BUG_M09_01 (Medium)** — giải quyết OBS-M07-02. |
| State transition hoa hồng không guard server | `ApproveCommission`/`MarkCommissionPaid` set status vô điều kiện; chỉ UI gate. Stale-UI + 2 admin → Paid→Approved revert / chi khi chưa duyệt. | **BUG_M09_02 (Low)**. |
| Attribution sai (first-user) | Mọi nơi dùng `GetRequiredUserIdAsync` (actor thật). | **Đúng** — không dính anti-pattern. |
| Broken authz / IDOR portal | Page `[Authorize]` + dialog re-check; MyCommissions lọc theo agentId/collaboratorId; AgentDetail redirect partner-only; CTV mask SĐT. | **Đóng** ở code. |
| RB-2 reset hoàn hoa hồng (U2) | `exists` check (agent,candidate,milestone) → đổi đơn KHÔNG regenerate mốc đã hưởng, KHÔNG hủy/hoàn commission cũ. | **U2 xác nhận: không hoàn tiền vô tình.** |
| CTV share clamp | `ClampPercentage` 30-40 ở Save (không chỉ Min/Max UI). | **Đóng**. |
| CTV share không snapshot | CTV amount tính tại display từ % hiện tại → đổi % làm đổi hiển thị hoa hồng lịch sử. | **OBS-M09-01** (confirm nghiệp vụ). |
| Leaderboard lộ doanh số đối thủ | Bảng thi đua hiện revenue/commission mọi đại lý cho partner. | **OBS-M09-02** — chủ đích Vietgroup, confirm. |
| Config amount khi CostAmount null | `baseAmount = CostAmount ?? 0` → commission 0. | Chấp nhận (chưa nhập chi phí đơn). |

## 8. Unknowns / Needs Requirement Clarification

- **U-M09-1 (OBS-M09-01):** CTV share có cần snapshot tại thời điểm phát sinh hoa hồng không (để đổi % không hồi tố)? Hiện tính động. Non-blocking.
- **U-M09-2 (OBS-M09-02):** Leaderboard hiển thị doanh số/hoa hồng của mọi đại lý cho tài khoản partner — xác nhận là chủ đích (thi đua). Mặc định coi đúng spec Vietgroup.
