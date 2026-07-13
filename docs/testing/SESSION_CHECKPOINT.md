# QA Session Checkpoint

## Session Summary

- **Thời điểm cập nhật:** 2026-07-13 (phiên #9 — fix CR-M14-2: thu hẹp danh bạ Phụ huynh/Học viên)
- **AI thực hiện:** Claude (Bug Fix theo yêu cầu trực tiếp của user)

## 🔧 Phiên #9 (2026-07-13) — CR-M14-3: MA TRẬN 5 BẬC (chấm dứt "nhắn loạn xạ")

> **📖 LUẬT GỐC: [`docs/messaging-tiers.md`](../messaging-tiers.md)** — mô hình 5 bậc + ma trận đầy đủ 12×12. Đọc file đó trước khi sửa luật nhắn tin.

| Hạng mục | Kết quả |
|---|---|
| **User báo** | "Tin nhắn đang nhắn loạn xạ với nhau." |
| **Root cause** | `MessagingPolicy.CanMessage` chỉ có vài luật rời rạc rồi kết thúc bằng **`return true`** → **mặc định MỞ** cho mọi cặp nhân sự nội bộ. Bậc 5 đã siết (CR-M14-1/2) nhưng bậc 2–4 thả nổi. |
| **Mô hình (user chốt)** | 5 bậc: (1) `super_admin` · (2) `director` · (3) `accountant`/`recruitment_manager`/`document_staff`/`visa_staff` · (4) `consultant`/`recruiter`/`agent` · (5) `parent`/`student`/`collaborator`. |
| **Quy tắc** | **SA hai chiều với tất cả** · **chênh bậc ≤ 1** · **3 ngoại lệ chặn**: TVV✗TVV, CTV✗CTV, **Đại lý ✗ toàn bộ bậc 4** (đối thủ + đối tác ngoài) · **tầng quan hệ ứng viên siết thêm** lên trên ma trận. |
| **Fix** | **MỚI** `Polymind.Domain/Messaging/MessagingTiers.cs` (ma trận thuần, fail-closed). `MessagingPolicy` ủy quyền cho nó (xóa fallback `return true`). `Messages.razor` gom luật vào `IsAllowedRecipient` dùng chung danh bạ + `Send`; bậc 5 nạp thêm SA vào danh sách đóng; thêm **bộ lọc theo vai trò** ở ô tìm kiếm. |
| **✅ ĐÓNG BLOCKER QA** | Ma trận role trước đây nằm ở `Polymind.Web` → phải **kiểm thủ công**. Nay ở Domain → `M14_MessagingMatrixTests` **56 case** phủ bằng máy, kèm test **chống lệch tên role** giữa Domain và `RoleNames.All`. |
| **Tests** | Full suite **208/208** (Failed 0, Skipped 0); Web build **0 Warning / 0 Error**. |
| **Ảnh hưởng (siết quyền)** | **Mất** liên lạc: GĐ✗TVV/NVTD/Đại lý, GĐ✗bậc 5, bậc 3✗bậc 5, Đại lý✗TVV/NVTD/Đại lý khác, TVV✗TVV. **Mới được**: Đại lý↔CTV của mình, bậc 5↔SA. **Không xóa dữ liệu** (tin cũ vẫn trong DB). |
| **Trạng thái** | `Codex=Fixed`, `Verification=Waiting for Fix` — chờ xác minh độc lập. Runtime E2E đa tài khoản pending harness. |

## 🔧 Phiên #9 (2026-07-13) — CR-M08-2 / CR-M09-3: siết quyền CTV

| Hạng mục | Kết quả |
|---|---|
| **User chốt** | CTV **không** thấy tổng hoa hồng + tổng doanh thu của đại lý. "Tổng doanh thu" chỉ **super_admin/kế toán/giám đốc**. CTV **không** vào được Đào tạo. |
| **Rà soát toàn bộ role** | Dashboard `/` và `/reports` **đã đúng từ trước** (gate `financial_reports:read`). **Lỗ hổng ở `/agents`**: cột "Hoa hồng" + "Doanh số (đã thu)" **không hề gate** → RM/NVTD/TVV/Đại lý/CTV đều thấy. |
| **Fix** | `DbSeeder`: gỡ `training:read` khỏi `Collaborator` (seeder tự **thu hồi** quyền thừa khi khởi động). `MyCommissions.razor`: ẩn KPI "Hoa hồng qua đại lý" + cột "Hoa hồng đại lý" với CTV. `Agents.razor`: gate 2 cột tiền bằng `financial_reports:read`; **đại lý chỉ thấy tiền của chính mình** (`CanSeeAgentMoney`/`CanSeeCtvMoney`). |
| **Tests** | +11 invariant ở `M20_SecurityInvariantsTests` (partner/portal ✗ `financial_reports:read`; CTV ✗ `training:read`; không cắt nhầm quyền khác của CTV; giữ `training:read` cho các role đã chốt ở U-M08-1). |
| **Trạng thái** | `Fixed` — chờ xác minh độc lập. |

## 🔧 Phiên #9 (2026-07-13) — CR-M14-2: danh bạ portal chỉ CTV + TVV + người nhà

> **User báo:** trang tin nhắn của tài khoản Học viên và Phụ huynh đang sai phạm vi.

| Hạng mục | Kết quả |
|---|---|
| **Quy tắc chốt** | **Học viên** → chỉ CTV, TVV, **phụ huynh** của mình. **Phụ huynh** → chỉ CTV, TVV, **học viên** của mình. Đối xứng: chỉ CTV/TVV của đúng ứng viên đó mới nhắn được portal. |
| **Root cause** | CR-M14-1 (phiên #8) định nghĩa "người phụ trách" quá rộng: `BuildRelationshipRecipientsAsync` gom `ConsultantId` **+ `Agent.UserId` + `CandidateJobOrder.AssignedTo` + `WorkflowStepRecord.AssignedTo` + `Visa.HandledBy` + `Flight.AssignedTo` → portal thấy cả đại lý và toàn bộ nhân sự hồ sơ/visa/workflow. |
| **Fix** | Domain: `MessagingCandidateScope.ForResponsibleUser` → **`ForConsultant`**; thêm factory **`CandidateMessagingRelationship.ForCandidate(student, parent, consultant, collaborator)`**. Web: participants chỉ còn `{ConsultantId, Collaborator.UserId}`; **đại lý (`IsAgentOnly`) fail-closed rỗng**; bỏ 4 query assignee + query `db.Agents`. |
| **Đối xứng (có chủ đích)** | Đại lý + NV hồ sơ/visa/workflow **không còn** nhắn được Phụ huynh/Học viên (trừ khi là TVV của ứng viên). `Send` re-check server-side dùng chung scope → guard cả UI lẫn mutation. Staff↔staff không đổi. |
| **Tests** | +5 regression CR-M14-2; `M14_MessagingRulesTests` **10/10**; full suite **141/141** (Failed 0, Skipped 0); Web build **0 Warning / 0 Error**. |
| **Trạng thái** | `Codex=Fixed`, `Verification=Waiting for Fix` — **chờ xác minh độc lập**. Runtime E2E đa tài khoản (Blazor/PostgreSQL) pending harness. |

## Phiên #8 (Claude 2026-07-11)

- **Thời điểm:** 2026-07-11 (Claude phiên #8 — verify M08/M14/M16/M17, unblock+verify M09 CR-M09-1/2, QA M20)
- **AI thực hiện:** Claude (Verification + QA Batch)

## 🏁 Phiên #8 (Claude 2026-07-11) — HOÀN TẤT TOÀN BỘ 20 MODULE

> Môi trường đã khôi phục: `dotnet restore/build/test` chạy sạch (blocker offline-restore của M09 đã hết). Docker/Postgres healthy → chạy được migration PoC.

| Hạng mục | Kết quả |
|---|---|
| **Gate chung** | Full suite **138/138** (Failed 0, Skipped 0); Web build `.qa/build/session8-web` **0 warning/0 error**. |
| **Verify M08** CR-M08-1 | **Verified (code):** `Read(...,"training")` cấp `training:read` cho Recruiter/Document/Visa/Accountant, KHÔNG mutation; RM/Consultant giữ `Crud`. M08 8/8. `08-verification-report.md`. |
| **Verify M14** CR-M14-1 | **Verified (code):** `CandidateMessagingRelationship` đối xứng fail-closed; `Send` re-query graph + recipient roles từ DB trước mutation (server-side). M14 7/7. |
| **Verify M16** BUG_M16_01+CR-M16-1 | **Verified (code):** `ExportHref` truyền range; endpoint 400 reversed + re-check `financial_reports:read` → 403; `VisibleExports`/`LoadData` ẩn+skip finance cho RM. M16 6/6. |
| **Verify M17** CR-M17-1 | **Verified (code):** `_canReadFinance` gate CẢ render lẫn query (Payments/Commissions/Agents); recruitment KPI vô điều kiện. Dùng chung policy M16. |
| **Unblock+Verify M09** CR-M09-1/2 | **Verified (code+runtime migration):** migration `20260711170000_SnapshotCollaboratorCommissionShare` áp sạch trên DB test `polymind_m09_verify` (cột `collaborator_id`/`collaborator_share_percentage` + index; unique idempotency BUG_M09_01 còn nguyên). CommissionEngine ghi snapshot mỗi mốc; MyCommissions/NotificationService đọc snapshot (không theo config hiện tại). `PartnerLeaderboardVisibility`: partner→agency mình (rank toàn cục giữ nguyên), staff đầy đủ, fail-closed. M09 17/17. DB test đã DROP. **⚠ Lưu ý kỹ thuật:** migration thiếu `.Designer.cs` → `dotnet ef --no-build` với binary cũ KHÔNG thấy migration; build tươi thì apply OK (residual R-M09-D, không blocker). |
| **QA M20** Security & Deployment | **No Confirmed Bugs.** Deliverables `01→06` + `M20_SecurityInvariantsTests` (16 case chống leo thang quyền dọc: partner/portal không có quyền tài chính/user/role/audit/commission; non-finance không `financial_reports:read`; fail-closed). 10 observation hardening (OBS-M20-01..10, hoãn tới prod theo deploy plan); **U-M20-1** (go-live hardening checklist) + **U-M20-2** (JWT revoke) chờ user. Đính chính: `DbSeeder` prod thiếu super_admin env → `LogError`+skip (không throw), vẫn an toàn (không lộ default cred). |
| **Codex Queue** | **TRỐNG** — tất cả bug/CR đã Verified Fixed, không còn item chờ Codex hay chờ Claude verify. |

## ⚠️ RÀNG BUỘC ÁP DỤNG CHO CẢ CLAUDE VÀ CODEX (user chốt 2026-07-11)
- **Mọi mục "cần user chốt" (U-*) phải được đưa RA RÕ RÀNG cho user quyết** (kèm tình trạng hiện tại + ví dụ dễ hiểu + đề xuất) — không AI nào tự quyết thay.
- **Codex KHÔNG được tự sửa bug đang gate bởi requirement chưa chốt** (VD BUG_M15_01 gate U-M15-1, BUG_M13_01 gate U-M13-2). Chỉ fix khi user đã chốt hướng.
- **Claude không sửa business logic** trong giai đoạn QA; chỉ đọc + chạy test + viết tài liệu + verify.
- **Cả hai:** không tuyên bố coverage 100% khi runtime chưa đo; ghi rõ Blocked/residual. Không né test/hard-code để pass.

## Phiên #6 (Claude 2026-07-11) — Verify M11 + QA M14(sync)/M15/M16

| Hạng mục | Kết quả |
|---|---|
| **M11 VERIFY** | BUG_M11_01 + CR-M11-1/2/3 → **Verified Fixed (code + RUNTIME).** Docker/Postgres đang chạy → tạo DB test `polymind_m11_verify`, `dotnet ef database update` áp sạch migration `20260711123000`; `\d receipts` xác nhận `loan_id`(index)/`loan_repayment_id`(UNIQUE). **DB PoC:** insert trùng `loan_repayment_id` → bị chặn; 2 phiếu NULL (thu hết) → cho phép (khớp residual). Suite 88/88, Web 0/0. `modules/M11-loans/08-verification-report.md`. DB test đã DROP. |
| **M14 SYNC** | Board ghi Pending nhưng M14 đã QA đầy đủ (01→06, No Confirmed Bugs, `M14_MessagingRulesTests`). Đồng bộ board = No Confirmed Bugs / Verified (code). OBS-M14-01 → **U-M14-1** (scoping tin nhắn bất đối xứng). |
| **M15 QA** | **BUG_M15_01 (Medium):** đại lý/CTV `!_selfScoped` → `BuildDataContextAsync` nạp TOÀN BỘ ứng viên/lead/job không lọc AgentId → lộ qua Trợ lý AI. CTV thấy icon AI (UI); agent vào bằng URL (`[Authorize]` không policy). Self-scoped (parent/student) cô lập đúng ở tầng dữ liệu; RB-5 logout-clear đúng. **0 automated test** (DTO/logic ở `Polymind.Web`, test project không ref Web). Gate **U-M15-1**. |
| **M16 QA** | **BUG_M16_01 (Low):** export Excel/PDF/CSV dùng link tĩnh → bỏ qua khoảng thời gian đang chọn (file luôn toàn kỳ). Phân quyền đúng (`reports:read` = Director/RM/Accountant/SuperAdmin; `receipts:read` finance-only). OBS-M16-01 receipt PDF IDOR **latent** (không khai thác được với seed). **0 automated test** (endpoint ở Web). **U-M16-1** (RM xem tài chính tổng?). |
| **M17 QA** | **No Confirmed Bugs.** Home `dashboard:read` staff-only + **partner redirect** `/my-commissions`; Portal `/me` cô lập theo `OwnedCandidateId`; không IDOR. **OBS-M17-01 → U-M17-1:** KPI tài chính (doanh thu/công nợ/**hoa hồng đại lý**) hiện cho MỌI staff (gồm recruiter/consultant/document/visa) — song song U-M16-1. **0 automated test.** |
| **M18 QA** | **No Confirmed Bugs.** MinIO: objectKey **server-gen** (không path traversal), extension whitelist, size limit, sanitize; upload **staff-only**. 3 hardening obs: OBS-M18-01 download không re-check scope (không exploit — versionId server-side + trang scoped), OBS-M18-02 content-type client (MinIO khác origin), OBS-M18-03 orphan object khi xóa. **0 automated test** (storage ở Web + cần MinIO). |
| **U-M13-1 chốt** | User chốt nốt: finance recipients = Kế toán + super_admin, **BỎ Giám đốc**; nhắc đóng tiền gửi cả người phụ trách + Kế toán (đã có). → **CR-M13-1** (bỏ Director ở `NotificationService.cs:266-270`). |
| Suite/Build | `dotnet test` → **88/88**; Web build **0/0**. Không sửa business logic; không đụng production DB. |

## Session Summary (Codex phiên trước)

- **Thời điểm:** 2026-07-11 (Codex — fix M11, tiếp nối phiên Claude #5)
- **AI:** Codex (Bug Fix + Regression Handoff)
- **Trọng tâm Codex hiện tại:** (1) **M11 Fixed — chờ Claude xác minh độc lập:** BUG_M11_01 + CR-M11-1/2/3, 82/82, Web 0/0, migration chưa áp DB. (2) **M13 Needs Requirement Clarification:** đã sửa phần chắc chắn Agent owner + Pending/Approved/Paid + Accountant/Director, null-safe; suite hiện **86/86**, Web 0/0. Chưa gửi CTV vì U-M13-2 chưa chốt direct/all-tree và nội dung số tiền.
- **Trọng tâm phiên #5 (Claude, trước):**
  1. **Verify M12 Visa & Flight** (BUG_M12_01/02) → **Verified Fixed (code-level)**. Đọc diff thật (2 dialog resolve `GetRequiredUserIdAsync` + `VisaFlightCreationRules`, edit path không ghi đè attribution), regression, `NotificationService:291-293` route visa reminder tới `HandledBy`. Viết `modules/M12-visa-flight/08-verification-report.md`. Suite **64/64**, Web build **0/0**.
  2. **QA mới M13 Notifications** (dep M07/M10/M12 đều Verified) → **Bugs Found**. **BUG_M13_01 (Medium):** thông báo hoa hồng không tới CTV/Đại lý liên quan (chỉ Accountant/Director) + thiếu event "đã chi" (Paid) → vi phạm RB-7. +5 unit contract (**69/69**). 5 observations; 2 clarification (U-M13-1/2).
- **Trọng tâm phiên #4 (trước):** Verify M06/M01/M09/M10; QA M11; QA M12.
- **Trọng tâm phiên #3:** QA M08/M09/M10; Codex fix queue M09/M10/M06/M01.
- **Trọng tâm phiên #2:** Verify M01/M03/M04; QA mới M05/M06/M07.
- **Nguyên tắc giữ:** không sửa business logic; chỉ đọc source + chạy test + viết tài liệu QA. Không tuyên bố 100% khi runtime chưa đo (ghi rõ Blocked).

## Phiên #5 (Claude 2026-07-11) — Verify M12 + QA M13

| Hạng mục | Kết quả |
|---|---|
| M12 verify | BUG_M12_01 (Med) + BUG_M12_02 (Low) → **Verified Fixed (code-level)**. `HandledBy`/`AssignedTo`=actor; edit không ghi đè; visa reminder route đúng; departure reminder dùng candidate owners (không dùng AssignedTo); permission re-check trước mutation; không né test/hard-code. `modules/M12-visa-flight/08-verification-report.md`. |
| M13 deliverables | `modules/M13-notifications/01→06` đầy đủ |
| M13 test cases | 42 (TC_M13_001..042) |
| M13 automated | +5 unit `M13_NotificationRulesTests` (NotificationType 10 giá trị/Channel 4/entity default) → **69/69 pass** |
| M13 bug | **BUG_M13_01 (Medium)** — Agent owner + Paid đã code; CTV scope/content còn mở → Needs Requirement Clarification |
| M13 observations | OBS-M13-01 recipient tài chính thiếu super_admin + payment owner-first (**U-M13-1**); 02 canSeeAll misnomer; 03 MarkRead no-ownership (không exploit UI); 04 timezone biên UTC; 05 dedup vĩnh viễn non-LeadCare |
| M13 điểm đúng ở code | visa reminder=HandledBy (M12) · RB-6 URL map đủ 9 nhánh · IDOR đọc lọc UserId · dedup unique+catch · mark-read idempotent · Labels có default · SendPending try/catch retry |

## Verification phiên #4 (Claude 2026-07-11) — tất cả Verified Fixed (code-level)

| Module | Bug | Verdict | Bằng chứng chính |
|---|---|---|---|
| M06 | BUG_M06_01 (Low) | **Verified Fixed** | Create path `GetRequiredUserIdAsync` + `JobOrderCreationRules.Create(actorId)`; edit path giữ `CreatedBy`; không còn first-user trong M06 |
| M01 | BUG_M01_03 (Low) | **Verified Fixed** | `PartnerAccountDialog.Unlink` dùng `UpdateSecurityStampAsync` + abort-on-failure; khớp Parent/Student |
| M09 | BUG_M09_01 (Med) | **Verified Fixed** | Unique index `(AgentId,CandidateId,Milestone)` (DbContext:152 + migration preflight RAISE, không auto-delete); `EnsureAsync`/`PersistAsync` retry hẹp đúng `IsIdempotencyConflict`, rethrow lỗi khác; 2 caller save trước Ensure |
| M09 | BUG_M09_02 (Low) | **Verified Fixed** | `AgentCommissionTransitions` + atomic `ExecuteUpdateAsync` predicate `Status==Pending/Approved` trong transaction, rollback nếu affected==0 |
| M10 | BUG_M10_01 (Med) | **Verified Fixed** | Chỉ 1 assignment `Paid` runtime (`PaymentPostingService`); 3 entry point cùng gọi service; dialog re-check `payments:approve`; tuần tự stage; thu lẻ không commission |

- **Suite:** `dotnet test` → **Passed 52, Failed 0, Skipped 0**. **Web build:** 0 Warning, 0 Error.
- **Residual (chưa đo, ghi rõ):** runtime create-as-RM (M06), partner multi-session (M01), race probe DB (M09), 3-entry posting probe (M10) — Claude chưa dựng lại DB/UI harness; dựa evidence Codex + phân tích tĩnh.

## M08 Training — QA phiên #3 (No Confirmed Bugs / Verified code)

| Hạng mục | Kết quả |
|---|---|
| Deliverables | `modules/M08-training/01→06` đầy đủ |
| Test cases | 34 (TC_M08_001..034) |
| Automated | +4 unit `M08_TrainingRulesTests` (enum/entity contract) → **33/33 pass** toàn suite |
| Bug | **Không có confirmed bug** |
| Observations | OBS-M08-01 concurrency no-rowversion (Low, cùng lớp M07-01); OBS-M08-02 recruiter/document/visa/accountant thiếu `training:read` (**req U-M08-1**); OBS-M08-03 `training:delete` seed nhưng không UI (**req U-M08-3**); OBS-M08-04 Load nạp toàn bộ JobOrders (perf nhẹ) |
| Điểm đúng ở code | authz page+dialog re-check · IDOR scope fail-closed · attribution actor thật (KHÔNG first-user) · clamp 0..100 · audit đủ · JSON attach try/catch |

## Kết quả xác minh Codex (M01/M03/M04)

| Module | Bug | Verdict | Ghi chú |
|---|---|---|---|
| M01 | BUG_M01_01 (High) | **Verified Fixed (code)** | Revalidation kiểm `IsActive` bao mọi đường khóa + stamp rotation ở 3 caller. Runtime ≤30' pending harness. |
| M01 | BUG_M01_02 (Low) | **Verified Fixed (code)** | Web+API dùng chung `InvalidCredentialsMessage`, API 401 đồng nhất; lockout vẫn hoạt động. |
| M01 | **BUG_M01_03 (Low, MỚI)** | **Fixed — chờ Claude xác minh** | Sweep phát hiện `PartnerAccountDialog.Unlink` khóa không xoay stamp; Codex đã căn chỉnh `UpdateSecurityStampAsync` + xử lý lỗi Identity. |
| M03 | BUG_M03_01 (Med) | **Verified Fixed (code)** | `UnlinkUser` đối xứng Owner/Parent + `DeleteUserAsync` query cả 2 field. Residual two-transaction (pre-existing). |
| M04 | BUG_M04_01 (Low) | **Verified Fixed (code)** | Convert dùng actor thật; anti-duplicate + mapping giữ nguyên. |

## M01 Authentication — Codex fix bổ sung (chờ Claude xác minh BUG_M01_03)

| Hạng mục | Kết quả |
|---|---|
| BUG_M01_03 | **Fixed** — Partner unlink dùng `UpdateSecurityStampAsync` + xử lý lỗi Identity trước khi commit unlink |
| Scope giữ nguyên | BUG_M01_01/02 không làm lại; giữ verdict Claude Verified code-level |
| Regression | Shared suite **52 passed, 0 failed, 0 skipped**; Web build **0 warning, 0 error** |
| Handoff | Cập nhật `modules/M01-authentication/07-fix-report.md`; chờ Claude verdict riêng cho BUG_M01_03 |

## QA mới hoàn thành phiên này

| Module | Deliverables | Test Cases | Bug/Kết quả | Status |
|---|---|---:|---|---|
| M05 Candidate | 01→06 (analysis hoàn chỉnh) | 30 | **No Confirmed Bugs** (IDOR API=BUG_M02_02 đã fix) | Verified (code) |
| M06 Job Orders | 01→06 | 17 | **1 bug: BUG_M06_01 (Low)** — CreatedBy=first-user | Bugs Found → Waiting Codex |
| M07 Workflow | 01→06 | 21 | **No Confirmed Bugs** | Verified (code) |

### Regression sweep quan trọng (Claude)
Anti-pattern **"first-user attribution"** (`db.Users.Select(u=>u.Id).FirstOrDefaultAsync()` thay actor) — cùng lỗi BUG_M04_01 — còn ở:
- `JobOrderDialog:154` (`CreatedBy`) → **BUG_M06_01** (đã file).
- `VisaDialog:136` (`HandledBy`) + `FlightDialog:128` (`AssignedTo`) → **M12** (file khi QA tới).
- `AuditLogHelpers:33` → fallback (obs, nên `throw`). `DemoDataSeeder:23` → chấp nhận (seed).

## M06 Job Orders — Codex fix (chờ Claude xác minh)

| Hạng mục | Kết quả |
|---|---|
| BUG_M06_01 | **Fixed** — create path lấy authenticated actor, không query user đầu DB |
| Regression | `JobOrderCreationRules` unit + shared suite **52 passed, 0 failed, 0 skipped** |
| Build | Web output riêng **0 warning, 0 error** |
| Handoff | `modules/M06-job-orders/07-fix-report.md`; M12/shared fallback chưa sửa, chờ module riêng |

## M09 Agents & Commissions — QA phiên #3 (Bugs Found)

| Hạng mục | Kết quả |
|---|---|
| Deliverables | `modules/M09-agents-commissions/01→06` đầy đủ |
| Test cases | 33 (TC_M09_001..033) |
| Automated | +4 unit `M09_CommissionRatesTests` (rate hoa hồng contract) → **37/37 pass** toàn suite |
| Bug | **BUG_M09_01 (Medium)** idempotency race (thiếu unique index) → hoa hồng trùng; **BUG_M09_02 (Low)** approve/pay không guard status |
| Giải quyết | **OBS-M07-02** — idempotency đúng tuần tự, hở concurrency |
| Cross-check | **U2 xác nhận:** RB-2 reset KHÔNG hoàn hoa hồng (exists guard) |
| Observations | OBS-M09-01 CTV share không snapshot (req); OBS-M09-02 leaderboard lộ doanh số (req); OBS-M09-03 CTV DB default 50≠35; OBS-M09-04 agent/CTV save không audit |

## M09 Agents & Commissions — Codex fix (chờ Claude xác minh)

| Hạng mục | Kết quả |
|---|---|
| BUG_M09_01 | **Fixed** — unique index `(AgentId,CandidateId,Milestone)` + migration preflight duplicate + retry đúng PostgreSQL unique constraint; `EnsureAsync` tự lưu commission/audit |
| BUG_M09_02 | **Fixed** — Domain transition rule + atomic conditional update trong transaction cho Pending→Approved→Paid |
| Dữ liệu cũ | PostgreSQL local: **0 duplicate groups / 20 rows**; không sửa/xóa dữ liệu |
| Runtime race | Database tạm, 12 caller đồng thời: **1 commission / 1 audit / tổng return 1** |
| Regression | Shared suite **48 passed, 0 failed, 0 skipped**; Web build output riêng **0 warning, 0 error** |
| Handoff | `modules/M09-agents-commissions/07-fix-report.md`; **chờ Claude xác minh độc lập** |

## M10 Finance — QA phiên #3 (Bugs Found)

| Hạng mục | Kết quả |
|---|---|
| Deliverables | `modules/M10-finance/01→06` đầy đủ |
| Test cases | 33 (TC_M10_001..033) |
| Automated | +4 unit `M10_FinanceRulesTests` (PaymentStage/Status/ReceiptType contract) → **41/41 pass** toàn suite (trước khi Codex sửa test M09) |
| Bug | **BUG_M10_01 (Medium)** — 3 đường set Payment→Paid không đồng nhất (chỉ `MarkStagePaid` ép tuần tự + trigger `CommissionEngine`; `ApprovePayment`/`PaymentDialog` thì không → thiếu hoa hồng) |
| Cross-check | **U2 xác nhận:** Finance KHÔNG có logic refund → reset đơn không hoàn khoản thu |
| Observations | OBS-M10-01 khoản chi không có luồng duyệt (req RB-7); OBS-M10-02 Code random-suffix va unique index; OBS-M10-03 PDF phiếu latent-IDOR; OBS-M10-04 nạp không phân trang gốc |
| Điểm đúng ở code | attribution actor thật · authz page/dialog/action re-check · IDOR self-scope lọc OwnedCandidateId · PDF gated receipts:read · receipt idempotent · split 20/30/30/20 bù dư |

## M10 Finance — Codex fix (chờ Claude xác minh)

| Hạng mục | Kết quả |
|---|---|
| BUG_M10_01 | **Fixed** — mọi runtime Payment→Paid qua `PaymentPostingService`: tuần tự + actor/date/audit + CommissionEngine |
| Authorization | PaymentDialog re-check thêm `payments:approve` khi transition sang Paid |
| Runtime PostgreSQL | Chặn ServiceFee trước Deposit; 4 stage Paid; đúng 3 commission; thu lẻ Paid không commission; 5 payment audits |
| Regression | Shared suite **51 passed, 0 failed, 0 skipped**; Web build **0 warning, 0 error** |
| Handoff | `modules/M10-finance/07-fix-report.md`; **chờ Claude xác minh độc lập** |

## Module vừa Verified (Claude phiên #8): **M08, M14, M16, M17** (code-level) + **M09 CR-M09-1/2** (code + runtime migration). **M20** QA mới (No Confirmed Bugs).
## Module đang active: — (không có; tất cả 20 module đã hoàn tất QA).
## Module tiếp theo đủ điều kiện: — (không còn module Pending). Việc tiếp theo là hardening prod (U-M20-1/2) + dựng runtime harness, chờ user ưu tiên.
## Module đang chờ Codex: — (Codex Queue TRỐNG). Change request cũ chưa lên lịch: M10 (U-M10-1), M12 (U-M12-1/2) — chờ user.
## Module cần user làm rõ: **U-M20-1** (go-live hardening checklist), **U-M20-2** (JWT revoke). Không chặn — hoãn tới production thật.
## Module đang chờ Claude xác minh: — (không còn; toàn bộ verification queue đã Verified).

## Codex session hiện tại — M15 hoàn tất

| Hạng mục | Kết quả |
|---|---|
| M15 fix | BUG_M15_01 → **Fixed, chờ Claude**. `AiDataScope` lọc candidate/lead/job theo Agent/CTV và fail-closed khi thiếu mapping; staff + self-scoped giữ nguyên. |
| Regression | M15 6/6 (gồm PostgreSQL SQL translation); toàn suite **94/94**. |
| Build | Web output riêng `.qa/build/m15`: **0 warning, 0 error**. |
| Runtime gap | Chưa chạy UI/Gemini E2E với DB/key thật; không dùng production key. |

## Codex session hiện tại — M13 hoàn tất

| Hạng mục | Kết quả |
|---|---|
| M13 fix | BUG_M13_01 + CR-M13-1 → **Fixed, chờ Claude**. CTV trực tiếp nhận share-only; finance chỉ Accountant/SuperAdmin. |
| Regression | M13 **15/15**; toàn suite **98/98**. |
| Build | Web output riêng `.qa/build/m13-final`: **0 warning, 0 error**. |
| Runtime gap | Chưa chạy Hangfire/PostgreSQL/UI recipient E2E. |

## Codex session hiện tại — M14 hoàn tất

| Hạng mục | Kết quả |
|---|---|
| M14 fix | CR-M14-1 → **Fixed, chờ Claude**. Candidate relationship áp đối xứng; danh bạ + Send re-check DB. |
| Regression | M14 **7/7**; toàn suite **106/106** (gồm 3 test M19 Claude thêm đồng thời). |
| Build | Web output riêng `.qa/build/m14-final`: **0 warning, 0 error**. |
| Runtime gap | Chưa chạy Blazor/PostgreSQL/MinIO E2E. |

## M12 Visa & Flight — Codex fix (chờ Claude xác minh)

| Hạng mục | Kết quả |
|---|---|
| BUG_M12_01 | **Fixed** — Visa create resolve authenticated actor; `HandledBy` không còn lấy user đầu DB nên nguồn recipient reminder visa đúng |
| BUG_M12_02 | **Fixed** — Flight create resolve authenticated actor; `AssignedTo` đúng người tạo |
| Regression | +2 attribution unit; shared suite **64 passed, 0 failed, 0 skipped** |
| Build | Web output riêng **0 warning, 0 error** |
| Handoff | `modules/M12-visa-flight/07-fix-report.md`; **chờ Claude xác minh độc lập**, Codex không đánh dấu Verified Fixed |

## M12 Visa & Flight — QA phiên #4 (Bugs Found)

| Hạng mục | Kết quả |
|---|---|
| Deliverables | `modules/M12-visa-flight/01→06` đầy đủ |
| Test cases | 30 (TC_M12_001..030) |
| Automated | +5 unit `M12_VisaFlightRulesTests` (VisaStatus + entity default/nullable) → **62/62 pass** toàn suite |
| Bug | **BUG_M12_01 (Medium)** VisaDialog `HandledBy` first-user → NotificationService:291 nhắc visa **sai người**; **BUG_M12_02 (Low)** FlightDialog `AssignedTo` first-user (cosmetic) |
| Observations | OBS-M12-01 no audit visa/flight (req); OBS-M12-02 no VisaStatus state-machine + no rowversion; OBS-M12-03 `ActualDepartureAt` không set runtime → report xuất cảnh rỗng (req U-M12-1); OBS-M12-04 no unique (candidate,job) |
| Điểm đúng ở code | page authorize · create AuthorizeView + Save re-check permission (visa+flight) · CJO auto-fill + khóa khi edit · RejectionReason chỉ khi Rejected · departure reminder recipient đúng (owners) · **không role scoped → không IDOR** |
| Sweep note đóng | first-user attribution `VisaDialog:136`+`FlightDialog:128` nay đã file bug (khép sweep từ M06) |

## M11 Loans & Debt Collection — Codex Fixed 2026-07-11, chờ Claude

| Hạng mục | Kết quả |
|---|---|
| Deliverables | `modules/M11-loans/01→07`; `07-fix-report.md` là handoff hiện tại |
| Fixed | BUG_M11_01 + CR-M11-1/2/3 (4/4) |
| Automated | M11 gate/status/collection/migration regression; toàn suite **82/82 pass** |
| Build | Web **0 warning / 0 error** bằng output riêng |
| Thay đổi chính | `LoanCollectionRules`; finance-only; receipt source migration; Thu hết; no-forgiveness; Bank không gate/không Settled |
| Residual | runtime PostgreSQL/UI chưa đo vì chưa có integration harness; migration chưa áp DB; OBS-M11-01/04/06 ngoài phạm vi |
| Trạng thái | `Codex=Fixed`, `Verification=Waiting for Fix`; **chờ Claude xác minh** |

## Codex Queue (ưu tiên cao → thấp)

| Order | Module | Bug ID | Severity | Status (sau khi user chốt 2026-07-11) |
|---:|---|---|---|---|
| 1 | M15 | BUG_M15_01 | Medium | **Verified Fixed (code-level) — Claude phiên #7** |
| 2 | M13 | BUG_M13_01 | Medium | **Verified Fixed (code-level) — Claude phiên #7** |
| 3 | M14 | CR-M14-1 | Change | **✅ Verified Fixed (code) — Claude phiên #8** — 7/7 M14, suite 138/138, Web 0/0 |
| 4 | M16 | CR-M16-1 | Change | **✅ Verified Fixed (code) — Claude phiên #8** — `financial_reports:read` server re-check 403 + UI/query guard |
| 5 | M16 | BUG_M16_01 | Low | **✅ Verified Fixed (code) — Claude phiên #8** — export range inclusive + 400 reversed; M16 6/6 |
| 6 | M13 | CR-M13-1 | Change | **Verified Fixed (code-level) — Claude phiên #7** |
| 7 | M17 | CR-M17-1 | Change | **✅ Verified Fixed (code) — Claude phiên #8** — `_canReadFinance` gate render+query |
| 8 | M09 | CR-M09-1 | Change | **✅ Verified Fixed (code+runtime migration) — Claude phiên #8** — snapshot + migration `20260711170000` áp sạch DB test |
| 9 | M09 | CR-M09-2 | Change | **✅ Verified Fixed (code) — Claude phiên #8** — `PartnerLeaderboardVisibility` fail-closed, rank toàn cục giữ nguyên |

> **✅ Codex Queue TRỐNG (Claude phiên #8):** tất cả bug/CR đã Verified Fixed. M08 CR-M08-1 + M20 (No Confirmed Bugs) cũng đã hoàn tất. Không còn item chờ Codex hay chờ Claude.

> M11 đã **Verified** (Claude phiên #6). M08 đã Fixed; M09 implementation xong nhưng Blocked final regression; M10/M12 còn backlog. Không còn quyết định nghiệp vụ nào chờ user.

## Module đang active

- **— Không có.** Tất cả 20 module đã hoàn tất QA + verification (Claude phiên #8). M09 đã rời trạng thái Blocked (CR-M09-1/2 Verified).

## Verification Queue

| Module ID | File | Status |
|---|---|---|
| M01 | `modules/M01-authentication/08-verification-report.md` | **Verified (code)** — BUG_M01_01/02/03 (BUG_M01_03 verified phiên #4) |
| M02 | `modules/M02-authorization/08-verification-report.md` | Verified (code); runtime HTTP pending |
| M03 | `modules/M03-user-management/08-verification-report.md` | Verified (code); runtime DB pending |
| M04 | `modules/M04-lead-crm/08-verification-report.md` | Verified (code); runtime pending |
| M05 | `modules/M05-candidate/06-bug-report.md` | No Confirmed Bugs / Verified (code) |
| M06 | `modules/M06-job-orders/08-verification-report.md` | **Verified (code)** — BUG_M06_01 (phiên #4) |
| M07 | `modules/M07-workflow/06-bug-report.md` | No Confirmed Bugs / Verified (code) |
| M08 | `modules/M08-training/08-verification-report.md` | **✅ Verified (code) — phiên #8:** CR-M08-1 read-only cho Recruiter/Document/Visa/Accountant, không mutation; M08 8/8. |
| M09 | `modules/M09-agents-commissions/08-verification-report.md` | **✅ Verified (code+runtime migration) — phiên #8:** BUG_M09_01/02 + CR-M09-1/2. Migration `20260711170000` áp sạch DB test; snapshot ghi/đọc; partner leaderboard fail-closed. M09 17/17. |
| M10 | `modules/M10-finance/08-verification-report.md` | **Verified (code)** — BUG_M10_01 (phiên #4); posting probe của Codex, Claude chưa dựng lại DB harness |
| M11 | `modules/M11-loans/08-verification-report.md` | **Verified (code+runtime migration/DB PoC)** — phiên #6 |
| M12 | `modules/M12-visa-flight/08-verification-report.md` | **Verified (code-level)** — phiên #5. Runtime E2E pending harness |
| M13 | `modules/M13-notifications/08-verification-report.md` | **Verified Fixed (code-level) — phiên #7:** BUG_M13_01 + CR-M13-1. Runtime E2E pending. |
| M14 | `modules/M14-messaging/08-verification-report.md` | **✅ Verified (code) — phiên #8:** CR-M14-1 relationship đối xứng + Send DB re-check; M14 7/7. |
| M15 | `modules/M15-ai/08-verification-report.md` | **Verified Fixed (code-level) — phiên #7:** partner AI context scoped fail-closed. Runtime E2E pending. |
| M16 | `modules/M16-reports/08-verification-report.md` | **✅ Verified (code) — phiên #8:** BUG_M16_01 range inclusive + CR-M16-1 financial 403 guard; M16 6/6. Runtime HTTP/file-content pending. |
| M17 | `modules/M17-dashboard/08-verification-report.md` | **✅ Verified (code) — phiên #8:** CR-M17-1 gate render+query finance. Runtime role-render/query pending. |
| M18 | `modules/M18-documents/06-bug-report.md` | No Confirmed Bugs / Verified (code) — phiên #6 |
| M19 | `modules/M19-audit/…` | Verified (code) — phiên #7 |
| M20 | `modules/M20-security-deploy/06-bug-report.md` | **✅ No Confirmed Bugs / Verified (static+unit) — phiên #8:** `M20_SecurityInvariantsTests` 16/16; 10 obs hardening; U-M20-1/2 chờ user. |

## Blockers

| Loại | Blocker | Required Action |
|---|---|---|
| Test infra | Chưa có harness integration (WebApplicationFactory + DB test) cho REST API/E2E/bUnit | Dựng session sau (Testcontainers hoặc DB `polymind_test`) → phủ runtime IDOR, cascade delete, RB-2 password, workflow advance, concurrency |
| Test infra | `BusinessRoleAccess`/`AgentScope`/`WorkflowStepAccess`/`WorkflowSteps` nằm ở `Polymind.Web` → không unit-test được từ test project | Tách sang `Polymind.Domain`/`Application` → unit-test ma trận role/scope/step trực tiếp. **✅ `MessagingPolicy` ĐÃ TÁCH (phiên #9)** → `Polymind.Domain/Messaging/MessagingTiers.cs`, phủ 56 case tự động. Các lớp còn lại vẫn chờ. |
| Test infra | Test project không ref `Polymind.Web`; khi dev server `:5177` chạy có thể khóa DLL Web | Dừng dev server khi build full; hoặc build Web ra output riêng |
| ~~Environment M09~~ | ~~Offline restore NU1101~~ | **✅ ĐÃ HẾT (phiên #8):** restore/build/test/migration chạy sạch; M09 CR-M09-1/2 đã Verified. |
| Migration hygiene (Low) | Migration `20260711170000_SnapshotCollaboratorCommissionShare` thiếu `.Designer.cs`/`BuildTargetModel` → `dotnet ef --no-build` với binary cũ không thấy; build tươi thì apply OK | R-M09-D: bổ sung Designer khi generate migration kế tiếp (không blocker runtime; ModelSnapshot là nguồn chuẩn) |

## Dependency Decisions

| Module | Dependency/Bug | Continue or Block | Reason |
|---|---|---|---|
| M05 | BUG_M02_02 (đã fix+verify) | **Continue** | IDOR REST đã đóng ở M02. |
| M06 | — | **Continue** | Không dep bug chưa fix. |
| M07 | BUG_M06_01 (Low, M06) | **Continue** | Attribution job-order không ảnh hưởng state-machine/phân quyền workflow. |
| M08+ | BUG_M06_01, BUG_M01_03 (Low) | **Resolved by Codex** | Cả hai đã Fixed, đang chờ Claude xác minh; không còn chặn dependency. |
| M08 | M05 (Verified) | **Continue** | Dep đủ; không dep bug chưa fix. |
| M09 | M02+M05 (Verified) | **Continue → hoàn thành QA** | Dep đủ; phát hiện BUG_M09_01/02. |
| M10 Finance | BUG_M09_01 (Medium, M09) | **Continue** | Idempotency race chỉ sai dữ liệu khi concurrency; luồng tuần tự Finance đúng → không làm sai kết quả test M10. Ghi rõ dep khi QA. |
| M11 Loans | M05+M10 (Verified) | **Codex Fixed → chờ Claude** | Gate Company-only; finance-only collection; Income Receipt; Thu hết; cấm Settled khi còn dư. 82/82, Web 0/0. |
| M12 Visa/Flight | M05+M07 (Verified) | **Continue → hoàn thành QA** | Dep đủ. Filed BUG_M12_01 (Med)/M12_02 (Low) first-user attribution. |
| M13 Notifications | M07+M10 (Verified) + M12 (Fixed, chờ verify) | **Continue (next)** | Source fix BUG_M12_01 đã sẵn sàng; Claude verify attribution/visa recipient trong QA M13. |
| (test project) | Verify M06/M09/M10/M01 + QA/fix M11/M12 | **Continue** | Suite hiện tại **82/82**; Web build M11 output riêng 0/0. |

## Needs Requirement Clarification

### ĐÃ CHỐT (user 2026-07-10)
- **U1 (M05) — ĐÃ CHỐT:** Collaborator (CTV) **ĐƯỢC xem số hộ chiếu/CCCD** của ứng viên mình giới thiệu. → Hành vi hiện tại (chỉ mask SĐT, hiện passport/CCCD) **ĐÚNG**. **Không phải bug.** R8/TC_M05_028 = Pass (spec confirmed). RB-1 giữ nguyên (chỉ ẩn 2 dòng hoa hồng/số ứng viên).
- **U2 (M05/M07) — ĐÃ CHỐT:** RB-2 đổi đơn hàng reset workflow 20 bước **KHÔNG hoàn/hủy khoản thu + hoa hồng đã phát sinh**. → Hành vi hiện tại **ĐÚNG** (khớp WORKLOG). **Không phải bug.** Vẫn verify chéo ở M09/M10 rằng KHÔNG có logic hoàn tiền vô tình.

### ĐÃ CHỐT (user 2026-07-11) — chuyển sang Codex thực thi (chi tiết + bảng ở MODULE_QA_BOARD)
- **🚫 QUY TẮC CỨNG — KHÔNG BAO GIỜ MIỄN NỢ:** nợ công ty chỉ tất toán khi **thu đủ 100% tiền thật**. "Đây là kinh doanh không phải làm từ thiện." Cấm mọi luồng miễn nợ/write-off; không cho set Settled khi còn dư nợ. (memory: `polymind-no-debt-forgiveness`)
- **U-M11-1 — CHỐT:** Thu nợ **chỉ kế toán/super_admin** → CR-M11-1.
- **U-M11-2 — CHỐT:** Thu nợ **sinh phiếu thu (Receipt income)** → CR-M11-2.
- **U-M11-3 — CHỐT:** Tất toán chỉ khi thu đủ 100% (thu-hết sinh receipt); **CHẶN** Settled thủ công khi còn nợ; **không miễn nợ**; finance-only → CR-M11-3.
- **BUG_M11_01 (Medium) — MỚI (từ giải thích user):** cổng B20 chặn cả **vay ngân hàng**. Chốt: **vay ngân hàng KHÔNG gate B20** + ẩn "Đã tất toán" khỏi dropdown khi Kind=Bank; chỉ **nợ công ty** gate B20. → M11 chuyển **Bugs Found**.
- **U-M10-1 — CHỐT:** Khoản chi **cần luồng duyệt** (RB-7) → change request M10.
- **U-M12-1 — CHỐT:** Thêm nút **"Xác nhận đã bay"** (`Flight.ActualDepartureAt`) ở FlightDialog → change request M12.
- **U-M12-2 — CHỐT:** Visa/flight **cần ghi audit** → change request M12.
- **U-M09-1 — CHỐT:** **Snapshot % hoa hồng CTV** tại thời điểm phát sinh (đóng băng lịch sử) → change request M09.
- **U-M09-2 — CHỐT:** **Ẩn doanh số với ĐỐI THỦ** đại lý (mỗi đại lý chỉ thấy thứ hạng mình); **KHÔNG ẩn với role khác** → change request M09.
- **U-M08-1 — CHỐT:** recruiter/document/visa/accountant **được xem** đào tạo (`training:read`) → change request M08.

## Files Created/Updated (phiên này)

**Codex fix M12 (hiện tại):**

| File | Purpose |
|---|---|
| `src/Polymind.Domain/Visas/VisaFlightCreationRules.cs` | Domain factory khóa HandledBy/AssignedTo theo actor |
| `VisaDialog.razor`, `FlightDialog.razor` | Bỏ first-user attribution, resolve authenticated actor ở create path |
| `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs` | Thêm 2 regression BUG_M12_01/02 (64/64) |
| `modules/M12-visa-flight/07-fix-report.md` | Handoff đầy đủ cho Claude verification |
| `modules/M12-visa-flight/06-bug-report.md` | BUG_M12_01/02 → Fixed — chờ Claude xác minh |
| `docs/testing/MODULE_QA_BOARD.md` | Codex queue rỗng; M12 Fixed / verification queue |
| `docs/testing/SESSION_CHECKPOINT.md` | Checkpoint hiện tại |

**Phiên #4 (hiện tại — Claude verify + QA M11):**

| File | Purpose |
|---|---|
| `modules/M06-job-orders/08-verification-report.md` | Verify BUG_M06_01 → Verified Fixed (code) |
| `modules/M09-agents-commissions/08-verification-report.md` | Verify BUG_M09_01/02 → Verified Fixed (code) |
| `modules/M10-finance/08-verification-report.md` | Verify BUG_M10_01 → Verified Fixed (code) |
| `modules/M01-authentication/08-verification-report.md` | Bổ sung verdict BUG_M01_03 → Verified Fixed (code) |
| `modules/M06/M09/M10/M01 06-bug-report.md` | Cập nhật Status bug → Verified Fixed |
| `modules/M11-loans/01→06` | QA M11 đầy đủ (No Confirmed Bugs; 6 observations) |
| `tests/Polymind.Tests/M11_LoanRulesTests.cs` | 5 unit contract LoanKind/LoanStatus/LoanRepaymentStatus/entity default |
| `modules/M12-visa-flight/01→06` | QA M12 đầy đủ (BUG_M12_01 Med, BUG_M12_02 Low; 4 observations) |
| `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs` | 5 unit contract VisaStatus/Visa/Flight default (62/62) |
| `docs/testing/MODULE_QA_BOARD.md` | M01/M06/M09/M10 → Verified; M11 → No Confirmed Bugs; M12 → Bugs Found; Codex queue = BUG_M12_01/02; sweep note đóng |
| `docs/testing/SESSION_CHECKPOINT.md` | Checkpoint này |

**Phiên #3 (trước):**

| File | Purpose |
|---|---|
| `modules/M08-training/01→06` | QA M08 đầy đủ (No Confirmed Bugs) |
| `modules/M09-agents-commissions/01→06` | QA M09 (BUG_M09_01 Med, BUG_M09_02 Low) |
| `modules/M10-finance/01→06` | QA M10 đầy đủ (BUG_M10_01 Med) |
| `tests/Polymind.Tests/M08_TrainingRulesTests.cs` | 4 unit contract enum/entity Training |
| `tests/Polymind.Tests/M09_CommissionRatesTests.cs` | 4 unit contract rate hoa hồng (Codex đã bổ sung test fix M09) |
| `tests/Polymind.Tests/M10_FinanceRulesTests.cs` | 4 unit contract PaymentStage/Status/ReceiptType |
| `modules/M09-agents-commissions/07-fix-report.md` | Codex handoff BUG_M09_01/02; race/runtime evidence + verification instructions |
| `Domain/Commissions/AgentCommissionTransitions.cs` + migration idempotency + Web callers | Fix M09 state machine/concurrency |
| `modules/M10-finance/07-fix-report.md` | Codex handoff BUG_M10_01; PostgreSQL posting evidence + verification instructions |
| `Domain/Finance/PaymentPostingRules.cs` + `Web/Finance/PaymentPostingService.cs` + Finance callers | Fix M10 thống nhất Payment→Paid |
| `modules/M06-job-orders/07-fix-report.md` | Codex handoff BUG_M06_01 + verification instructions |
| `Domain/JobOrders/JobOrderCreationRules.cs` + `JobOrderDialog.razor` | Fix M06 attribution actor |
| `modules/M01-authentication/07-fix-report.md` | Bổ sung handoff BUG_M01_03 + verification instructions |
| `Components/Pages/Agents/PartnerAccountDialog.razor` | Fix M01 partner stamp rotation/error handling |
| `docs/testing/MODULE_QA_BOARD.md` | Cập nhật M08/M09/M10 + queue + dep notes |
| `docs/testing/SESSION_CHECKPOINT.md` | Checkpoint này |

**Phiên #2 (trước):**

| File | Purpose |
|---|---|
| `modules/M01-authentication/08-verification-report.md` | Verify M01 (+ BUG_M01_03) |
| `modules/M01-authentication/06-bug-report.md` | Thêm BUG_M01_03 + cập nhật queue Verified |
| `modules/M03-user-management/08-verification-report.md` | Verify M03 |
| `modules/M04-lead-crm/08-verification-report.md` | Verify M04 |
| `modules/M05-candidate/01→06` | QA M05 đầy đủ (No Confirmed Bugs) |
| `modules/M06-job-orders/01→06` | QA M06 (BUG_M06_01) |
| `modules/M07-workflow/01→06` | QA M07 (No Confirmed Bugs) |
| `docs/testing/MODULE_QA_BOARD.md` | Cập nhật trạng thái M01-M07 + queue + sweep note |
| `docs/testing/SESSION_CHECKPOINT.md` | Checkpoint này |

## Test Commands

```bash
# KHÔNG chạy khi dev server :5177 đang rebuild Web (có thể khóa DLL). Phiên #6: port 5177 trống.
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore
# Codex M14: Passed 106, Failed 0, Skipped 0.
dotnet build src/Polymind.Web/Polymind.Web.csproj --no-restore --nologo -p:OutputPath="C:\Users\khang\OneDrive\Documents\POLYMIND APP\.qa\build\m14-final\"
# 0 Warning, 0 Error.

# M11 runtime verify (phiên #6) — Docker/Postgres đang chạy:
docker exec polymind-postgres psql -U polymind -d polymind -c "CREATE DATABASE polymind_m11_verify;"
dotnet ef database update --project src/Polymind.Infrastructure/Polymind.Infrastructure.csproj \
  --startup-project src/Polymind.Web/Polymind.Web.csproj \
  --connection "Host=localhost;Port=5432;Database=polymind_m11_verify;Username=polymind;Password=polymind"
# Áp sạch tới 20260711123000_LinkLoanDebtCollectionReceipts. Sau verify: DROP DATABASE polymind_m11_verify.
# LƯU Ý: design-time factory (ApplicationDbContextFactory) hardcode user 'postgres' → PHẢI dùng --connection, KHÔNG dùng env override.
```

## Exact Next Action

> **🏁 20/20 module đã qua QA + verification. Codex Queue TRỐNG. Không còn bug/CR chờ xử lý.**

- **User (quyết định — không chặn, hoãn tới production thật):**
  - **U-M20-1:** chốt go-live hardening checklist (CSP, gate Swagger prod, rate limit login/API, KnownProxies, Data Protection persist, non-root container, Gemini key ra khỏi tracked config, siết AllowedHosts).
  - **U-M20-2:** JWT có cần revoke tức thì khi khóa user/đổi role (thêm stamp check cho Bearer) hay chấp nhận 4h expiry?
  - Change request cũ chưa lên lịch (đã chốt hướng, chờ ưu tiên handoff Codex): M10 U-M10-1 (duyệt chi RB-7); M12 U-M12-1 (nút "Xác nhận đã bay"), U-M12-2 (audit visa/flight).
- **Claude (khi user chốt U-M20-1/2):** không tự sửa business logic; đưa observation → change request cho Codex.
- **Codex:** không có việc chờ. Khi user chốt U-M20-1/2 hoặc ưu tiên M10/M12 CR → thực thi hardening/change request.
- **Đề xuất phiên sau (giảm residual runtime):**
  1. Dựng **WebApplicationFactory + DB integration harness** (Testcontainers hoặc `polymind_test`) → phủ runtime IDOR, headers, 401/403, cascade delete, concurrency, migration apply cho toàn bộ module.
  2. **R-M09-D:** bổ sung `.Designer.cs` cho migration `20260711170000` khi generate migration kế tiếp (không blocker hiện tại).
  3. **Quick win refactor (giảm gap automation):** tách DTO AI (M15), `MessagingPolicy` (M14), export builders (M16) ra Domain/Application để unit-test không cần ref Web.

---

## 🟩 QUYẾT ĐỊNH CẦN USER CHỐT (đầy đủ — cập nhật phiên #6)

> Ràng buộc: mỗi mục dưới đây **cả Claude và Codex đều KHÔNG tự quyết**. Codex chỉ code sau khi user chốt.

### A. ✅ ĐÃ CHỐT phiên #6 (user 2026-07-11) — chuyển Codex thực thi

| # | Quyết định đã chốt | Codex làm gì |
|---|---|---|
| **U-M15-1** (BUG_M15_01, Med) | Đại lý/CTV **ĐƯỢC** dùng Trợ lý AI, nhưng AI **chỉ nạp ứng viên trong phạm vi của họ** (lọc AgentId/CollaboratorId). | Sửa `BuildDataContextAsync` lọc theo scope partner (như các màn khác). |
| **U-M14-1** (→ CR-M14-1) | **Giới hạn** staff/CTV/đại lý chỉ nhắn phụ huynh/học viên **thuộc ứng viên mình phụ trách**. | `MessagingPolicy` + `Messages.LoadContacts/Send` lọc theo quan hệ. |
| **U-M13-2** (BUG_M13_01, Med) | Thông báo hoa hồng gửi **CHỈ CTV trực tiếp** (`Candidate.CollaboratorId`), CTV **chỉ thấy phần share của mình** (không lộ tổng). | Bổ sung recipient CTV trực tiếp + nội dung chỉ phần share. |
| **U-M16-1** (→ CR-M16-1) | **Giới hạn** RM chỉ báo cáo tuyển dụng; bỏ báo cáo tài chính khỏi RM. | Tách quyền `reports:read` (finance vs recruitment) ở seed + Reports + endpoint. |
| **U-M13-1** (→ CR-M13-1) | Finance recipients = **Kế toán + super_admin, BỎ Giám đốc**; nhắc đóng tiền gửi cả người phụ trách + Kế toán (đã có). | Bỏ `RoleNames.Director` khỏi `financeRecipients` (`NotificationService.cs:266-270`). |
| **U-M17-1** (→ CR-M17-1) | **Ẩn KPI tài chính** trên Home dashboard — **chỉ Director/Accountant/SuperAdmin** thấy (recruiter/consultant/document/visa/RM chỉ KPI tuyển dụng). | Kiểm role hiển thị nhóm thẻ tài chính ở `Home.razor`. |

### B. Trạng thái quyết định

> **✅ KHÔNG CÒN quyết định nào chờ user.** Toàn bộ U-M13-1/2, U-M14-1, U-M15-1, U-M16-1, U-M17-1 đã chốt (2026-07-11) → 7 mục trong Codex Queue (xem bên dưới). Change request cũ M08/M09/M10/M12 vẫn chờ Codex.

### C. Đã chốt trước đó — đang chờ Codex thực thi (KHÔNG cần chốt lại, chỉ ưu tiên)

| # | Nội dung đã chốt | Module |
|---|---|---|
| U-M08-1 | recruiter/document/visa/accountant được xem Đào tạo (`training:read`) | M08 |
| U-M09-1 | Snapshot % hoa hồng CTV tại thời điểm phát sinh (đóng băng lịch sử) | M09 |
| U-M09-2 | Ẩn doanh số với **đối thủ** đại lý (mỗi đại lý chỉ thấy thứ hạng mình); không ẩn role khác | M09 |
| U-M10-1 | Khoản chi cần luồng duyệt (RB-7) | M10 |
| U-M12-1 | Thêm nút "Xác nhận đã bay" (`Flight.ActualDepartureAt`) | M12 |
| U-M12-2 | Visa/flight cần ghi audit | M12 |

> **Bug fix không cần chốt:** BUG_M16_01 (Low, export theo range) — Codex sửa được ngay.
