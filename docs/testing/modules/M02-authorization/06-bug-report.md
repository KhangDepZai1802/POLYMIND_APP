# M02 — Authorization, Roles & Permissions · Bug Report

---

## BUG_M02_01 — Thu hồi quyền runtime không có hiệu lực trên phiên đang đăng nhập

- **Bug ID:** BUG_M02_01
- **Module ID:** M02
- **Title:** Chỉnh phân quyền role qua `/admin` (tab Phân quyền) không làm mới claim/security-stamp của user đang đăng nhập → quyền BỊ THU HỒI vẫn dùng được tới khi user re-login (cookie tối đa 8h)
- **Severity:** Medium
- **Priority:** P2
- **Business Flow ID:** BF-M02-04
- **Test Case ID:** TC_M02_016
- **Automated Test ID:** — (cần integration)
- **Environment:** mọi môi trường
- **Role:** super_admin (chỉnh) · nạn nhân: user mang role bị chỉnh
- **Preconditions:** User X (role R) đang đăng nhập. Super admin bỏ một permission của role R.
- **Test Data:** VD role `recruiter`, bỏ `candidates:update`; user recruiter đang mở phiên.
- **Steps to Reproduce:**
  1. Recruiter X đăng nhập (claim gồm `candidates:update`).
  2. Super admin `/admin` → Phân quyền → chọn recruiter → bỏ tick `candidates:update` → Lưu.
  3. X (không đăng xuất) tiếp tục sửa ứng viên.
