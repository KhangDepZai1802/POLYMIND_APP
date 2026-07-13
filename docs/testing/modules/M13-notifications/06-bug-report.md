# M13 — Notifications · Bug Report

> **Codex 2026-07-11:** BUG_M13_01 + CR-M13-1 đã **Fixed — chờ Claude xác minh độc lập**. Agent nhận tổng; chỉ CTV trực tiếp nhận nội dung share riêng; finance chỉ Accountant + SuperAdmin, bỏ Director. Xem `07-fix-report.md`.
>
> **✅ USER CHỐT 2026-07-11 (Claude phiên #6) — Codex thực thi tiếp:**
> - **U-M13-2 (BUG_M13_01):** thông báo hoa hồng gửi **CHỈ CTV trực tiếp giới thiệu ứng viên** (`Candidate.CollaboratorId`), và CTV **chỉ thấy phần hoa hồng share của mình** (KHÔNG lộ tổng commission của đại lý). Giữ Agent owner + Accountant như đã làm.
> - **U-M13-1 (→ CR-M13-1):** finance recipients = **Kế toán + super_admin, BỎ Giám đốc (Director)**. Nhắc đóng tiền vẫn gửi **cả người phụ trách ứng viên VÀ Kế toán** (đã có qua `FinancialNotificationRules` + owner). **Delta:** bỏ `RoleNames.Director` khỏi `financeRecipients` (`NotificationService.cs:266-270`); rà các nhánh tài chính khác còn dùng Director.
> - Trạng thái: BUG_M13_01 + CR-M13-1 = **Ready for Codex** (hết gate).

---

## BUG_M13_01 — Thông báo hoa hồng KHÔNG tới CTV/Đại lý liên quan (vi phạm RB-7) + thiếu thông báo "đã chi"

- **Bug ID:** BUG_M13_01
- **Module ID:** M13
- **Title:** RB-7 (đã chốt user) yêu cầu thông báo hoa hồng gửi **"CTV/Đại lý liên quan + Kế toán"** cho vòng đời **"phát sinh → chờ duyệt → đã chi"**. Code chỉ route `CommissionPending`/`CommissionPayment` tới **Accountant + Director** — **không** có nhánh Agent/Collaborator, và **không có event "đã chi" (Paid)**. Hệ quả: đại lý/CTV — người trực tiếp hưởng hoa hồng — **không bao giờ** nhận thông báo về hoa hồng của mình.
- **Severity:** **Medium** (vi phạm "đúng người" RB-7 với requirement đã chốt; đối tượng thụ hưởng chính bị bỏ sót — không chỉ cosmetic).
- **Priority:** P2
- **Business Flow ID:** BF-M13-01 (reminder matrix — commission rows)
- **Test Case ID:** TC_M13_041
- **Automated Test ID:** — (routing ở `Polymind.Web`, cần integration harness; hiện chốt qua source-analysis + contract enum `CommissionPending`/`CommissionPayment`)
- **Environment:** mọi môi trường
- **Role:** nạn nhân = Agent (`Agent.UserId`) / Collaborator (`Collaborator.UserId`) liên quan `AgentCommission.AgentId`.
- **Preconditions:** có `AgentCommission` ở trạng thái `Pending`/`Approved`/`Paid`; đại lý đã có tài khoản Portal (`Agent.UserId` != null).
- **Steps to Reproduce:**
  1. Tạo/duyệt/chi 1 `AgentCommission` cho đại lý có `Agent.UserId`.
  2. Chạy `NotificationJob.RunAsync` (hoặc mở `/notifications` bằng super_admin để trigger generate-all).
  3. Đăng nhập tài khoản đại lý (`Agent.UserId`) → mở `/notifications`.
- **Expected Result:** đại lý/CTV nhận thông báo: hoa hồng **phát sinh** (Pending), **chờ chi** (Approved), **đã chi** (Paid) — theo RB-7.
- **Actual Result trước fix:** đại lý/CTV không nhận; chỉ Accountant + Director nhận Pending/Approved; không có Paid. **Sau fix Codex:** Agent nhận đủ Pending/Approved/Paid cùng Accountant/SuperAdmin; CTV trực tiếp nhận đủ ba mốc bằng nội dung riêng chỉ nêu share của mình; Director không còn trong finance recipients.
- **UI Evidence:** trang `/notifications` của đại lý trống các mục hoa hồng.
- **API Evidence:** —
- **Database Evidence:** không có `Notification` nào `UserId = Agent.UserId` với `Type ∈ {CommissionPending, CommissionPayment}`; không có `NotificationType` cho trạng thái Paid.
- **Log Evidence:** —
- **Suspected Source Area:**
  - `src/Polymind.Web/Notifications/NotificationService.cs:399-406` (`CommissionPayment` → `RoleUsers(roleRecipients, RoleNames.Accountant, RoleNames.Director)`).
  - `src/Polymind.Web/Notifications/NotificationService.cs:408-420` (`CommissionPending` → cùng recipient, không có Agent/CTV).
  - Không có nhánh nào resolve `Agents.Where(a => a.Id == c.AgentId).Select(a => a.UserId)` (và/hoặc Collaborator liên quan) làm recipient.
  - Không có event `NotificationType` cho commission `Paid` (`CommissionStatus.Paid`).
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Notifications/NotificationService.cs` (`BuildReminderEventsAsync` — khối commission)
  - `src/Polymind.Domain/Entities/AgentCommission.cs` (`AgentId`, `Status`)
  - `src/Polymind.Domain/Entities/Agent.cs` (`UserId`), `src/Polymind.Domain/Entities/Collaborator.cs` (`AgentId`,`UserId`)
  - `src/Polymind.Domain/Enums/Enums.cs` (`NotificationType`, `CommissionStatus`)
  - `WORKLOG.md` RB-7 (định nghĩa recipient "CTV/Đại lý liên quan + Kế toán")
- **Dependencies:** M09 Commissions (nguồn `AgentCommission`), U-M09-2 (ẩn doanh số với đối thủ — **không xung đột**: thông báo là về hoa hồng của CHÍNH đại lý đó).
- **Regression Risk:** Thấp–Trung — routing thêm event riêng cho CTV trực tiếp và bỏ Director khỏi tài chính. Cần runtime xác nhận dedup/role recipient với DB/Hangfire.
- **Confidence Level:** Cao (source rõ; RB-7 là requirement đã chốt user; `AgentCommission.AgentId → Agent.UserId` tồn tại nhưng không được dùng).
- **Status:** **✅ Verified Fixed (code-level) — Claude phiên #7 (2026-07-11).** BUG_M13_01 (CTV trực tiếp/share-only/route `/my-commissions`, guard same-agent+has-UserId fail-closed) + CR-M13-1 (finance = Accountant+SuperAdmin, Director loại) đối chiếu source + Domain + test genuine (không weaken). M13 15/15; toàn suite 101/101 (98 Codex + 3 M19 Claude); Web build 0/0. Xem `08-verification-report.md`. Residual R-M13-A (runtime Hangfire E2E), R-M13-B (U-M09-1 snapshot backlog, không phải regression).
- **Gợi ý hướng sửa (không bắt buộc):** trong khối commission, load `agentUserByAgentId` từ `db.Agents` (và collaborator nếu cần), thêm `Agent.UserId` vào recipient cho Pending/Approved; thêm `NotificationType` (VD `CommissionPaid`) + event khi `CommissionStatus.Paid` gửi tới Agent/CTV + Kế toán. Cần user chốt liệu "CTV liên quan" gồm collaborator trong cây đại lý hay chỉ đại lý sở hữu `AgentCommission.AgentId`.

---

## Observations (theo dõi — không handoff Codex trừ khi user chốt)

- **OBS-M13-01 / CR-M13-1 — RESOLVED by Codex, chờ Claude:** finance = Accountant + SuperAdmin, bỏ Director; payment/repayment union thêm owner ứng viên thay vì owner-first/fallback.
- **OBS-M13-02 — `canSeeAll` misnomer** (Low): super_admin/director khi generate được đánh dấu `canSeeAll` (sinh cho MỌI recipient) nhưng `GetForUserAsync` vẫn lọc `UserId==userId` → **không thực sự "thấy tất cả"**, chỉ thấy thông báo gửi cho mình. Job nền đã generate-all mỗi 5' nên nhánh này gần như thừa. (`NotificationService:23-36, 134-143`).
- **OBS-M13-03 — `MarkReadAsync(id)` không kiểm ownership** (Low, defense-in-depth): đánh dấu đã đọc theo id bất kỳ, không kiểm `UserId`. **Không exploit qua UI**: không có REST endpoint notifications; Blazor Server giữ `_items` server-side nên client không truyền id tùy ý. Nên thêm guard `n.UserId == currentUser` khi có harness. (`NotificationService:215-224`).
- **OBS-M13-04 — Timezone biên ngày UTC** (Low): `today = DateOnly.FromDateTime(DateTime.UtcNow.Date)`; VN UTC+7 → reminder có thể lệch ±1 ngày quanh nửa đêm. Nhất quán quy ước UTC toàn app; horizon 7 ngày nên tác động nhỏ. (`NotificationService:243-245`).
- **OBS-M13-05 — Dedup vĩnh viễn cho non-LeadCare** (Low, intentional): `seen` gồm cả notification đã đọc (query lọc `ReferenceId != null`, không lọc IsRead) → sau khi đọc, cùng `(user,type,ref,channel)` **không tái nhắc** dù sự kiện còn hiệu lực. Chỉ `ReminderLeadCare` có revive. Là lựa chọn chống spam; ghi nhận để nếu user muốn "nhắc lại khi vẫn quá hạn" cho payment/repayment thì cần mở rộng revive. (`NotificationService:474-529`).

---

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M13_01 | Medium | TC_M13_041 | BF-M13-01 (commission) | Agent/Paid + CTV trực tiếp/share riêng | NotificationService.cs, AgentCommission.cs, Candidate.cs, Collaborator.cs, WORKLOG RB-7 | M13 unit/source + integration routing | **✅ Verified Fixed (code-level) — Claude phiên #7** |
| 2 | CR-M13-1 | Change | TC_M13_002/042 | BF-M13-01 (finance) | finance roles | NotificationService.cs, FinancialNotificationRules.cs | exact role-list + union | **✅ Verified Fixed (code-level) — Claude phiên #7** |

> **Ghi chú:** U-M13-1/2 đã chốt và đã được Codex thực thi. CTV share hiện tính theo `Collaborator.CommissionSharePercentage` giống `/my-commissions`; U-M09-1 snapshot tỷ lệ vẫn là backlog riêng, nên lịch sử có residual nếu tỷ lệ CTV bị đổi sau khi commission phát sinh.
