# M02 — Authorization, Roles & Permissions · Phân tích

## 1. Module Overview

- **Module ID:** M02
- **Module name:** Authorization, Roles & Permissions (RBAC)
- **Business purpose:** Kiểm soát ai được làm gì. Mô hình RBAC dạng `resource:action` (permission claim), 12 vai trò, super_admin toàn quyền; thêm data-scoping cho đối tác (đại lý/CTV) và cổng cá nhân hóa (phụ huynh/học viên) qua `AgentScope`; quy tắc nhắn tin `MessagingPolicy`.
- **Actor:** mọi người dùng đã xác thực (M01).
- **Role:** 12 role (`RoleNames`): super_admin, director, recruitment_manager, recruiter, consultant, document_staff, visa_staff, accountant, agent, collaborator, parent, student.
- **Dependencies:** M01 (đăng nhập cấp claim). Consumed bởi MỌI module nghiệp vụ (`[Authorize(Policy="resource:action")]`, `<AuthorizeView>`).
- **Entry point:** gắn claim tại đăng nhập (`PermissionClaimsPrincipalFactory` cookie / `JwtTokenService` JWT); enforcement tại `PermissionAuthorizationHandler`.
- **Exit point:** Succeed → truy cập; Fail → `/access-denied` (cookie) hoặc 401/403 (API).

## 2. Source Code Map

| # | File | Symbol | Mục đích |
|---|---|---|---|
| 1 | [PermissionRegistry.cs](../../../../src/Polymind.Infrastructure/Persistence/PermissionRegistry.cs) | `PermissionRegistry` | Sinh 20 resource × 5 action = **100 permission** `resource:action` |
| 2 | [RoleNames.cs](../../../../src/Polymind.Infrastructure/Persistence/Constants/RoleNames.cs) | `RoleNames` | 12 vai trò + nhãn hiển thị |
| 3 | [DbSeeder.cs](../../../../src/Polymind.Infrastructure/Persistence/DbSeeder.cs) | `RolePermissionMap`, `AssignRolePermissionsAsync` | Gán permission cho 11 role (super_admin nhận tất cả); reconcile add/remove mỗi lần khởi động |
| 4 | [PermissionAuthorization.cs](../../../../src/Polymind.Web/Authorization/PermissionAuthorization.cs) | `PermissionRequirement`, `PermissionAuthorizationHandler`, `PermissionPolicyProvider` | Enforce: có claim `permission==policy` **hoặc** IsInRole(super_admin) → Succeed; policy động cho tên `resource:action` hợp lệ |
| 5 | [PermissionClaimsPrincipalFactory.cs](../../../../src/Polymind.Web/Identity/PermissionClaimsPrincipalFactory.cs) | — | Nạp permission claim vào cookie principal khi đăng nhập (từ RolePermission theo role của user) |
| 6 | [JwtTokenService.cs](../../../../src/Polymind.Web/Api/JwtTokenService.cs) | `CreateAsync` | Nạp cùng bộ permission claim vào JWT → dùng lại policy `resource:action` |
| 7 | [AgentScope.cs](../../../../src/Polymind.Web/Identity/AgentScope.cs) | `AgentScope`, `AgentScopeInfo` | Data-scoping: đại lý thấy CTV/ứng viên của mình; CTV thấy ứng viên mình giới thiệu; parent/student thấy đúng 1 hồ sơ (`OwnerUserId`/`ParentUserId`) |
| 8 | [MessagingPolicy.cs](../../../../src/Polymind.Web/Identity/MessagingPolicy.cs) | `CanMessage`, `PrimaryRoleLabel` | Ai được nhắn cho ai theo role người nhận |
| 9 | [Admin.razor](../../../../src/Polymind.Web/Components/Pages/Admin/Admin.razor) | tab "Phân quyền", `SaveRolePermissionsAsync` | UI chỉnh RolePermission runtime (gate `roles:update`), super_admin bị khóa chỉnh |
| 10 | [ApiContracts.cs](../../../../src/Polymind.Web/Api/ApiContracts.cs) | `ApiAuth.Bearer` | Ép scheme JWT cho API (trả 401 thay vì redirect cookie) + policy động |

