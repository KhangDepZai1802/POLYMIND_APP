# M13 — Notifications · Verification Report

> Claude độc lập xác minh bản sửa **BUG_M13_01 + CR-M13-1** của Codex (07-fix-report.md, 2026-07-11 15:07). Không sửa business logic.
> Nguồn đối chiếu: `06-bug-report.md`, `07-fix-report.md`, U-M13-1/U-M13-2 (đã chốt), diff source + test.

## 1. Bug/Change xác minh

| ID | Loại | Severity | Verdict |
|---|---|---|---|
| BUG_M13_01 | Bug | Medium | **Verified Fixed (code-level)** |
| CR-M13-1 | Change | Change | **Verified Fixed (code-level)** |

## 2. Yêu cầu nghiệp vụ áp dụng

- **U-M13-2 (BUG_M13_01):** thông báo hoa hồng gửi **CHỈ CTV trực tiếp** (`Candidate.CollaboratorId`); CTV **chỉ thấy phần share của mình**, KHÔNG lộ tổng `CommissionAmount` của đại lý.
- **U-M13-1 (CR-M13-1):** finance recipients = **Kế toán + super_admin, BỎ Giám đốc**; nhắc đóng tiền vẫn gửi cả người phụ trách ứng viên + Kế toán (union).

## 3. Bằng chứng đã đọc / kiểm

### 3.1 CR-M13-1 — Bỏ Director khỏi finance recipients
- **Domain** `FinancialNotificationRules.cs:7` — `RecipientRoleNames = ["accountant", "super_admin"]` (Director loại bỏ). `Recipients(finance, owners)` = union distinct finance + candidate owner (không thay thế).
- **Wiring** `NotificationService.cs:268-270` — `financeRecipients = RoleUsers(roleRecipients, FinancialNotificationRules.RecipientRoleNames.ToArray())` → chỉ Accountant + SuperAdmin. Dùng cho:
  - Payment reminder (`FinancialRecipients` = finance ∪ owner, dòng 271-274, 289).
  - Loan repayment (dòng 496 `FinancialRecipients`).
  - Expense chờ duyệt (dòng 514 `financeRecipients`).
  - Commission finance payload (dòng 473 `Recipients(financeRecipients, agent?.UserId)`).
- **Director còn lại CHỈ ở:** `canSeeAll` (dòng 29, quyền xem tất cả — không phải finance recipient) và visa/flight fallback (dòng 304, 323 — nhóm RB-7 Visa, ngoài phạm vi CR-M13-1). ✅ đúng như fix report tuyên bố.

### 3.2 BUG_M13_01 — CTV trực tiếp + share-only
- **Resolve CTV trực tiếp** `NotificationService.cs:416-419` — chỉ candidate có commission **và** `CollaboratorId != null` → `directCollaboratorByCandidate`. Không mở rộng ra cả cây CTV thuộc đại lý.
- **Guard fail-closed** `:449-452` — gửi CTV CHỈ khi: (a) đúng CTV trực tiếp trên candidate; (b) `collaborator.AgentId == c.AgentId` (đúng đại lý sở hữu commission); (c) `collaborator.UserId is Guid` (có tài khoản). Thiếu/mismatch → không tạo event CTV, không crash.
- **Share-only** `:454-461` — `CollaboratorShareAmount(CommissionAmount, CommissionSharePercentage)` (cùng công thức làm tròn `/my-commissions`), nội dung qua `CollaboratorTextFor` chỉ nhận **numeric share** → không thể format tổng vào title/body. Domain helper `CommissionNotificationRules.cs:32-48` xác nhận chỉ nêu `collaboratorShareAmount`.
- **Least-privilege dedup** `:446-468` — event CTV được add **TRƯỚC** event tổng. Vì unique key `(UserId,Type,ReferenceId,Channel)` không chứa `ReferenceType`, nếu một UserId trùng cả Agent lẫn CTV thì bản CTV (ít quyền hơn) persist trước → không vô tình lộ tổng. Fail-safe hợp lý.
- **Route RB-6** `:196-197` — `collaborator_commission → /my-commissions` (CTV tới đúng màn hoa hồng cá nhân); `commission → /agents/{agentId}` (`:190-194`) cho Agent/finance. Khớp: partner bị redirect khỏi `/agents` nên route riêng là cần thiết.
- **Agent vẫn thấy tổng** `:432-444, 471-473` — payload tổng gửi finance ∪ `agent?.UserId`.

