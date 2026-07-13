# M04 — Lead CRM · Phân tích

## 1. Module Overview

- **Module ID:** M04
- **Module name:** Lead CRM (Quản lý khách hàng tiềm năng)
- **Business purpose:** Quản lý vòng đời Lead từ tiếp nhận → chăm sóc → chuyển thành ứng viên. Gồm CRUD Lead, đổi trạng thái (10 trạng thái), phân công tư vấn viên, lịch hẹn tư vấn, lịch sử chăm sóc (LeadActivity), nhắc chăm sóc quá hạn (LeadCareRules), và chuyển Lead → Ứng viên (bắt đầu quy trình 20 bước).
- **Actor:** super_admin, recruitment_manager, recruiter, consultant, document_staff (theo permission `leads:*` + `BusinessRoleAccess`).
- **Role:** tạo/sửa cần `leads:create/update` + role hợp lệ; xóa cần `leads:delete` + role hẹp hơn; convert cần `leads:update` + `candidates:create`.
- **Dependencies:** M02 (permission `leads:*`, `candidates:create`). Tạo ra dữ liệu cho M05 (Candidate) khi convert. Sinh thông báo M13 (ReminderLeadCare).
- **Entry point:** `/leads` (list), `/leads/{id}` (detail), `/leads/converted`; REST `/api/leads` (CRUD).
- **Exit point:** Lead ở trạng thái kết thúc (Converted/Unsuitable/Cancelled) hoặc bị xóa; convert → tạo `Candidate`.

## 2. Source Code Map

| # | File | Symbol | Method | Mục đích | Dependency |
|---|---|---|---|---|---|
| 1 | [Leads.razor](../../../../src/Polymind.Web/Components/Pages/Leads/Leads.razor) | `Leads` | `Load`, `Filtered`, `OpenCreate` | List + tìm kiếm/lọc (client-side) + phân trang (MudDataGrid) + chuông quá hạn; ẩn lead đã convert | DbFactory, AuthorizationService |
| 2 | [LeadDetail.razor](../../../../src/Polymind.Web/Components/Pages/Leads/LeadDetail.razor) | `LeadDetail` | `UpdateStatus`, `AssignLead`, `SaveAppointment`, `Convert`, `RevertToLead`, `DeleteLead` | Chi tiết + đổi trạng thái + phân công + lịch hẹn + convert/revert + xóa | DbFactory, AuthorizationService, LeadCareRules, BusinessRoleAccess |
| 3 | [LeadDialog.razor](../../../../src/Polymind.Web/Components/Pages/Leads/LeadDialog.razor) | `LeadDialog` | (create/edit form) | Form tạo/sửa Lead (validation field) | UserManager?, DbFactory |
| 4 | [LeadsConverted.razor](../../../../src/Polymind.Web/Components/Pages/Leads/LeadsConverted.razor) | `LeadsConverted` | — | Danh sách Lead đã chuyển thành ứng viên | DbFactory |
| 5 | [LeadsEndpoints.cs](../../../../src/Polymind.Web/Api/LeadsEndpoints.cs) | `LeadsEndpoints` | GET/POST/PUT/DELETE `/api/leads` | REST CRUD Lead + phân trang + audit | DbFactory |
| 6 | [LeadCareRules.cs](../../../../src/Polymind.Web/Display/LeadCareRules.cs) | `LeadCareRules` | `ThresholdHours`, `NextAction`, `TryGetOverdue`, `DurationLabel` | Rule nhắc chăm sóc (thuần, không AI) | Domain enum |
| 7 | [BusinessRoleAccess.cs](../../../../src/Polymind.Web/Display/BusinessRoleAccess.cs) | `BusinessRoleAccess` | `CanEditLead`, `CanDeleteLead` | Siết thêm theo role (trên permission) | RoleNames |
| 8 | [Lead.cs](../../../../src/Polymind.Domain/Entities/Lead.cs) / [LeadActivity.cs](../../../../src/Polymind.Domain/Entities/LeadActivity.cs) | entity | — | Lead + lịch sử chăm sóc | — |
| 9 | [NotificationService.cs](../../../../src/Polymind.Web/Notifications/NotificationService.cs) | — | (lead-care) | Job nhắc chăm sóc quá hạn: UPDATE hàng cũ (tránh vỡ khóa unique 23505) | LeadCareRules |

## 3. UI Inventory
- **Trang:** `/leads` (bảng desktop MudDataGrid + phân trang; card mobile), `/leads/{id}`, `/leads/converted`.
- **Tìm kiếm/lọc:** ô tìm (tên/SĐT/mã/tỉnh/quốc gia — client-side), lọc Trạng thái, lọc Nguồn. **Sort:** mặc định theo ngày tạo giảm dần (không có control sort riêng).
- **Form:** LeadDialog (tạo/sửa). **Nút:** Thêm Lead (gate `leads:create`), Sửa/Xóa (theo quyền), đổi trạng thái, phân công, lịch hẹn, Chuyển thành ứng viên.
- **Chuông quá hạn:** icon đỏ khi `LeadCareRules.TryGetOverdue`; banner cảnh báo ở detail.
- **State:** loading (progress), empty ("Không có lead phù hợp"), converted → banner + link.

## 4. API Inventory