## 3. Ma trận vai trò → quyền (từ DbSeeder.RolePermissionMap)

| Resource | super_admin | director | rec_mgr | recruiter | consultant | doc_staff | visa_staff | accountant | agent | collaborator | parent | student |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| dashboard | ALL | R | R | R | R | R | R | R | — | — | — | — |
| leads | ALL | R | CRUD | CRU | CRU | RUD | — | — | — | — | — | — |
| candidates | ALL | R | CRU | CRU | CRU | RUD | RU | R | R | R | R | R |
| job_orders | ALL | R | R | R | R | R | R | R | — | — | — | — |
| payments | ALL | R | — | — | — | — | — | ALL | — | — | R | R |
| expenses | ALL | R | — | — | — | — | — | ALL | — | — | — | — |
| receipts | ALL | R | — | — | — | — | — | ALL | — | — | — | — |
| agents | ALL | R | R | R | R | — | — | R | R | R | — | — |
| collaborators | ALL | R | CRU | R | R | R | R | R | U | — | — | — |
| commissions | ALL | R+approve | — | — | — | — | — | ALL | R | R | — | — |
| loans | ALL | R | CRU | CRU | CRU | R | R | ALL | R | — | R | R |
| visas | ALL | R | — | — | — | — | ALL | — | — | — | — | — |
| flights | ALL | R | — | — | — | — | ALL | — | — | — | — | — |
| reports | ALL | C+R | R | — | — | — | — | R | — | — | — | — |
| users | ALL | R | — | — | — | — | — | — | — | — | — | — |
| roles | ALL | R | — | — | — | — | — | — | — | — | — | — |
| notifications | ALL | R | R | R | R | R | R | R | R | R | R | R |
| messages | ALL | R+C | R+C | R+C | R+C | R+C | R+C | R+C | R+C | R+C | R+C | R+C |
| audit | ALL | R | — | — | — | — | — | — | — | — | — | — |
| training | ALL | R | CRUD | — | CRUD | — | — | — | R | R | R | R |

(ALL=create/read/update/delete/approve; CRUD=create/read/update/delete; CRU=create/read/update; RUD=read/update/delete; R=read; C=create; U=update.) Nguồn: `DbSeeder.cs:37-112`.

## 4. Cơ chế enforcement

- **Policy động:** `PermissionPolicyProvider.GetPolicyAsync("resource:action")` → nếu tên hợp lệ (resource∈Resources & action∈Actions) → tạo policy `RequireAuthenticatedUser + PermissionRequirement`.
- **Handler:** `PermissionAuthorizationHandler` Succeed nếu user có claim `permission == policy` (OrdinalIgnoreCase) **hoặc** `IsInRole(super_admin)`.
- **Cookie:** claim nạp 1 lần lúc đăng nhập (`PermissionClaimsPrincipalFactory`).
- **JWT:** claim nạp lúc cấp token (`JwtTokenService`), `MapInboundClaims=false` giữ nguyên type "permission".
- **Data-scope (không phải permission):** UI/query tự lọc theo `AgentScope` (agentId/collaboratorId/ownedCandidateId). Đây là lớp lọc DỮ LIỆU, tách khỏi lớp permission — QA IDOR ở từng module nghiệp vụ (M04/M05/M09…).

## 5. Database Impact

- **Bảng:** `permissions` (Name, Resource, Action), `role_permissions` (RoleId, PermissionId), `asp_net_roles`, `asp_net_user_roles`.
- **Seed reconcile:** mỗi khởi động, `AssignRolePermissionsAsync` thêm permission thiếu + XÓA permission thừa so với map → chỉnh tay runtime (tab Phân quyền) sẽ bị **ghi đè khi restart** về đúng map code (trừ super_admin luôn full). → rủi ro/nhầm lẫn vận hành (xem Risk R3).

