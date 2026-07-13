# MODULE QA BOARD — POLYMIND OLMS

> Bảng điều phối QA giữa Claude (QA) và Codex (Fix). Nguồn sự thật về trạng thái QA từng module.
> Đọc cùng `SESSION_CHECKPOINT.md`. Không dựa vào trí nhớ session trước — mọi trạng thái nằm ở đây.

- **Hệ thống:** POLYMIND OLMS — web quản lý xuất khẩu lao động (.NET 10 + Blazor Interactive Server + MudBlazor + EF Core + PostgreSQL).
- **Kiến trúc:** `Polymind.Domain` (entity/enum thuần) · `Polymind.Application` (rỗng — chưa dùng) · `Polymind.Infrastructure` (EF Core, Identity, seed, RBAC registry) · `Polymind.Web` (Blazor UI + REST API + Notification + AI). Business logic nằm trong Blazor components + `Web/Api` + `Web/Notifications`, KHÔNG nằm ở Application layer.
- **Test project:** `tests/Polymind.Tests` (xUnit) — tạo ở session này cho unit test logic thuần. Integration/E2E qua DB/UI ghi rõ ở phần coverage/gap của mỗi module (chưa có harness).

## Chú thích trạng thái

- **QA Status:** Pending · Analyzing · Test Cases Ready · Automated Tests Ready · Bugs Found · No Confirmed Bugs · Completed · Blocked
- **Codex Status:** Not Required · Waiting for Codex · Investigating · Fixed · Cannot Reproduce · Blocked · Needs Requirement Clarification · Returned to Codex
- **Verification Status:** Not Started · Waiting for Fix · Verifying · Verified · Partially Verified · Failed · Blocked

## Bảng module (sắp theo dependency + rủi ro)

