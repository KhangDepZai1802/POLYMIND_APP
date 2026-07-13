# Module Fix Report

## Summary

- **Module ID:** M02
- **Module Name:** Authorization, Roles & Permissions
- **Bugs Received:** 2
- **Bugs Fixed:** 2
- **Cannot Reproduce:** 0
- **Blocked:** 0 bug; runtime API PoC vẫn chờ integration harness
- **Needs Clarification:** 0

## BUG_M02_02

### Status

- Fixed

### Investigation

Đã đọc chuỗi JWT claim, policy `candidates:read`, `ResourceEndpoints`, `AgentScope`, mapping role seed và các query Candidate UI. Hai REST endpoint bắt đầu trực tiếp từ toàn bộ `db.Candidates`; vì vậy mọi tài khoản ngoài công ty có `candidates:read` đều vượt qua permission gate nhưng không qua data-scope.

### Root Cause

Authorization chỉ kiểm quyền theo resource/action. Data-scope của web nằm riêng trong `AgentScope` và từng component, trong khi REST API không resolve user JWT thành agent/CTV/hồ sơ sở hữu và không áp predicate tương ứng.

### Evidence

- Trước sửa: `ResourceEndpoints.MapCandidatesApi` dùng `db.Candidates.AsNoTracking()` cho cả list và detail, không có predicate theo principal.
- `JwtTokenService` phát `NameIdentifier`, role và `permission=candidates:read`, đủ dữ liệu để resolve scope ở API.
- Web UI áp các predicate `AgentId`, `CollaboratorId`, `OwnedCandidateId`, xác nhận phạm vi nghiệp vụ hiện hữu.
- Regression thuần sau sửa kiểm đủ All/Agent/Collaborator/Self/None: 5/5 pass.

### Files Inspected

- `src/Polymind.Web/Api/ResourceEndpoints.cs`
- `src/Polymind.Web/Api/ApiContracts.cs`
- `src/Polymind.Web/Api/AuthEndpoints.cs`
- `src/Polymind.Web/Api/JwtTokenService.cs`
- `src/Polymind.Web/Identity/AgentScope.cs`
- `src/Polymind.Web/Components/Pages/Candidates/Candidates.razor`
- `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor`
- `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`
- `src/Polymind.Infrastructure/Persistence/Constants/RoleNames.cs`
- `src/Polymind.Domain/Entities/Candidate.cs`, `Agent.cs`, `Collaborator.cs`

### Files Changed

- `src/Polymind.Domain/Security/CandidateAccessScope.cs`
- `src/Polymind.Web/Api/ResourceEndpoints.cs`
- `tests/Polymind.Tests/M02_CandidateAccessScopeTests.cs`

### Symbols Changed

- `CandidateAccessScope`, `CandidateAccessScopeKind`
- `ResourceEndpoints.MapCandidatesApi`
- `ResourceEndpoints.ResolveCandidateScopeAsync`

### Fix

REST API resolve principal theo đúng thứ tự role của `AgentScope`: staff có full scope; agent theo bản ghi `Agent.UserId`; CTV theo `Collaborator.UserId`; parent/student theo user id liên kết Candidate. Cả list và detail đều áp cùng `CandidateAccessScope` trước search/count/projection. Mapping thiếu hoặc role lạ fail-closed.

### Why This Fix Is Correct

Fix giữ nguyên permission contract và dữ liệu DTO cho hồ sơ hợp lệ, đồng thời đưa BF-M02-06/TC_M02_022 về cùng quy tắc data-scope đang dùng ở UI. Detail ngoài scope trả 404 nên không xác nhận sự tồn tại của id; list ngoài scope không lọt vào count hoặc phân trang.

### Alternatives Considered

- Bỏ `candidates:read` của parent/student/agent/CTV: phá contract và các client hợp lệ.
- Chỉ lọc list: vẫn để IDOR ở `/{id}`.
- Dùng `AgentScope` trực tiếp: service đó phụ thuộc `AuthenticationStateProvider` cookie/Blazor, không phù hợp JWT API.

### Impact

- **API:** response shape giữ nguyên; số bản ghi bị giới hạn đúng scope cho role ngoài công ty.
- **Database:** chỉ đọc, không migration.
- **UI:** không đổi.
- **Security:** chặn broken access control/PII ngoài scope; mapping thiếu fail-closed.
- **Backward compatibility:** staff không đổi; client ngoài công ty chỉ mất quyền truy cập dữ liệu vốn không thuộc phạm vi.
- **Data compatibility:** dùng các cột/mapping hiện có.

### Regression Risks

- Tài khoản agent/CTV chưa gắn `UserId` sẽ nhận danh sách rỗng thay vì dữ liệu toàn hệ thống; đây là hành vi an toàn mong muốn.
- Cần runtime PoC với JWT và PostgreSQL khi có integration harness.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| Suite trước sửa | Unit | Passed 11/11 | Xác nhận baseline |
| `M02_CandidateAccessScopeTests` | Unit regression | Passed 5/5 | BUG_M02_02 / TC_M02_022 |
| Toàn bộ `Polymind.Tests` | Unit regression | Passed 16/16 | Failed 0, Skipped 0 |
| Build `Polymind.Web` ra `C:\tmp\polymind-codex-build` | Compile | Passed | 0 warning, 0 error |
| Build output mặc định | Compile/environment | Blocked | Dev host PID 42884 khóa DLL; không phải lỗi code |