- **Expected Result:** Trong ≤30' (revalidation) hoặc ngay lần thao tác sau, X mất `candidates:update` → thao tác sửa bị chặn.
- **Actual Result:** X vẫn sửa được vì claim `candidates:update` đã nạp trong cookie lúc đăng nhập; `SaveRolePermissionsAsync` chỉ sửa bảng `role_permissions` + audit, KHÔNG đổi `security_stamp` của X. Revalidation 30' chỉ so security stamp (không đổi) → phiên hợp lệ, claim cũ giữ nguyên tới re-login.
- **UI Evidence:** `Admin.razor:268-290` `SaveRolePermissionsAsync` — không có `UpdateSecurityStampAsync`/không đụng user.
- **API Evidence:** claim nạp 1 lần tại `PermissionClaimsPrincipalFactory.GenerateClaimsAsync` (đăng nhập), không tái tạo khi revalidate.
- **Database Evidence:** `role_permissions` đổi; `asp_net_users.security_stamp` của user liên quan không đổi.
- **Suspected Source Area:** `Admin.SaveRolePermissionsAsync`, `IdentityRevalidatingAuthenticationStateProvider`, `PermissionClaimsPrincipalFactory`.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Components/Pages/Admin/Admin.razor` (SaveRolePermissionsAsync)
  - `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`
  - `src/Polymind.Web/Identity/PermissionClaimsPrincipalFactory.cs`
- **Dependencies:** cùng gốc cơ chế với BUG_M01_01 (security stamp không đổi khi thao tác quản trị bằng `UpdateAsync`/sửa bảng phụ). KHÔNG chặn QA module nghiệp vụ dùng phân quyền tĩnh mặc định.
- **Regression Risk:** nếu buộc đổi security stamp của TẤT CẢ user mang role đó khi lưu phân quyền → mọi user role đó bị đá đăng nhập lại (có thể phiền nếu chỉnh thường xuyên). Cần cân nhắc: đổi stamp cho user role đó, hoặc chấp nhận trễ theo revalidation.
- **Confidence Level:** Cao (source rõ ràng).
- **Status:** Verified Fixed (code-level) — Claude 2026-07-10 (source review + 16/16 unit + Web compile 0/0). Runtime HTTP PoC còn chờ restart app; xem `08-verification-report.md`.
- **Codex resolution:** Sau khi tập quyền thực sự thay đổi, cập nhật security stamp của mọi user thuộc role. Phiên cookie cũ bị vô hiệu ở lần revalidation kế tiếp (tối đa 30 phút) và lần đăng nhập sau nạp claim mới. Không cập nhật stamp khi người quản trị bấm lưu nhưng tập quyền không đổi.
- **Residual limitation:** JWT đã cấp vẫn stateless và sống tới thời điểm hết hạn; thay đổi này sửa đúng luồng cookie/revalidation của TC_M02_016, không mở rộng sang cơ chế thu hồi JWT.
- **Gợi ý hướng sửa (không bắt buộc):** sau khi lưu phân quyền, cập nhật security stamp cho các user mang role vừa chỉnh (`UpdateSecurityStampAsync`) để revalidation buộc re-login (nạp lại claim mới); hoặc tài liệu hóa "quyền có hiệu lực sau khi đăng nhập lại".

---

## BUG_M02_02 — REST `/api/candidates` bỏ qua AgentScope → lộ PII (số hộ chiếu) mọi ứng viên cho tài khoản phạm vi hẹp

> Ghi chú xuất xứ: ban đầu ghi dạng "R7 / TC_M02_022" (nghi ngờ, chờ runtime). Đã **nâng thành bug xác nhận từ source** khi tiếp quản QA (đọc đủ chuỗi bằng chứng bên dưới). Runtime PoC + regression test hoãn tới harness M05/M20 (chưa dựng được ở session này).

- **Bug ID:** BUG_M02_02
- **Module ID:** M02 (bề mặt REST API — liên đới M05 Candidate, M20 Security)
- **Title:** `GET /api/candidates` và `GET /api/candidates/{id}` chỉ gate permission `candidates:read`, KHÔNG lọc theo `AgentScope` → tài khoản `parent`/`student`/`agent`/`collaborator` (đều có `candidates:read`) đọc được **toàn bộ** ứng viên gồm **số hộ chiếu, SĐT, tỉnh, giới tính**, trong khi web UI giới hạn đúng phạm vi.
- **Severity:** High (PII số hộ chiếu của mọi ứng viên bị lộ cho cả tài khoản cổng ngoài — phụ huynh/học viên).
- **Priority:** P1
- **Business Flow ID:** BF-M02-06 (data-scope enforcement)
- **Test Case ID:** TC_M02_022
- **Automated Test ID:** — (cần integration harness: JWT + WebApplicationFactory)
- **Environment:** mọi môi trường (REST API `/api/*` được map trong `Program.cs`, không có filter role toàn cục)
- **Role:** kẻ tấn công = bất kỳ tài khoản active có `candidates:read` (đặc biệt nguy hiểm với `parent`/`student` — người dùng ngoài công ty)
- **Preconditions:** có 1 tài khoản active bất kỳ giữ `candidates:read` (VD `student`/`parent` demo, `agent@polymind.local`, hoặc `ctv-*`).
- **Test Data:** đăng nhập lấy JWT; danh sách ứng viên có ≥2 hồ sơ không thuộc phạm vi tài khoản đó.
- **Steps to Reproduce (source-confirmed; PoC runtime khi có harness):**
  1. `POST /api/auth/login` bằng tài khoản `student`/`parent`/`agent`/`collaborator` (AllowAnonymous, chấp mọi role active) → nhận JWT chứa claim `permission=candidates:read`.
  2. `GET /api/candidates?page=1&pageSize=100` kèm `Authorization: Bearer <jwt>`.
  3. (Hoặc) `GET /api/candidates/{id_bất_kỳ}`.
- **Expected Result:** Chỉ trả ứng viên trong phạm vi tài khoản (đại lý→subtree; CTV→ứng viên mình giới thiệu; parent/student→đúng hồ sơ của mình), hoặc 403 nếu role không được dùng API dữ liệu ứng viên. Không lộ hồ sơ/PII ngoài phạm vi.
- **Actual Result:** Trả **mọi** ứng viên trong DB (chỉ phân trang + tìm kiếm tùy chọn), gồm `FullName, Phone, Province, Gender, PassportNumber`. `GET /{id}` trả bất kỳ hồ sơ nào theo id. Không có bước lọc scope.
- **UI Evidence:** — (đây là lỗ hổng API, web UI giới hạn đúng).
- **API Evidence:**
  - `ResourceEndpoints.cs:13-38` (`MapCandidatesApi` GET list): `db.Candidates.AsNoTracking().AsQueryable()` + chỉ optional `search`, **không** lọc theo user/scope; `.RequireAuthorization(ApiAuth.Bearer("candidates:read"))`.
  - `ResourceEndpoints.cs:40-50` (GET `/{id}`): `FirstOrDefaultAsync(x => x.Id == id)` — lấy bất kỳ id.
  - `ApiContracts.cs:43-45` `CandidateDto` chứa `PassportNumber`.
  - `AuthEndpoints.cs:15-47` `POST /api/auth/login` `AllowAnonymous` — cấp JWT cho mọi tài khoản active.
  - `DbSeeder.cs:104-111` parent/student = `Read("candidates", ...)`; `:93-100` agent/collaborator có `candidates:read`.
  - `AgentScope` là service scoped của Blazor (dùng cookie `AuthenticationStateProvider`) — **không** được gọi ở bất kỳ endpoint `/api/*` nào.
- **Database Evidence:** không cần thay đổi DB — chỉ đọc.
- **Log Evidence:** —
- **Suspected Source Area:** `src/Polymind.Web/Api/ResourceEndpoints.cs` (thiếu lọc scope) + thiết kế API chưa có lớp data-scope tương đương `AgentScope` cho JWT.
- **Required Files for Codex to Inspect:**
  - `src/Polymind.Web/Api/ResourceEndpoints.cs`
  - `src/Polymind.Web/Identity/AgentScope.cs` (logic scope để tái dụng cho API)
  - `src/Polymind.Web/Api/ApiContracts.cs` (`ApiAuth`, `CandidateDto`)
  - `src/Polymind.Infrastructure/Persistence/DbSeeder.cs` (role nào có `candidates:read`)
- **Dependencies:** liên đới M05 (Candidate data-scope) và M20 (Security). KHÔNG chặn QA module khác dùng permission tĩnh.
- **Regression Risk:** Trung bình — thêm lọc scope cho API phải tương thích tài khoản staff (thấy tất cả) và tài khoản phạm vi hẹp (thấy đúng phần mình); cần map `userId → agent/collaborator/owned candidate` ở tầng API (không có `AuthenticationStateProvider` như Blazor). Cân nhắc: hoặc bỏ `candidates:read` khỏi parent/student/agent/collaborator ở API (siết policy), hoặc thêm lớp scope.
- **Confidence Level:** **RUNTIME CONFIRMED (Cao nhất).** PoC chạy thật trên `:5177` — xem [`evidence-M02_02-runtime.md`](evidence-M02_02-runtime.md): tài khoản `student` và `parent` demo gọi `GET /api/candidates` trả **HTTP 200, total=18 (toàn bộ ứng viên)** kèm `passportNumber`, trong khi web UI chỉ cho thấy 1 hồ sơ. Chuỗi source đã khớp thực tế. (Regression test tự động vẫn hoãn tới harness.)
- **Status:** Verified Fixed (code-level) — Claude 2026-07-10 (source review + 16/16 unit + Web compile 0/0). Runtime HTTP PoC còn chờ restart app; xem `08-verification-report.md`.
- **Codex resolution:** Giữ API tương thích cho các role đang có `candidates:read` và áp data-scope giống web UI: staff thấy tất cả; agent theo `AgentId`; CTV theo `CollaboratorId`; parent/student theo `OwnerUserId`/`ParentUserId`. Tài khoản phạm vi hẹp chưa được gắn mapping hoặc role không được nhận diện sẽ fail-closed (danh sách rỗng/chi tiết 404).

---

## Ghi chú không phải bug (để module sau kiểm)

- **`/api/job-orders`:** chỉ gate `job_orders:read` mà chỉ role nội bộ (staff) có quyền này (agent/collaborator/parent/student KHÔNG có) → staff xem mọi đơn hàng là đúng nghiệp vụ; không lọc scope ở đây chấp nhận được. Vẫn nên xác nhận lại ở M06/M20.
- **U1 (accountant approve) — Resolved 2026-07-10:** user xác nhận kế toán được approve khoản thu, khoản chi, hoa hồng và khoản vay. `DbSeeder.RolePermissionMap` hiện đã đúng; không phải defect, không cần Codex sửa code.

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |
|---:|---|---|---|---|---|---|---|---|
| 1 | BUG_M02_02 | High | TC_M02_022 | BF-M02-06 | ResourceEndpoints thiếu lọc AgentScope | ResourceEndpoints.cs, AgentScope.cs, ApiContracts.cs, DbSeeder.cs | 5 unit regression scope + runtime PoC khi có harness | Fixed — Waiting for Claude Verification |
| 2 | BUG_M02_01 | Medium | TC_M02_016 | BF-M02-04 | SaveRolePermissions + stamp | Admin.razor, IdentityRevalidating..., PermissionClaimsPrincipalFactory | TC_M02_014, 016, 007, 008 | Fixed — Waiting for Claude Verification |