| Method | Route | Policy | Request | Response | DB side effect | Audit | Error |
|---|---|---|---|---|---|---|---|
| GET | `/api/leads` | `leads:read` | search/status/page/pageSize | PagedResult<LeadDto> | — | — | — |
| GET | `/api/leads/{id}` | `leads:read` | — | LeadDto / 404 | — | — | 404 |
| POST | `/api/leads` | `leads:create` | LeadCreateRequest | 201 LeadDto | tạo Lead (Code auto) | create | 400 thiếu FullName |
| PUT | `/api/leads/{id}` | `leads:update` | LeadUpdateRequest | 200 LeadDto | cập nhật + UpdatedAt | update | 400/404 |
| DELETE | `/api/leads/{id}` | `leads:delete` | — | 204 | xóa Lead | delete | 404 |

- **Data-scope:** `leads:read` chỉ role nội bộ (staff) có → staff xem mọi Lead là đúng nghiệp vụ (không như candidates). API KHÔNG lọc theo assignee nhưng chấp nhận được (Lead dùng chung). Ghi nhận để M20 xác nhận lại.

## 5. Database Impact

| Bảng | Thao tác |
|---|---|
| `leads` | CRUD; `status`, `assigned_to`, `appointment_at`, `updated_at`, `collaborator_id`, `agent_id` |
| `lead_activities` | thêm record khi đổi trạng thái/phân công/lịch hẹn/convert (audit nghiệp vụ) |
| `candidates` | convert → tạo Candidate (LeadId=lead.Id); xóa lead → set Candidate.LeadId=null |
| `notifications` | xóa lead → xóa notification `ReferenceType=lead`; job lead-care UPDATE hàng cũ |
| `audit_logs` | create/update/assign/delete/revert_to_lead |

- **Timezone:** `AppointmentAt`, `UpdatedAt` lưu `UtcNow`/UTC offset 0 (đúng quy ước Postgres). Lịch hẹn: convert local→UTC đúng.
- **State field:** `Lead.Status` (10 trạng thái). Không có concurrency token riêng cho Lead.

## 6. Roles và Permissions

| Action | Permission | Role narrowing (BusinessRoleAccess) | Nguồn |
|---|---|---|---|
| Xem list/detail | `leads:read` | — | Leads/LeadDetail:2 |
| Tạo | `leads:create` | — (chỉ role có leads:create: RM/recruiter/consultant + super_admin) | Leads:14 |
| Sửa/phân công/lịch hẹn/đổi trạng thái | `leads:update` + `CanEditLead` (super_admin, RM, recruiter, consultant, doc_staff) | ✓ | LeadDetail:649 |
| Xóa | `leads:delete` + `CanDeleteLead` (super_admin, RM, doc_staff) | ✓ | LeadDetail:652 |
| Convert → ứng viên | `leads:update` + `candidates:create` | — | LeadDetail:570 |

- **Enforcement:** dùng `AuthorizationService.AuthorizeAsync(user, null, "resource:action")` server-side (không chỉ ẩn UI) + `BusinessRoleAccess` — tốt, chặn cả gọi trực tiếp.

## 7. Risk Analysis (đã đối chiếu source)

1. **[XÁC NHẬN — Low] `Convert()` gán sai `Candidate.CreatedBy`.** Dùng `adminId = await db.Users.Select(u => u.Id).FirstOrDefaultAsync()` (user ĐẦU TIÊN, không OrderBy) thay vì actor thật (có sẵn qua `AuthStateProvider.GetRequiredUserIdAsync(db)` — dùng ở các method khác cùng file). → CreatedBy quy sai người tạo hồ sơ ứng viên (sai truy vết/attribution; nếu user đầu tiên bị xóa → CreatedBy dangling). → **BUG_M04_01**.
2. **[Theo dõi — Low] Tìm kiếm/lọc client-side toàn bộ.** `Load` nạp TẤT CẢ lead chưa convert vào bộ nhớ rồi `Filtered` lọc phía client → không phân trang server; dữ liệu lớn tốn RAM/chậm. REST `/api/leads` thì có phân trang server (tốt). → rủi ro hiệu năng khi scale.
3. **[Theo dõi — Low] Convert đồng thời có thể tạo trùng ứng viên.** `Convert` kiểm `existingId` trong method + nút Disabled khi `_busy`, nhưng KHÔNG có unique constraint trên `Candidate.LeadId` → 2 tab/2 request đồng thời có thể vượt kiểm và tạo 2 ứng viên. Xác suất thấp.
3b. **[Theo dõi] Chuyển ngược (RevertToLead)** kiểm đủ 6 loại dữ liệu (documents/payments/CJO/visa/flight/commission) trước khi xóa candidate — an toàn, không mất dữ liệu ngoài ý.
4. **[OK] Chống tạo trùng cơ bản, xóa dọn liên kết** (candidate.LeadId=null, activities, notifications) — tốt.
5. **[OK] Lịch hẹn quá khứ bị chặn**; timezone UTC đúng.
6. **[OK] Permission server-side** qua AuthorizationService + BusinessRoleAccess (không chỉ ẩn UI).
7. **[OK] Lead-care reminder** tránh vỡ khóa unique (23505) bằng UPDATE hàng cũ (đã sửa Session 57) — verify ở M13.

## 8. Unknowns / Cần làm rõ
- **U1:** `Candidate.CreatedBy` được dùng cho mục đích gì (audit/hiển thị/scoping)? Ảnh hưởng mức nghiêm trọng BUG_M04_01. Giả định: audit/attribution → Low.
- **U2:** Có cần phân trang server + tìm kiếm server cho `/leads` (khi số lead lớn) không? — nghiệp vụ/hiệu năng.
- **U3:** Data-scope Lead: có role nào chỉ được xem lead mình phụ trách không, hay mọi staff xem chung? (hiện: chung).
