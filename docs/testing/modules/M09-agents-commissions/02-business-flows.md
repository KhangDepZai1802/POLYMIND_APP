# M09 — Agents & Commissions · 02 Business Flows

> QA: Claude · 2026-07-10.

## BF-M09-01 — Sinh hoa hồng theo giai đoạn đóng tiền (CommissionEngine.EnsureAsync)

- **Actor:** hệ thống (trigger bởi staff advance workflow hoặc kế toán mark payment Paid). Actor bắt buộc (`GetRequiredUserIdAsync`).
- **Preconditions:** ứng viên có AgentId; có CJO + JobOrder; có ≥1 Payment.Stage=Paid; agent có CommissionConfig cho mốc.
- **Main flow:** với mỗi (Milestone,Stage) trong Map [(Deposit,Deposit),(Selected,ServiceFee),(Departure,Settlement)]: nếu Stage đã Paid **và** chưa tồn tại commission (agent,candidate,milestone) → chọn config khớp nhất (JobOrder > Country > general) → amount = %×CostAmount hoặc FixedAmount → insert AgentCommission(Pending) + audit. Trả số lát mới (caller SaveChanges).
- **Idempotency:** `exists` guard theo (agent,candidate,milestone) → gọi lặp lại (mỗi advance) an toàn ở tuần tự.
- **Error/edge:** không AgentId/CJO/JobOrder/config/paid-stage → tạo 0. CostAmount null → amount 0.
- **DB:** insert agent_commissions + audit create. **Notification:** RB-7 khi sau đó Approved.
- **Risk:** **concurrency race** (2 trigger đồng thời) → duplicate (BUG_M09_01). **U2:** đổi đơn không regenerate mốc đã hưởng.

### State machine hoa hồng

| Current | Action | Allowed Role | Condition | Next | DB | History |
|---|---|---|---|---|---|---|
| (none) | EnsureAsync | system(actor) | stage Paid + config + chưa tồn tại | **Pending** | insert | audit create |
| Pending | Duyệt | super_admin/director/accountant | `commissions:approve` | **Approved** (ApprovedBy) | update | audit approve |
| Approved | Đã chi | super_admin/accountant | `commissions:update` | **Paid** (PaidDate) | update | audit mark_paid |
| Paid | *(không nút)* | — | — | — | — | — |

## BF-M09-02 — Duyệt hoa hồng

- **Actor/Role:** super_admin/director/accountant (`commissions:approve`).
- **Main flow:** `/agents/{id}` bảng hoa hồng → nút "Duyệt" (chỉ hiện khi Pending) → `ApproveCommission`: re-check approve → set Approved + ApprovedBy=actor + audit → reload.
- **Alternate:** thiếu quyền → snackbar + return.
- **Gap:** không guard `Status==Pending` server-side → stale-UI có thể revert Paid→Approved (BUG_M09_02).

## BF-M09-03 — Chi hoa hồng

- **Actor/Role:** super_admin/accountant (`commissions:update`).
- **Main flow:** nút "Đã chi" (chỉ hiện khi Approved) → `MarkCommissionPaid`: re-check update → set Paid + PaidDate + audit → reload.
- **Gap:** không guard `Status==Approved` server-side → về lý thuyết chi khi chưa duyệt nếu UI stale (BUG_M09_02).

## BF-M09-04 — Cấu hình % hoa hồng đại lý

- **Actor/Role:** super_admin (`agents:update`).
- **Main flow:** AgentDetail → Thêm/Sửa config → chọn mốc + (đơn/quốc gia tùy chọn) + % hoặc fixed → Save re-check `agents:update` → % ưu tiên (fixed=null khi có %) → audit → reload. EnsureAsync sau này dùng config khớp nhất.
- **Validation:** phải nhập % hoặc fixed.

## BF-M09-05 — Quản lý CTV + tỷ lệ hoa hồng

- **Actor/Role:** super_admin/RM/recruiter (create/update); agent (update, qua MyCommissions/AgentDetail).
- **Main flow:** thêm/sửa CTV → Save re-check `collaborators:*` → **clamp share 30-40%** → lưu. CTV code auto `CTV-yyMM###`.
- **Validation:** FullName bắt buộc; share clamp 30-40.

## BF-M09-06 — Portal đối tác (MyCommissions)

- **Actor/Role:** agent-only / collaborator-only (`commissions:read` + scope).
- **Main flow:** resolve scope → agent: KPI + bảng CTV + ứng viên + hoa hồng (agent net = gross − CTV share); CTV: chỉ ứng viên mình giới thiệu, **mask SĐT**, KPI CTV hưởng. CTV amount = round(CommissionAmount × share%/100).
- **Error flow:** không partner / chưa gắn agent/CTV → cảnh báo scope. IDOR: CTV không thấy ứng viên/hoa hồng của CTV khác (filter `CollaboratorId`).

## BF-M09-07 — Leaderboard thi đua (Agents list)

- **Actor:** staff (full) + partner (chỉ leaderboard). CTV-only: ẩn bảng CTV.
- **Main flow:** theo tháng lọc → doanh số = payments Paid trong tháng; ứng viên = đạt bước Đặt cọc trong tháng; commission = AgentCommission tạo trong tháng. Top3 đại lý / Top5 CTV + pin dòng "của tôi".
- **Risk:** hiện doanh số đối thủ (OBS-M09-02, chủ đích).

### Checklist nghiệp vụ

| Điểm kiểm | Kết quả |
|---|---|
| Thao tác trái quyền | Nút ẩn + re-check server (approve/pay/config/ctv/agent) |
| Hoa hồng trùng (idempotency) | App-level guard OK tuần tự; **race → duplicate** (BUG_M09_01) |
| State transition guard | **Thiếu server guard** approve/pay (BUG_M09_02) |
| Attribution | actor thật khắp nơi |
| RB-2 reset hoàn tiền (U2) | KHÔNG hoàn — exists guard giữ mốc đã hưởng |
| CTV mask SĐT | Có (collaborator portal) |
| IDOR portal | filter agentId/collaboratorId |
| Clamp CTV 30-40 | Có |
