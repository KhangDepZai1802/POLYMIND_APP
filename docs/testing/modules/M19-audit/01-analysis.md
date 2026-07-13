# M19 — Audit Log · Analysis

## 1. Module Overview

- **Module ID:** M19
- **Module name:** Audit Log (Nhật ký thao tác)
- **Business purpose:** Ghi nhật ký kiểm toán "ai đã làm gì, ở mục nào, lúc nào" cho các thao tác nghiệp vụ quan trọng (create/update/delete/approve/mark_paid/assign/advance_step/link/unlink/reset_password/...), phục vụ truy vết và đối chiếu khi có tranh chấp. Yêu cầu phi chức năng xuyên suốt các module.
- **Actor:** Mọi staff/partner thực hiện thao tác ghi (tạo bản ghi audit); người XEM nhật ký = Giám đốc + super_admin.
- **Role đọc:** `audit:read` — chỉ **Director** và **SuperAdmin** (seed). Không cấp cho RM/Accountant/Recruiter/... 
- **Dependencies:** M02 (Authorization — policy `audit:read`, `users:read` gate trang `/admin`); ghi audit là side-effect của hầu hết module nghiệp vụ (M01→M18).
- **Entry point:** Ghi = `ApplicationDbContext.AddAudit(...)` gọi trong luồng nghiệp vụ (chung `SaveChanges`); Xem = `/admin` → tab "Nhật ký thao tác".
- **Exit point:** Hàng `audit_logs` trong DB (append-only theo quy ước); hiển thị 200 dòng mới nhất trên UI.

## 2. Source Code Map

| File | Class/Component | Method | Mục đích | Dependency |
|---|---|---|---|---|
| `src/Polymind.Domain/Entities/AuditLog.cs` | `AuditLog : BaseEntity` | — | Entity nhật ký: `UserId?`, `Action`, `Resource`, `ResourceId?`, `OldValue?` (jsonb), `NewValue?` (jsonb), `IpAddress?`, `UserAgent?` | `BaseEntity` (Id/CreatedAt/UpdatedAt) |
| `src/Polymind.Domain/Common/BaseEntity.cs` | `BaseEntity` | — | `Id=Guid.NewGuid()`, `CreatedAt=UtcNow`, `UpdatedAt=UtcNow` | — |
| `src/Polymind.Web/Auditing/AuditLogHelpers.cs` | `AuditLogHelpers` (static) | `AddAudit(db, userId, action, resource, resourceId, oldValue?, newValue?)` | Thêm 1 `AuditLog` vào change-tracker (KHÔNG tự SaveChanges); serialize old/new bằng `JsonSerializer` + `JsonStringEnumConverter` | `ApplicationDbContext` |
| " | " | `GetUserIdAsync(authStateProvider)` | Lấy `Guid?` actor từ claim `NameIdentifier` | AuthStateProvider |
| " | " | `GetRequiredUserIdAsync(authStateProvider, db)` | Actor bắt buộc; **fallback `db.Users.Select(u=>u.Id).FirstAsync()` khi null** (rủi ro mis-attribution — xem Risk) | AuthStateProvider, db |
| `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs` | `ApplicationDbContext` | `OnModelCreating` (dòng 161-167) | Map `AuditLog`: index `(Resource,ResourceId)` + `(UserId,CreatedAt)`; `OldValue`/`NewValue` = jsonb | EF Core / Npgsql |
| " | " | `DbSet<AuditLog> AuditLogs` | Truy vấn/ghi | — |
| `src/Polymind.Web/Components/Pages/Admin/Admin.razor` | `Admin` | `LoadAuditAsync` (316-337) | Query `audit:read`; filter resource/action; `OrderByDescending(CreatedAt).Take(200)`; resolve tên user; map nhãn VN | `IDbContextFactory` |
| " | " | `AuditActionLabel/ResourceLabel/Color/Summary`, `ShortTechnicalId`, `NormalizeAuditFilter`, `HumanizeTechnicalName` | Hiển thị thân thiện (nhãn VN, màu chip, mã kỹ thuật rút gọn) | — |
| " | " | `@attribute [Authorize(Policy="users:read")]` + tab `AuthorizeView Policy="audit:read"` | Chặn truy cập trang + tab nhật ký | M02 |
| `src/Polymind.Infrastructure/Persistence/DbSeeder.cs` | `DbSeeder` | `RolePermissionMap` | Cấp `audit:read` cho **Director**; **SuperAdmin** nhận tất cả permission (dòng 149) | PermissionRegistry |
| `src/Polymind.Infrastructure/Persistence/PermissionRegistry.cs` | `PermissionRegistry` | — | "audit" là 1 resource hợp lệ (sinh `audit:read/create/update/delete/approve`) | — |
| Migration `20260624034033_InitialCreate` | — | — | Tạo bảng `audit_logs` + 2 index | — |

