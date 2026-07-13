# M02 — Authorization, Roles & Permissions · Business Flows

## BF-M02-01 — Kiểm tra quyền khi truy cập trang/endpoint

- **Actor/Role:** mọi user đã đăng nhập · **Preconditions:** đã có claim permission (nạp lúc login).
- **Main flow:** truy cập trang có `[Authorize(Policy="resource:action")]` → `PermissionPolicyProvider` tạo policy → `PermissionAuthorizationHandler`: có claim khớp **hoặc** super_admin → Succeed → render; ngược lại → `/access-denied` (cookie) / 403 (API).
- **Source:** PermissionAuthorization.cs. **Risk:** claim stale sau khi đổi phân quyền (BUG_M02_01).

## BF-M02-02 — super_admin toàn quyền

- **Main flow:** super_admin truy cập bất kỳ policy → Handler Succeed nhờ `IsInRole(super_admin)` kể cả không có claim.
- **Source:** PermissionAuthorization.cs:25. **Final state:** full access. **Đúng.**

## BF-M02-03 — Gán quyền theo role lúc khởi động (seed reconcile)

- **Main flow:** `DbSeeder.SeedAsync` → tạo 100 permission (nếu thiếu) → với mỗi role trong map: thêm permission thiếu, XÓA permission thừa (super_admin nhận tất cả).
- **DB changes:** `role_permissions`. **Risk:** ghi đè chỉnh tay runtime (R3).

## BF-M02-04 — Chỉnh phân quyền runtime (tab Phân quyền)

- **Actor/Role:** super_admin (gate `roles:update`).
- **Main flow:** `/admin` → tab Phân quyền → chọn role → tick/bỏ permission → "Lưu phân quyền" → `SaveRolePermissionsAsync` reconcile add/remove + audit `update role_permissions`.
- **Alternate:** chọn super_admin → nút Lưu Disabled + cảnh báo.
- **Error/Risk:** KHÔNG đổi security stamp → user đang đăng nhập giữ claim cũ (BUG_M02_01). Restart sẽ reconcile về map code (R3).
- **Source:** Admin.razor:268-290.

## BF-M02-05 — Nạp quyền vào JWT (API)

- **Main flow:** `/api/auth/login` → `JwtTokenService.CreateAsync` nạp role + permission claim → API dùng `ApiAuth.Bearer("resource:action")` enforce.
- **Source:** JwtTokenService.cs, ApiContracts.cs. **Risk:** JWT không thu hồi trong 240' (giao thoa M01).

## BF-M02-06 — Data-scoping đối tác & cổng cá nhân hóa

- **Main flow:** `AgentScope.GetAsync` phân loại: staff (full theo permission), agentOnly (theo agentId), collaboratorOnly (theo collaboratorId), selfScoped parent/student (theo ownedCandidateId). UI/query lọc theo đó.
- **Risk:** enforcement scope ở tầng UI/query — API REST chưa lọc scope (R7). Kiểm IDOR ở M05/M09/M10/M11/M20.

## BF-M02-07 — Nhắn tin theo quy tắc role (MessagingPolicy)

- **Rule:** người nhận super_admin → ai cũng nhắn được; người nhận director → chỉ super_admin; người nhận agent/collaborator → chỉ super_admin/director/rec_mgr/recruiter; còn lại (nội bộ) → cho phép.
- **Source:** MessagingPolicy.cs. Chi tiết QA ở M14; ở đây kiểm bảng chân trị của quy tắc.

## Bảng phân quyền quan trọng — điểm cần kiểm

| Current State | Action | Allowed Role | Condition | Next State | DB Change | Notification | History |
|---|---|---|---|---|---|---|---|
| có quyền X (claim) | thao tác X | role có X | claim khớp / super_admin | thực hiện | tùy module | — | tùy module |
| bị thu quyền X runtime | thao tác X | — | **VẪN làm được tới re-login (SAI)** | thực hiện | — | — | — |
| không quyền X | gọi API `resource:action` | — | không claim | 403 | — | — | — |
| agent | đọc `/api/candidates/{id}` bất kỳ | agent có candidates:read | KHÔNG lọc scope | trả về mọi ứng viên (cần kiểm IDOR) | — | — | — |

**Kiểm tra vấn đề điển hình:** broken authorization (super_admin bypass — đúng); vertical escalation (role thấp gọi policy cao → chặn, cần test); horizontal escalation/IDOR (đổi ID ở API — nghi ngờ R7, kiểm module sau); UI vs API nhất quán quyền (cả hai dùng cùng bộ claim — nhất quán ở lớp permission, KHÁC ở lớp scope).