## 4. Chống né test / workaround (PHẦN G mục 10-11)

- **Không** đổi expected result để pass; **không** xóa/skip/weaken assertion; **không** hard-code; **không** tắt authorization.
- Test mới trong `M13_NotificationRulesTests.cs` assert **thật**:
  - `Financial_recipient_roles_are_accountant_and_super_admin_only` (dòng 129) — `Assert.Equal(["accountant","super_admin"])` **và** `Assert.DoesNotContain("director")`.
  - `Collaborator_notification_contains_share_but_not_agent_total` (Theory ×3, dòng 139) — share=350k (1M @35%), `Contains(share)` **và** `DoesNotContain(agentTotal)` ở cả Title lẫn Body.
- Không có mock che dữ liệu; test dùng công thức + helper thật.

## 5. Build & Test

| Hạng mục | Lệnh | Kết quả |
|---|---|---|
| M13 unit | `dotnet test --filter ~M13_NotificationRulesTests` | **15/15** (Codex) |
| Toàn suite | `dotnet test tests/Polymind.Tests` | **101/101** (Claude phiên #7 — 98 của Codex + 3 M19 mới của Claude); Failed 0, Skipped 0 |
| Web build | `dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo` | Codex báo **0/0**; Domain symbols compile (suite 101/101 build Domain+Infra+Tests OK). Claude rebuild Web độc lập **sau** thay đổi NotificationService 15:03 = **pending** (Bash tool tạm gián đoạn phiên #7) — retry trước khi đóng session. |

## 6. Kết luận từng mục

- **CR-M13-1 → Verified Fixed (code-level).** Director bị loại khỏi mọi nhánh finance recipient (payment/repayment/expense/commission-finance); chỉ còn ở `canSeeAll` + visa/flight fallback (ngoài phạm vi). Kế toán + super_admin giữ; candidate owner cộng thêm.
- **BUG_M13_01 → Verified Fixed (code-level).** CTV trực tiếp (`Candidate.CollaboratorId`) nhận đủ 3 lifecycle Pending/Approved/Paid với **chỉ phần share**; guard same-agent + has-UserId fail-closed; không gửi cả cây CTV; Agent giữ tổng; route `/my-commissions`. Khớp U-M13-2.

## 7. Residual / chưa đo (ghi rõ — không tuyên bố 100%)

- **R-M13-A (runtime Hangfire/PostgreSQL E2E):** chưa dựng harness để chạy `NotificationJob` thật với: CTV trực tiếp A nhận / CTV cùng đại lý B không nhận / Director không nhận / Accountant+SuperAdmin nhận / dedup không tạo trùng khi job chạy lại. Rủi ro thấp (logic tĩnh + unit rõ), nhưng **chưa đo runtime**.
- **R-M13-B (U-M09-1 snapshot tỷ lệ CTV):** share tính bằng `CommissionSharePercentage` **hiện tại** của Collaborator (giống `/my-commissions`), CHƯA snapshot tại thời điểm commission phát sinh. Nếu đổi tỷ lệ sau khi commission đã phát sinh, notification mới dùng tỷ lệ mới. Đây là **backlog U-M09-1 đã chốt (change request M09)**, KHÔNG phải regression của BUG_M13_01 → không đánh fail, ghi rõ rủi ro lịch sử.
- **OBS-M13 cũ (timezone UTC biên, dedup vĩnh viễn non-LeadCare, MarkRead no-ownership) giữ nguyên** — non-blocking.

## 8. Cập nhật trạng thái

- `06-bug-report.md`: BUG_M13_01 + CR-M13-1 → **Verified Fixed (code-level)**.
- Board: M13 → `QA=Completed`, `Codex=Fixed`, `Verification=Verified (code-level)`. Gỡ BUG_M13_01 + CR-M13-1 khỏi Codex Queue.
- Verified bởi Claude phiên #7 (2026-07-11).