### Test Results

- **Passed:** 16
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** runtime HTTP/JWT PoC do chưa có WebApplicationFactory + DB test

### Verification Instructions for Claude

1. Lấy JWT của staff, agent, CTV, student và parent.
2. Gọi `GET /api/candidates?page=1&pageSize=100`: staff thấy toàn bộ; từng role hẹp chỉ thấy Candidate đúng mapping.
3. Với mỗi role hẹp, gọi `GET /api/candidates/{id_ngoài_scope}` và xác nhận 404; gọi id trong scope và xác nhận 200.
4. Kiểm `Total` và tìm kiếm không đếm bản ghi ngoài scope; chú ý `PassportNumber` không xuất hiện từ hồ sơ ngoài scope.
5. Xóa mapping `Agent.UserId`/`Collaborator.UserId` trên DB test và xác nhận fail-closed.

## BUG_M02_01

### Status

- Fixed

### Investigation

Đã đối chiếu `SaveRolePermissionsAsync`, factory tạo permission claim và authentication-state provider. Permission claim chỉ được tạo lúc đăng nhập; revalidation 30 phút chỉ so security stamp. Lưu role-permission không tác động user nên stamp cũ vẫn hợp lệ.

### Root Cause

`SaveRolePermissionsAsync` reconcile bảng `role_permissions` nhưng không phát tín hiệu invalidation tới các user đang mang role vừa sửa.

### Evidence

- `PermissionClaimsPrincipalFactory.GenerateClaimsAsync` chụp quyền vào cookie principal.
- `IdentityRevalidatingAuthenticationStateProvider` chỉ so security stamp mỗi 30 phút.
- Trước sửa, sau `db.SaveChangesAsync()` chỉ hiện snackbar, không gọi `UpdateSecurityStampAsync`.

### Files Inspected

- `src/Polymind.Web/Components/Pages/Admin/Admin.razor`
- `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`
- `src/Polymind.Web/Identity/PermissionClaimsPrincipalFactory.cs`
- `src/Polymind.Web/Authorization/PermissionAuthorization.cs`

### Files Changed

- `src/Polymind.Web/Components/Pages/Admin/Admin.razor`

### Symbols Changed

- `Admin.SaveRolePermissionsAsync`

### Fix

Sau khi tập quyền thực sự thay đổi và được lưu, lấy mọi user thuộc role rồi cập nhật security stamp tuần tự. Identity revalidation sẽ vô hiệu cookie cũ trong tối đa 30 phút; đăng nhập lại tạo principal với tập claim mới. Lỗi cập nhật stamp không bị nuốt.

### Why This Fix Is Correct

Security stamp là cơ chế revoke phiên đã có sẵn của M01. Chỉ invalidation role bị chỉnh, không ảnh hưởng role khác và không làm yếu authorization. Luồng đáp ứng TC_M02_016 mà không thay expected result.

### Alternatives Considered

- Chỉ hiển thị cảnh báo “đăng nhập lại”: để quyền đã thu hồi còn hiệu lực.
- Rebuild claim trong circuit hiện tại: không xử lý các thiết bị/phiên khác.
- Kiểm DB mọi authorization request: tăng chi phí và mở rộng phạm vi không cần thiết.

### Impact

- **API:** không đổi; JWT stateless đã cấp chưa bị thu hồi bởi fix này.
- **Database:** cập nhật `security_stamp`/`concurrency_stamp` của user thuộc role; không migration.
- **UI:** snackbar báo số tài khoản cần đăng nhập lại.
- **Security:** quyền cookie cũ không tồn tại quá chu kỳ revalidation.
- **Backward compatibility:** user role bị đổi quyền phải đăng nhập lại; role khác không ảnh hưởng.
- **Data compatibility:** không đổi schema.

### Regression Risks

- Role đông user tạo thêm các lệnh update tuần tự khi lưu quyền; thao tác quản trị hiếm, chấp nhận được.
- Permission đã commit trước khi update stamp; nếu Identity store lỗi, exception được phát ra để không báo thành công giả.
- JWT cũ vẫn hết hạn theo cấu hình 240 phút; đây là giới hạn đã biết ngoài TC_M02_016.

### Tests Run

| Test | Type | Result | Notes |
|---|---|---|---|
| Toàn bộ `Polymind.Tests` | Regression | Passed 16/16 | Không hồi quy registry/config/scope |
| Build `Polymind.Web` ra output riêng | Compile | Passed | 0 warning, 0 error |
| TC_M02_016 hai phiên | Integration/manual | Blocked | Cần DB test + hai phiên/browser |

### Test Results

- **Passed:** build + 16 unit tests
- **Failed:** 0
- **Skipped:** 0
- **Blocked:** xác minh revalidation runtime

### Verification Instructions for Claude

1. Đăng nhập user X thuộc role `recruiter`, giữ phiên mở.
2. Super admin bỏ một quyền của recruiter rồi lưu; xác nhận snackbar báo số tài khoản cần đăng nhập lại.
3. Xác nhận `asp_net_users.security_stamp` của mọi recruiter đổi, user role khác không đổi.
4. Sau revalidation (tối đa 30 phút), phiên X bị vô hiệu; đăng nhập lại và xác nhận quyền/claim mới.
5. Bấm lưu lần nữa khi không đổi tập quyền: stamp không đổi và snackbar “Phân quyền không thay đổi”.