### Nơi GHI audit (call sites `AddAudit`, ~40) — theo module
- **M01 Auth:** `ChangePasswordDialog` (`change_password`), `PartnerAccountDialog` (`link_account`/`unlink_account`).
- **M03 User:** `AccountManagerPanel` (`update_role`, `lock`/`unlock`, `delete`), `UserEditDialog` (`update`), `Admin.razor` (`update` role_permissions).
- **M04 Lead:** `LeadDialog`/`LeadDetail`/`LeadsEndpoints` (`create`/`update`/`delete`/`assign`/`revert_to_lead`).
- **M05 Candidate:** `CandidateDialog`/`CandidateDetail` (`create`/`update`/`delete`/`reassign_people`/`change_job_order`/`assign`/`advance_step`/`fail_step`/`reselect_job_order`/`overseas_log`/`restore_version`/`upload_version`), `ParentAccountDialog`/`StudentAccountDialog` (`link_parent`/`unlink_parent`/`link_student`/`unlink_student`).
- **M06 Job:** `JobOrderDetail` (`delete`).
- **M08 Training:** `TrainingTrackDialog`/`TrainingEvaluationDialog` (`create`/`update`).
- **M09 Commission:** `CommissionEngine` (`create` agent_commissions), `AgentDetail` (`approve`/`mark_paid`), `CommissionConfigDialog` (`create`/`update`).
- **M10 Finance:** `PaymentPostingService`/`PaymentDialog`/`Finance` (`create`/`update` payments/receipts), `ExpenseDialog` (`create`/`update`).
- **M11 Loans:** `LoanDialog`/`Loans`/`DebtCollection`/`CandidateDetail` (`create`/`update`/`delete`/`collect_debt`).
- **M12 Visa:** (VisaDialog/FlightDialog audit = OBS-M12-01, hiện CHƯA ghi audit — nằm ở M12 change request U-M12-2, không thuộc M19).

## 3. UI Inventory

- **Trang:** `/admin` → tab "Nhật ký thao tác" (chỉ hiện khi có `audit:read`; nếu không → `MudAlert` "Bạn không có quyền xem nhật ký thao tác.").
- **Filter:** 2 ô text — "Lọc theo khu vực" (resource), "Lọc theo thao tác" (action) + nút "Lọc".
- **Table:** cột Thời gian (`dd/MM/yyyy HH:mm` local) · Người thực hiện (FullName hoặc "Hệ thống"/"—") · Việc đã làm (chip màu theo action) · Khu vực (nhãn VN) · Ghi chú (summary + "Mã kỹ thuật" 8 ký tự đầu của ResourceId).
- **Empty state:** bảng rỗng nếu không có log khớp filter.
- **Loading:** `MudProgressLinear` khi `_loading`.
- **KHÔNG có:** phân trang, chọn khoảng ngày, xem chi tiết Old/New value, export. (xem OBS.)

## 4. API Inventory

- **Không có REST API riêng cho audit.** Ghi/đọc đều qua Blazor Server + EF (`IDbContextFactory`). Không endpoint public → không bề mặt IDOR REST.
- Đọc: `LoadAuditAsync` (server-side, sau khi qua `AuthorizeView audit:read`). Ghi: side-effect trong `SaveChanges` của luồng nghiệp vụ.

## 5. Database Impact

- **Table:** `audit_logs`.
- **Cột:** `id` (PK, Guid), `user_id` (Guid?, **không FK cứng** tới users — audit sống sót khi xóa user), `action` (text, required), `resource` (text, required), `resource_id` (Guid?, **không FK** — audit sống sót khi xóa resource), `old_value` (jsonb?), `new_value` (jsonb?), `ip_address` (text?), `user_agent` (text?), `created_at`, `updated_at`.
- **Index:** `ix_audit_logs_resource_resource_id (Resource,ResourceId)`, `ix_audit_logs_user_id_created_at (UserId,CreatedAt)` → hỗ trợ filter theo resource + sort/filter theo user/thời gian.
- **Constraint:** không unique, không cascade — bản chất append-only, độc lập vòng đời resource.
- **State field:** không có (audit là bản ghi bất biến, không có trạng thái).

## 6. Roles và Permissions