## 6. Roles & Permissions (thao tác quản trị RBAC)

| Action | Role | UI | API | Điều kiện | Source |
|---|---|---|---|---|---|
| Xem trang `/admin` | có `users:read` (director, super_admin) | `[Authorize(users:read)]` | — | — | Admin.razor:2 |
| Chỉnh phân quyền role | có `roles:update` (chỉ super_admin) | `<AuthorizeView roles:update>` | — | không cho chỉnh super_admin | Admin.razor:38, 271 |
| Xem nhật ký | có `audit:read` (director, super_admin) | `<AuthorizeView audit:read>` | — | — | Admin.razor:128 |

## 7. Risk Analysis (đã đối chiếu source)

1. **[XÁC NHẬN — Medium] Đổi phân quyền runtime KHÔNG làm mới phiên đang đăng nhập.** `SaveRolePermissionsAsync` sửa RolePermission + audit nhưng KHÔNG đổi security stamp của user liên quan. Claim permission đã nạp trong cookie tại đăng nhập; revalidation 30' chỉ so stamp (không đổi) → quyền BỊ THU HỒI vẫn còn hiệu lực tới khi user re-login (cookie tối đa 8h). → **BUG_M02_01**.
2. **[Đúng thiết kế] super_admin bypass** qua `IsInRole(super_admin)` — kể cả khi RolePermission trống, super_admin vẫn full. An toàn (khớp UI khóa chỉnh super_admin + seed reconcile).
3. **[Theo dõi — Low] Seed ghi đè chỉnh tay:** tab Phân quyền cho chỉnh runtime nhưng restart app sẽ reconcile về map code (thêm thiếu/xóa thừa). Người vận hành có thể tưởng đã đổi vĩnh viễn. → cần tài liệu/ghi chú UI. Đánh giá: rủi ro vận hành, chưa đủ là bug chức năng.
4. **[ĐÃ CHỐT — đúng nghiệp vụ] accountant có `approve`** trên payments/expenses/receipts/commissions/loans (AllActions). User xác nhận ngày 2026-07-10: kế toán được duyệt khoản thu, khoản chi, hoa hồng và khoản vay. Role map hiện tại đã đúng, không phải bug và không cần sửa code.
5. **[Cần làm rõ — U2] parent/student có `payments:read` + `loans:read`** nhưng chỉ được xem hồ sơ của mình — enforcement phạm vi nằm ở AgentScope + query từng trang, KHÔNG ở permission. Phải QA IDOR ở M05/M10/M11 (gọi API/đổi ID). Permission chỉ mở "read", không giới hạn "read của ai".
6. **[Theo dõi] Data-scope stale:** `AgentScope._cached` scoped theo circuit Blazor (sống dài) → nếu gán ứng viên mới cho đại lý giữa phiên, scope cũ. Nhẹ. Parent/student `FirstOrDefault` chỉ 1 hồ sơ → phụ huynh nhiều con chỉ thấy 1 (giới hạn thiết kế).
7. **[Theo dõi] REST API IDOR:** `/api/candidates/{id}`, `/api/job-orders` chỉ gate `candidates:read`/`job_orders:read`, KHÔNG lọc theo scope đại lý → tài khoản agent có `candidates:read` gọi API có thể đọc MỌI ứng viên (không giới hạn của mình). Cần QA ở M05/M20 (đây là data-scope, không phải permission thuần). Ghi nhận để module sau kiểm.

## 8. Unknowns / Cần làm rõ

- **U1 — Resolved 2026-07-10:** accountant được quyền `approve` thu/chi/hoa hồng/vay; source hiện tại đúng nghiệp vụ.
- **U2:** phạm vi "read" của parent/student với payments/loans có được ràng buộc chặt ở tầng API không (không chỉ UI)? — kiểm ở M05/M10/M11.
- **U3:** mong muốn khi thu hồi quyền: có buộc user đăng xuất ngay không? (liên quan BUG_M02_01, giống U1 của M01).