| Order | Module ID | Module Name | Scope | Dependencies | Risk | QA Status | Codex Status | Verification Status | Folder |
|---:|---|---|---|---|---|---|---|---|---|
| 1 | M01 | Authentication & Session | Login web, `/api/auth/login` (JWT), cookie, lockout, security-stamp revalidation, logout | — | Critical | Completed | Fixed | Verified (BUG_M01_01/02/03 code) | `modules/M01-authentication` |
| 2 | M02 | Authorization, Roles & Permissions | RBAC permission claims, PermissionRegistry, role→permission seed map, PermissionAuthorizationHandler/PolicyProvider, AgentScope data-scope, MessagingPolicy, JWT permission claims | M01 | Critical | Completed | Fixed | Verified (code) | `modules/M02-authorization` |
| 3 | M03 | User & Account Management | `/admin`, AccountManagerPanel, ParentStudentAccounts, UserEditDialog, ConfirmPasswordDialog, change/reset password | M01, M02 | High | Completed | Fixed | Verified (code) | `modules/M03-user-management` |
| 4 | M04 | Lead CRM | Leads, LeadDetail, LeadDialog, LeadsConverted, LeadActivity, lead-care reminder, `/api/leads` | M02 | High | Completed | Fixed | Verified (code) | `modules/M04-lead-crm` |
| 5 | M05 | Candidate Management | Candidates, CandidateDetail, CandidateDialog, documents, self-scoped portal, parent/student link | M02, M04 | High | No Confirmed Bugs | Not Required | Verified (code) | `modules/M05-candidate` |
| 6 | M06 | Job Orders | JobOrders, JobOrderDetail, JobOrderDialog, JobCategory, cost/deadline | M02 | Medium | Completed | Fixed | Verified (code) | `modules/M06-job-orders` |
| 7 | M07 | Candidate Workflow (20 bước) | WorkflowStep, WorkflowStepRecord, CandidateJobOrder, đổi đơn hàng reset tiến trình | M05, M06 | High | No Confirmed Bugs | Not Required | Verified (code) | `modules/M07-workflow` |
| 8 | M08 | Training | Training, TrainingDetail, TrainingRecord, TrainingEvaluation, 2 track (tiếng/nghề) | M05 | Medium | No Confirmed Bugs | Fixed | **Verified (code) — CR-M08-1 phiên #8** | `modules/M08-training` |
| 9 | M09 | Agents & Commissions | Agents, AgentDetail, AgentsTree, Collaborator, AgentCommission, config, rates, MyCommissions | M02, M05 | High | Completed | Fixed | **Verified (code+runtime migration) — CR-M09-1/2 phiên #8** | `modules/M09-agents-commissions` |
| 10 | M10 | Finance (Payments & Expenses) | Finance, Payment, Expense, Receipt, PaymentStage 20/30/30/20, duyệt chi | M05, M06 | High | Completed | Fixed | Verified (code) | `modules/M10-finance` |
| 11 | M11 | Loans & Debt Collection | Loans, LoanDialog, DebtCollection, Loan, LoanRepayment (bank vs company) | M05, M10 | High | Completed | Fixed | **Verified (code+runtime migration/DB PoC)** | `modules/M11-loans` |
| 12 | M12 | Visa & Flight / Exit | Visas, VisaDialog, FlightDialog, VisaStatus, xuất cảnh | M05, M07 | Medium | Completed | Fixed | Verified (BUG_M12_01/02 code) | `modules/M12-visa-flight` |
| 13 | M13 | Notifications | NotificationService, NotificationJob (Hangfire), Notifications page, preferences, RB-6 điều hướng, RB-7 | M07, M10, M12 | Medium | Completed | Fixed | **Verified (code) — BUG_M13_01+CR-M13-1 phiên #7** | `modules/M13-notifications` |
| 14 | M14 | Messaging / Chat | Messages, MessagingTiers (ma trận 5 bậc), quan hệ ứng viên | M02 | **High** | Completed | Fixed | **Waiting for Fix — CR-M14-2/3 (phiên #9) chờ xác minh** | `modules/M14-messaging` |
| 15 | M15 | AI Assistant | AiAssistant, CandidateAnalysisDialog, GeminiClient, AiSessionStore (RB-5) | M05 | Medium | Completed | Fixed | **Verified (code) — BUG_M15_01 phiên #7** | `modules/M15-ai` |
| 16 | M16 | Reports & Export | Reports, CsvExportEndpoints, QuestPDF/ClosedXML | M10, M09 | Medium | Completed | Fixed | **Verified (code) — BUG_M16_01+CR-M16-1 phiên #8** | `modules/M16-reports` |
| 17 | M17 | Dashboard | Home (KPI/tổng quan) + Portal Overview | M02 | Low | No Confirmed Bugs | Fixed | **Verified (code) — CR-M17-1 phiên #8** | `modules/M17-dashboard` |
| 18 | M18 | File Upload / Documents | MinioDocumentStorage, CandidateDocument, DocumentVersion | M05 | Medium | No Confirmed Bugs | Not Required | Verified (code) | `modules/M18-documents` |
| 19 | M19 | Audit Log | AuditLog, `AddAudit`, nhật ký `/admin` | M02 | Medium | No Confirmed Bugs | Not Required | **Verified (code) — phiên #7** | `modules/M19-audit` |
| 20 | M20 | Security & Deployment | Security headers, cookie/JWT config, IDOR, rate limit, env/secret, production seed, docker | tất cả | High | No Confirmed Bugs | Not Required | **Verified (static+unit) — phiên #8** | `modules/M20-security-deploy` |

## Codex Handoff Queue (tổng hợp — chi tiết ở `06-bug-report.md` mỗi module)

| Order | Bug ID | Module | Severity | Suspected Area | Status |
|---:|---|---|---|---|---|
| ~~1~~ | ~~BUG_M15_01~~ | M15 | ~~Medium~~ | `AiAssistant.BuildDataContextAsync` lọc AgentId (fail-closed) | **✅ VERIFIED FIXED (code-level) — Claude phiên #7:** `AiDataScope`; 6/6 M15, suite 94/94, Web 0/0; `modules/M15-ai/08-verification-report.md`. **Đã rời queue.** |
| ~~2~~ | ~~BUG_M13_01~~ | M13 | ~~Medium~~ | Thông báo hoa hồng cho CTV | **✅ VERIFIED FIXED (code-level) — Claude phiên #7:** CTV trực tiếp/share-only/route `/my-commissions`, guard fail-closed; `modules/M13-notifications/08-verification-report.md`. **Đã rời queue.** |
| ~~3~~ | ~~CR-M14-1~~ | M14 | Change | Giới hạn tin nhắn staff/CTV/đại lý theo ứng viên phụ trách | **✅ VERIFIED FIXED (code) — Claude phiên #8:** symmetric relationship fail-closed + Send DB re-check; M14 7/7, suite 122/122, Web 0/0; `modules/M14-messaging/08-verification-report.md`. **Đã rời queue.** |
| ~~4~~ | ~~CR-M16-1~~ | M16 | Change | Bỏ báo cáo tài chính khỏi RecruitmentManager (RM chỉ báo cáo tuyển dụng) | **✅ VERIFIED FIXED (code) — Claude phiên #8:** `financial_reports:read` riêng + endpoint 403 server re-check + UI/query guard; `modules/M16-reports/08-verification-report.md`. **Đã rời queue.** |
| ~~5~~ | ~~BUG_M16_01~~ | M16 | **Low** | Export Excel/PDF/CSV dùng link tĩnh, bỏ qua khoảng thời gian đang chọn → file luôn toàn kỳ | **✅ VERIFIED FIXED (code) — Claude phiên #8:** `ExportHref` truyền range; endpoint 400 reversed; 8 builders inclusive; all-time backward-compatible. **Đã rời queue.** |
| ~~6~~ | ~~CR-M13-1~~ | M13 | Change | Thông báo tài chính bỏ Giám đốc (giữ Kế toán + super_admin) | **✅ VERIFIED FIXED (code-level) — Claude phiên #7:** `FinancialNotificationRules.RecipientRoleNames=[accountant,super_admin]`; Director loại khỏi mọi nhánh finance. **Đã rời queue.** |
| ~~7~~ | ~~CR-M17-1~~ | M17 | Change | Ẩn KPI tài chính (doanh thu/công nợ/hoa hồng đại lý) trên Home dashboard, chỉ Director/Accountant/SuperAdmin thấy — `Home.razor` | **✅ VERIFIED FIXED (code) — Claude phiên #8:** `_canReadFinance` gate cả render lẫn query Payments/Commissions/Agents; recruitment KPI vô điều kiện; `modules/M17-dashboard/08-verification-report.md`. **Đã rời queue.** |
| ~~8~~ | ~~CR-M09-1~~ | M09 | Change | Snapshot CTV + % share tại lúc commission phát sinh | **✅ VERIFIED FIXED (code+runtime migration) — Claude phiên #8:** snapshot fields + migration `20260711170000` áp sạch trên DB test; CommissionEngine ghi, MyCommissions/NotificationService đọc snapshot. **Đã rời queue.** |
| ~~9~~ | ~~CR-M09-2~~ | M09 | Change | Partner chỉ thấy doanh số/thứ hạng đại lý mình | **✅ VERIFIED FIXED (code) — Claude phiên #8:** `PartnerLeaderboardVisibility` + `Agents.Load` lọc board partner→agency mình (rank toàn cục giữ nguyên), staff đầy đủ, fail-closed. **Đã rời queue.** |

| 10 | **CR-M14-2** | M14 | Change (Med) | Danh bạ Học viên/Phụ huynh quá rộng — thấy cả đại lý + NV hồ sơ/visa/workflow | **🔧 FIXED (phiên #9, 2026-07-13) — chờ xác minh:** thu hẹp còn CTV + TVV + người nhà; đối xứng fail-closed. Bị bao trùm bởi CR-M14-3. |
| 11 | **CR-M14-3** | M14 | **Change (High)** | `MessagingPolicy.CanMessage` fallback `return true` → mọi cặp nhân sự nội bộ nhắn nhau vô tội vạ ("nhắn loạn xạ") | **🔧 FIXED (phiên #9, 2026-07-13) — chờ xác minh độc lập:** ma trận 5 bậc fail-closed ở Domain (`MessagingTiers`). Suite **208/208**, Web 0/0. Luật gốc: `docs/messaging-tiers.md`. |
| 12 | **CR-M08-2 / CR-M09-3** | M08/M09 | Change (Med) | CTV thấy tổng hoa hồng + doanh thu đại lý; CTV vào được trang Đào tạo; doanh số `/agents` không gate | **🔧 FIXED (phiên #9, 2026-07-13) — chờ xác minh:** gỡ `training:read` khỏi CTV; ẩn hoa hồng đại lý ở `/my-commissions`; gate cột tiền `/agents` bằng `financial_reports:read` (đại lý chỉ thấy tiền của chính mình). |

> **Codex Handoff Queue (phiên #9, 2026-07-13):** **CR-M14-2, CR-M14-3, CR-M08-2/CR-M09-3** đã Fixed, **chờ xác minh độc lập**. Các item phiên #8 đều Verified Fixed. Change request cũ chưa lên lịch: M10 (U-M10-1 duyệt chi RB-7), M12 (U-M12-1 nút "đã bay" + U-M12-2 audit) — chờ user ưu tiên.

> **📖 LUẬT NHẮN TIN — NGUỒN SỰ THẬT: [`docs/messaging-tiers.md`](../messaging-tiers.md)** (user chốt 2026-07-13, CR-M14-3).
> Mô hình **5 bậc**: (1) `super_admin` · (2) `director` · (3) `accountant`/`recruitment_manager`/`document_staff`/`visa_staff` · (4) `consultant`/`recruiter`/`agent` · (5) `parent`/`student`/`collaborator`.
> Quy tắc: **SA hai chiều với tất cả** · **chênh bậc ≤ 1** · **3 ngoại lệ chặn** (TVV✗TVV, CTV✗CTV, Đại lý✗toàn bộ bậc 4) · **tầng quan hệ ứng viên siết thêm** lên trên ma trận. Fail-closed.

> **Doanh thu/hoa hồng (user chốt 2026-07-13):** "tổng doanh thu" chỉ **super_admin / kế toán / giám đốc** (`financial_reports:read`) được xem toàn hệ thống. **Đại lý** chỉ xem tiền của **chính đại lý mình**. **CTV** và nhân sự còn lại (RM/NVTD/TVV/hồ sơ/visa): KHÔNG xem cột tiền. **CTV KHÔNG được vào module Đào tạo.**

> **M11 đã VERIFIED (Claude 2026-07-11 phiên #6):** BUG_M11_01 + CR-M11-1/2/3 → Verified Fixed. Suite 88/88, Web 0/0, migration `20260711123000` áp sạch trên DB test `polymind_m11_verify`, unique index `ix_receipts_loan_repayment_id` chặn thu-trùng (DB PoC). Xem `modules/M11-loans/08-verification-report.md`. Residual R-M11-A/B/C (Low, non-blocking).

> **M12 đã Verified Fixed (code-level)** — Claude 2026-07-11 (`modules/M12-visa-flight/08-verification-report.md`): BUG_M12_01/02 attribution = authenticated actor; visa reminder route đúng `HandledBy`; edit không ghi đè attribution. Suite 64/64, Web build 0/0. Change-request M12 (U-M12-1/2) chờ user ưu tiên.

**Lịch sử (đã đóng — Verified Fixed code-level, Claude 2026-07-11):**

| Bug ID | Module | Severity | Suspected Area | Verdict |
|---|---|---|---|---|
| BUG_M09_01 | M09 | Medium | `CommissionEngine.EnsureAsync` idempotency chỉ app-level; thiếu unique index (Agent,Candidate,Milestone) → race double pay | **Verified Fixed** (unique index + migration preflight + retry hẹp) |
| BUG_M10_01 | M10 | Medium | 3 đường set Payment→Paid không đồng nhất (tuần tự + trigger `CommissionEngine`) → thiếu hoa hồng | **Verified Fixed** (một `PaymentPostingService` cho mọi đường Paid) |
| BUG_M09_02 | M09 | Low | `AgentDetail` approve/pay hoa hồng không guard status server → stale-UI revert | **Verified Fixed** (atomic conditional update + Domain transition) |
| BUG_M06_01 | M06 | Low | `JobOrderDialog.Save` gán `CreatedBy` = user đầu tiên thay vì actor | **Verified Fixed** (factory nhận actorId + `GetRequiredUserIdAsync`) |
| BUG_M01_03 | M01 | Low | `PartnerAccountDialog.Unlink` không xoay security stamp khi khóa | **Verified Fixed** (`UpdateSecurityStampAsync` + error handling) |

> **Sweep note (Codex 2026-07-11):** BUG_M12_01/02 đã sửa bằng authenticated actor + Domain factory; chờ Claude xác minh. Toàn `src` chỉ còn `AuditLogHelpers:33` (fallback, observation — nên `throw`) + `DemoDataSeeder:23` (seed — chấp nhận) dùng first-user. BUG_M04_01/M06_01 đã Verified Fixed.

## Verification Queue

| Module ID | Fix Report | Status |
|---|---|---|
| M02 | `modules/M02-authorization/08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-10. Runtime HTTP PoC còn chờ app mới/harness. |
| M01 | `modules/M01-authentication/07-fix-report.md` + `08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-11: BUG_M01_03 partner unlink rotate stamp + xử lý IdentityResult, khớp Parent/Student. BUG_M01_01/02 giữ verdict Verified. Suite 52/52, Web build 0/0. |
| M03 | `modules/M03-user-management/08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-10: BUG_M03_01 Verified Fixed. Runtime DB delete + residual two-transaction risk pending harness. |
| M04 | `modules/M04-lead-crm/08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-10: BUG_M04_01 Verified Fixed. Runtime convert attribution + race R3 (pre-existing) pending harness. |
| M05 | `modules/M05-candidate/06-bug-report.md` | **No Confirmed Bugs / Verified (code)** — Claude 2026-07-10: IDOR web + RB-1 + RB-2 + delete AuthZ đúng ở source; IDOR REST = BUG_M02_02 (đã fix). **U1 đã chốt: CTV được xem passport/CCCD → không bug.** |
| M06 | `modules/M06-job-orders/08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-11: BUG_M06_01 create path dùng `GetRequiredUserIdAsync` + factory; edit path giữ `CreatedBy`; suite 52/52, Web build 0/0. Runtime create-as-RM pending harness. |
| M09 | `modules/M09-agents-commissions/08-verification-report.md` | **✅ Verified (code+runtime migration) — Claude phiên #8:** BUG_M09_01/02 + CR-M09-1/2 đều Verified Fixed. Migration `20260711170000` áp sạch DB test (cột snapshot + index); snapshot ghi/đọc đúng; partner leaderboard fail-closed. M09 17/17, suite 122/122, Web 0/0. Residual R-M09-D (thiếu Designer.cs migration, không blocker). |
| M10 | `modules/M10-finance/08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-11: BUG_M10_01 chỉ 1 assignment Paid runtime (`PaymentPostingService`); 3 entry point cùng gọi service, dialog re-check `payments:approve`, tuần tự stage. Suite 52/52, Web build 0/0. Posting probe của Codex; Claude chưa dựng lại DB harness. |
| M07 | `modules/M07-workflow/06-bug-report.md` | **No Confirmed Bugs / Verified (code)** — Claude 2026-07-10: phân quyền chuyển bước + state-machine + attribution đúng ở source. OBS-M07-01 (concurrency, no rowversion) theo dõi M17/M20. **U2 đã chốt: RB-2 reset KHÔNG hoàn tiền/hoa hồng → đúng, không bug.** |
| M08 | `modules/M08-training/08-verification-report.md` | **✅ Verified (code) — Claude phiên #8:** CR-M08-1 read-only training cho Recruiter/Document/Visa/Accountant; không mở mutation; RM/Consultant giữ Crud. M08 8/8, suite 122/122, Web 0/0. |
| M11 | `modules/M11-loans/08-verification-report.md` | **Verified (code+runtime) — Claude phiên #6:** BUG_M11_01 + CR-M11-1/2/3 Verified Fixed. Gate chỉ Company; finance-only; mỗi lần thu sinh Income Receipt; Thu hết + cấm Settled khi còn dư; không miễn nợ. Suite 88/88, Web 0/0. **Migration đã áp sạch trên DB test** + DB unique-index PoC chặn thu-trùng. E2E UI thật pending harness. |
| M12 | `modules/M12-visa-flight/08-verification-report.md` | **Verified (code-level)** — Claude 2026-07-11: BUG_M12_01/02 attribution = authenticated actor (diff sạch, edit path không ghi đè); visa reminder `NotificationService:291-293` route đúng `HandledBy`; departure reminder không dùng `AssignedTo`. Suite 64/64, Web build 0/0. Runtime E2E multi-user + NotificationJob pending harness. |
| M13 | `modules/M13-notifications/08-verification-report.md` | **Verified Fixed (code-level) — Claude phiên #7:** BUG_M13_01 + CR-M13-1; suite 101/101 tại thời điểm verify, Web 0/0. Runtime Hangfire/DB pending. |
| M14 | `modules/M14-messaging/08-verification-report.md` | **✅ Verified (code) — Claude phiên #8:** CR-M14-1 relationship đối xứng fail-closed; `Send` re-query graph + recipient roles từ DB trước mutation. M14 7/7, suite 122/122, Web 0/0. Runtime DB/Blazor/MinIO pending. |
| M15 | `modules/M15-ai/08-verification-report.md` | **Verified Fixed (code-level) — Claude phiên #7:** BUG_M15_01 scoped fail-closed; suite 94/94, Web 0/0. Runtime Gemini/UI pending. |
| M16 | `modules/M16-reports/08-verification-report.md` | **✅ Verified (code) — Claude phiên #8:** BUG_M16_01 export range inclusive + 400 reversed; CR-M16-1 `financial_reports:read` server re-check 403 + UI/query guard, RM chỉ recruitment. M16 6/6, suite 122/122, Web 0/0. Runtime HTTP/file-content pending. |
| M17 | `modules/M17-dashboard/08-verification-report.md` | **✅ Verified (code) — Claude phiên #8:** CR-M17-1 `_canReadFinance` gate render + query (Payments/Commissions/Agents); recruitment KPI vô điều kiện. Suite 122/122, Web 0/0; runtime role-render/query pending. |
| M18 | `modules/M18-documents/06-bug-report.md` | **No Confirmed Bugs / Verified (code)** — Claude phiên #6: objectKey server-gen (không path traversal), extension whitelist + size + sanitize, upload staff-only. OBS-M18-01/02/03 hardening (download re-check scope, content-type, orphan MinIO) — không exploit hiện tại. 0 automated test (storage ở Web + cần MinIO). |

## Ghi chú dependency / quyết định tiếp tục hay chặn

> ⚠️ **LƯU Ý (Claude phiên #8):** Phần dưới là **nhật ký lịch sử tích lũy qua các phiên** — một số dòng ghi "Codex Fixed, chờ Claude" đã CŨ. **Trạng thái chuẩn hiện tại nằm ở Bảng module + Codex Handoff Queue (TRỐNG) + Verification Queue phía trên** (tất cả 20 module đã Verified tính đến phiên #8). Khi mâu thuẫn, ưu tiên bảng module.

- **M01 BUG_M01_03 — Codex Fixed, chờ Claude:** `PartnerAccountDialog.Unlink` nay dùng `UpdateSecurityStampAsync` + dừng unlink khi Identity lỗi, khớp Parent/Student. BUG_M01_01/02 giữ verdict Claude Verified code-level; shared suite 52/52, Web build 0/0.
- **M02 đã Verified (code):** REST Candidate API áp data-scope fail-closed; IDOR REST đóng. Runtime JWT/API pending harness.
- **M05/M07 QA mới — No Confirmed Bugs (Verified code):** authorization/IDOR/RB-1/RB-2/state-machine đúng ở source. Runtime UI + concurrency (OBS-M07-01) pending harness.
- **M08 Training — Codex Fixed, chờ Claude:** CR-M08-1 mở `training:read` cho Recruiter/Document/Visa/Accountant nhưng giữ mutation đóng; M08 8/8, suite 116/116, Web 0/0. Observations concurrency/delete/perf giữ nguyên.
- **M09 Agents & Commissions — ✅ Verified (Claude phiên #8):** BUG_M09_01/02 + CR-M09-1/2 đều Verified Fixed. Blocker offline-restore đã hết; full restore/build/test/migration chạy sạch. CR-M09-1 snapshot CTV/% (migration `20260711170000` áp sạch DB test, CommissionEngine ghi + MyCommissions/NotificationService đọc snapshot); CR-M09-2 `PartnerLeaderboardVisibility` (partner→agency mình, rank toàn cục giữ nguyên, fail-closed). M09 rời trạng thái Blocked.
- **M10 Finance — Codex Fixed, chờ Claude:** **BUG_M10_01** gom mọi runtime Payment→Paid vào `PaymentPostingService`: kiểm tuần tự, actor/date/audit, save, CommissionEngine; dialog re-check thêm `payments:approve`. PostgreSQL probe chặn out-of-order, Paid đủ 4 stage sinh đúng 3 commission, thu lẻ không sinh commission. Shared suite 51/51, Web build 0/0. U2 no-refund và observations chưa đổi.
- **M06 Job Orders — Codex Fixed, chờ Claude:** `JobOrderDialog` resolve actor thật và tạo qua `JobOrderCreationRules`; regression attribution pass, shared suite 52/52, Web build 0/0. M12/shared fallback không nằm trong fix này.
- **Regression sweep first-user attribution:** `VisaDialog`/`FlightDialog` đã Fixed, chờ Claude; chỉ còn fallback `AuditLogHelpers:33` (observation) và seeder.
- **Quyết định quyền kế toán (user chốt 2026-07-10):** accountant được `approve` khoản thu, khoản chi, hoa hồng và khoản vay. Role map `AllActions` đúng.
- **Đã user chốt 2026-07-10:** **U1** — CTV ĐƯỢC xem passport/CCCD (không mask) → hành vi hiện tại đúng, **không phải bug**. **U2** — RB-2 đổi đơn hàng reset workflow **KHÔNG** hoàn tiền/hoa hồng → hành vi hiện tại đúng, **không phải bug** (verify chéo M09/M10 rằng không có hoàn tiền vô tình).
- **M11 Loans — Codex Fixed, chờ Claude:** `LoanCollectionRules` chốt gate/status/outstanding/collection; thu nợ chỉ Accountant/SuperAdmin, tạo Income Receipt gắn Loan/Repayment trong transaction, có Thu hết và chặn Settled khi còn dư; Bank không gate B20/không có Settled. 82/82, Web build 0/0; migration chưa áp DB test.

### Requirement/quyết định user chốt 2026-07-11

| Req | Quyết định | Module | Loại | Trạng thái |
|---|---|---|---|---|
| U-M11-1 | Thu nợ **chỉ kế toán/super_admin** | M11 | CR-M11-1 | **Fixed — Waiting Claude** |
| U-M11-2 | Thu nợ **sinh phiếu thu (Receipt income)** | M11 | CR-M11-2 | **Fixed — Waiting Claude** |
| U-M11-3 | **Tất toán CHỈ khi thu đủ 100% (KHÔNG BAO GIỜ miễn nợ)**; thu-hết sinh receipt; CHẶN Settled thủ công khi còn nợ; finance-only. Vay ngân hàng ẩn "Đã tất toán" + **không gate B20** (BUG_M11_01) | M11 | CR-M11-3 + BUG_M11_01 | **Fixed — Waiting Claude** |
| U-M10-1 | Khoản chi **cần luồng duyệt** (RB-7) | M10 | Change request | Ready for Codex (khi QA/fix M10 mở rộng) |
| U-M12-1 | Thêm nút **"Xác nhận đã bay"** (`Flight.ActualDepartureAt`) ở FlightDialog | M12 | Change request | Ready for Codex |
| U-M12-2 | Visa/flight **cần ghi audit** | M12 | Change request | Ready for Codex |
| U-M09-1 | **Snapshot % hoa hồng CTV** tại thời điểm phát sinh (đóng băng, lịch sử bất biến) | M09 | CR-M09-1 | **✅ Verified (code+runtime migration) — Claude phiên #8** |
| U-M09-2 | **Ẩn doanh số với ĐỐI THỦ** đại lý (mỗi đại lý chỉ thấy thứ hạng của mình); **KHÔNG ẩn với role khác** | M09 | CR-M09-2 | **✅ Verified (code) — Claude phiên #8** |
| U-M08-1 | recruiter/document/visa/accountant **được xem** module Đào tạo (`training:read`) | M08 | CR-M08-1 | **✅ Verified (code) — Claude phiên #8** |
| U-M13-1 | Recipient thông báo **Tài chính**. | M13 | OBS-M13-01 → **CR-M13-1** | **Verified Fixed (code-level) — Claude phiên #7.** |
| U-M13-2 | Phạm vi **CTV liên quan** + mức tiền hiển thị. | M13 | BUG_M13_01 | **Verified Fixed (code-level) — Claude phiên #7.** |
| U-M14-1 | Giới hạn tin nhắn theo quan hệ ứng viên. | M14 | OBS-M14-01 → **CR-M14-1** | **✅ Verified (code) — Claude phiên #8:** staff/Agent/CTV chỉ portal của candidate phụ trách; portal reply đối xứng; Send re-check DB. |
| U-M15-1 | Đại lý/CTV dùng Trợ lý AI + phạm vi dữ liệu. | M15 | BUG_M15_01 | **✅ Verified (code) — Claude phiên #7.** |
| U-M16-1 | RM xem báo cáo tài chính tổng? | M16 | OBS-M16-03 → **CR-M16-1** | **✅ Verified (code) — Claude phiên #8:** RM chỉ báo cáo tuyển dụng; Director/Accountant/SuperAdmin giữ tài chính; UI + direct export (403) đều guard. |
| U-M17-1 | KPI tài chính trên Dashboard. | M17 | OBS-M17-01 → **CR-M17-1** | **✅ Verified (code) — Claude phiên #8:** chỉ Director/Accountant/SuperAdmin có UI/query tài chính; RM/staff còn lại chỉ KPI tuyển dụng. |
- **M12 Visa & Flight — Verified Fixed (code-level), Claude 2026-07-11:** BUG_M12_01/02 dùng authenticated actor + `VisaFlightCreationRules`; visa reminder route theo `HandledBy` (nguồn đã đúng), departure reminder dùng candidate owners (không dùng `AssignedTo`); edit path không ghi đè attribution; permission re-check trước mutation. 2 regression; shared suite 64/64; Web build 0/0. Observations/requirement mở (U-M12-1/2) không sửa. → `QA=Completed`, `Codex=Fixed`, `Verification=Verified`.
- **M11 Loans — Codex fix 2026-07-11:** BUG_M11_01 + CR-M11-1/2/3 Fixed, không sửa test để né lỗi, không có write-off. Receipt source migration additive; 82/82, Web 0/0. → `QA=Completed`, `Codex=Fixed`, `Verification=Waiting for Fix`; **chờ Claude xác minh độc lập**.
- **M13 Notifications — Verified Fixed (code-level), Claude phiên #7:** BUG_M13_01 + CR-M13-1; runtime Hangfire/DB pending.
- **M14 Messaging — Codex Fixed, chờ Claude:** CR-M14-1 áp relationship đối xứng candidate; danh bạ + Send re-check DB; 7/7 M14, suite 106/106, Web 0/0.
- **M15 AI Assistant — Verified Fixed (code-level), Claude phiên #7:** BUG_M15_01 scoped fail-closed; runtime UI/Gemini pending.
- **M16 Reports & Export — Codex Fixed, chờ Claude:** BUG_M16_01 + CR-M16-1; export range cho CSV/XLSX/PDF, RM chỉ recruitment, tài chính guard server-side. 6 regression M16; suite 112/112, Web 0/0. Runtime HTTP/file-content và observations latent/perf còn pending.
- **M17 Dashboard — Codex Fixed, chờ Claude:** CR-M17-1 dùng `financial_reports:read` guard UI + query tài chính; suite 112/112, Web 0/0. Portal và KPI tuyển dụng không đổi.
- **M18 Documents — No Confirmed Bugs (Claude phiên #6):** MinIO storage an toàn (objectKey server-gen → không path traversal, whitelist ext, size, sanitize, upload staff-only). 3 observation hardening (download re-check scope, content-type, orphan object) — không exploit hiện tại (MinIO khác origin, Blazor Server binding, trang scoped M05).
- **🏁 TẤT CẢ 20 MODULE ĐÃ QUA QA (Claude phiên #8):** M01–M20 đều đạt `QA=Completed`/`No Confirmed Bugs` + `Verification=Verified`. Không còn module Pending. Codex Handoff Queue TRỐNG.
- **M20 Security & Deployment — No Confirmed Bugs (phiên #8):** không lỗ hổng khai thác được; 10 observation hardening (OBS-M20-01..10) hoãn tới production thật; **U-M20-1** (go-live hardening checklist) + **U-M20-2** (JWT revoke) chờ user chốt. `M20_SecurityInvariantsTests` 16/16 chống leo thang quyền dọc.
- **Việc còn lại (chờ user ưu tiên, chưa handoff Codex):** U-M20-1/2 (hardening/JWT revoke); change request cũ M10 (U-M10-1 duyệt chi RB-7), M12 (U-M12-1 nút "đã bay", U-M12-2 audit). Không có bug đang chờ.
- **Residual xuyên suốt (không blocker):** chưa có WebApplicationFactory/HTTP+DB integration harness → runtime E2E của nhiều module ghi rõ Blocked; nên dựng harness ở phiên sau để đo runtime IDOR/headers/403/concurrency.