| Action | Role | UI Permission | API Permission | Business Condition | Source |
|---|---|---|---|---|---|
| Xem nhật ký | SuperAdmin, Director | `audit:read` (tab + `[Authorize users:read]` trang) | — (không REST) | — | `Admin.razor:2,128`, `DbSeeder:41,149` |
| Xem nhật ký | RM/Accountant/Recruiter/Consultant/... | ❌ không có `audit:read` | — | thấy alert "không có quyền" | `DbSeeder` (không cấp) |
| Ghi nhật ký | mọi actor thực hiện thao tác nghiệp vụ | (không cần quyền riêng — là side-effect) | — | ghi kèm SaveChanges của thao tác | `AuditLogHelpers.AddAudit` |

## 7. Risk Analysis

- **[R1] IpAddress/UserAgent KHÔNG được ghi:** `AddAudit` không nhận/không set 2 field này; không call site nào set → 2 cột luôn NULL dù entity định nghĩa cho mục đích forensic. → **OBS-M19-01** (requirement clarification: có cần IP/UA không).
- **[R2] Login/Logout KHÔNG được ghi audit:** XML doc entity ghi "mọi thao tác CRUD/**đăng nhập**", nhưng `Login.razor` (web) và `AuthEndpoints` (API) chỉ set `LastLoginAt`, KHÔNG `AddAudit`; không có action `login`/`logout` ở bất kỳ call site. → **OBS-M19-02** (gap vs entity doc; requirement clarification).
- **[R3] Mis-attribution qua fallback first-user:** `GetRequiredUserIdAsync` khi actor null → `db.Users.Select(u=>u.Id).FirstAsync()` → audit có thể gán thao tác cho user đầu DB thay vì null/throw. Với **nhật ký kiểm toán**, sai "ai" nguy hại hơn các module khác. Thực tế chỉ kích hoạt khi caller không xác thực (trang đã `[Authorize]` nên hiếm). → **OBS-M19-03** (khuyến nghị throw/null cho audit; cùng lớp `AuditLogHelpers:33`).
- **[R4] History không ghi / ghi lệch giao dịch:** `AddAudit` chỉ Add vào change-tracker; audit + thay đổi nghiệp vụ commit CHUNG 1 `SaveChanges` (cùng DbContext) → **nguyên tử** (đã đối chiếu `LeadDetail.Delete`, `AccountManagerPanel.AddAuditAsync`, `PaymentPostingService`). Không thấy đường ghi audit ở transaction tách rời có thể rớt một nửa. ✅ đúng.
- **[R5] Bất biến/chống sửa:** không UI/endpoint xóa/sửa audit; không code nào `Remove` audit_logs → append-only theo quy ước app. Không enforce DB-level immutability (super_admin/DB trực tiếp có thể sửa) → **OBS-M19-05** (Low, non-exploit qua app).
- **[R6] Rò rỉ qua view:** view chỉ hiện khi `audit:read` (Director+super_admin), hiển thị toàn hệ thống (không cần data-scope vì là chức năng quản trị) → không IDOR. ✅.
- **[R7] Hiệu năng/đầy đủ:** `Take(200)` → log cũ không xem được qua UI; không phân trang/khoảng ngày/export → **OBS-M19-04** (Low completeness/perf).
- **[R8] Nhãn hiển thị:** switch `AuditActionLabel` có key `create_receipt`/`reset_password` không bao giờ được emit (receipts ghi action `create`; đổi mật khẩu ghi `change_password`) → hiển thị rơi về `HumanizeTechnicalName` (vẫn đọc được) → **OBS-M19-06** (cosmetic, Low).
- **[R9] jsonb old/new:** serialize bằng `JsonStringEnumConverter` → enum đọc được; không log secret rõ ràng (ChangePassword chỉ log `PasswordChanged=true`, KHÔNG log mật khẩu) ✅.

## 8. Unknowns / Needs Requirement Clarification

- **U-M19-1:** Nhật ký kiểm toán có **bắt buộc** ghi **login/logout** và **IpAddress/UserAgent** không? (Entity doc gợi ý có, nhưng RB-7 chỉ nói về *thông báo*, không nói audit; user từng chốt "KHÔNG thêm nhóm Tài khoản/Bảo mật" cho *notification* — không rõ áp cho audit.) → cần user chốt trước khi coi R1/R2 là bug.
- **U-M19-2:** Có cần **phân trang/khoảng ngày/export** nhật ký (R7) cho vận hành thật không? (hiện 200 dòng mới nhất.)
