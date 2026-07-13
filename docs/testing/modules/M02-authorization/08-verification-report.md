# M02 — Authorization · Verification Report (PHẦN G)

- **Người xác minh:** Claude Code (QA) — độc lập với Codex.
- **Thời điểm:** 2026-07-10 ~15:35.
- **Đầu vào đã đọc:** `01`→`07` của M02, `evidence-M02_02-runtime.md`, git diff (`ResourceEndpoints.cs`, `Admin.razor`), file mới `src/Polymind.Domain/Security/CandidateAccessScope.cs`, test mới `tests/Polymind.Tests/M02_CandidateAccessScopeTests.cs`.
- **Lệnh đã chạy:**
  - `dotnet test tests/Polymind.Tests` → **Passed 16/16, Failed 0** (độc lập, không tin số Codex báo).
  - `dotnet build src/Polymind.Web -o <temp>` → **0 warning / 0 error** (tránh khóa DLL của app đang chạy).

---

## BUG_M02_02 — REST `/api/candidates` bỏ qua data-scope → **Verified Fixed** (code-level)

### Điều đã kiểm
1. **Đọc source sửa:**
   - Mới: `CandidateAccessScope` (Domain) — `Apply` cho `Self` = `OwnerUserId == userId || ParentUserId == userId` ✅ (khớp `AgentScope`, phủ cả học viên lẫn phụ huynh); default fail-closed `Where(_ => false)` ✅.
   - `ResourceEndpoints`: `ResolveCandidateScopeAsync` — staff→`All`; agent→`ForAgent(Agent.UserId)`; CTV→`ForCollaborator(Collaborator.UserId)`; parent/student→`ForUser(userId)`; thiếu mapping/role lạ→`None` (fail-closed) ✅. Áp cho **cả list và `/{id}`**; ngoài scope ở detail → **404** (không xác nhận tồn tại id) ✅.
   - `CandidateFullAccessRoles` khớp đúng 8 role staff trong `DbSeeder`.
2. **Regression test:** `M02_CandidateAccessScopeTests` (5 test) phủ All / Agent (chỉ ứng viên của agent) / Collaborator / Self (student + parent) / None (rỗng) → **5/5 pass**. Test genuine, KHÔNG né bug.
3. **Không sửa test cũ để né bug:** 11 test cũ (SmokeTests, M01 config, M02 PermissionRegistry) vẫn nguyên + pass → tổng 16/16.
4. **Không hard-code / không làm yếu authorization:** giữ nguyên `RequireAuthorization(candidates:read)`; chỉ THÊM lớp data-scope. Không tắt validation, không đổi expected result.
5. **Compile:** Web build 0/0.

### Điều CHƯA kiểm (blocker runtime)
- **Live HTTP re-PoC chưa chạy:** app đang chạy trên `:5177` vẫn phục vụ **code CŨ** (dev host PID khóa DLL; Codex/QA build ra output riêng). Để xác nhận runtime rằng parent/student nay chỉ thấy hồ sơ của mình (hoặc 404), cần **restart app** rồi chạy lại PoC ở `evidence-M02_02-runtime.md`. **Không tự restart server của người dùng.**
- Regression test tự động ở tầng HTTP/JWT vẫn cần WebApplicationFactory + DB test (chưa dựng).

### Kết luận BUG_M02_02: **Verified Fixed (code)** — đúng nghiệp vụ, fail-closed, có regression unit + compile sạch.
- **Khuyến nghị đóng hoàn toàn:** restart app `:5177` → chạy lại PoC 5 bước trong "Verification Instructions for Claude" (07-fix-report) → kỳ vọng: staff total=18; student/parent chỉ 1 hồ sơ của mình; `/{id}` ngoài scope = 404.

---

## BUG_M02_01 — Thu quyền runtime không hiệu lực phiên đang mở → **Verified Fixed** (code-level)

### Điều đã kiểm
1. **Đọc source sửa** (`Admin.SaveRolePermissionsAsync`): sau khi tập quyền THỰC SỰ đổi (`toRemove>0 || toAdd>0`), lấy `GetUsersInRoleAsync(role)` → `UpdateSecurityStampAsync` từng user ✅. Guard "không đổi thì không churn stamp" → snackbar "Phân quyền không thay đổi" ✅. Lỗi update stamp **ném exception** (không nuốt, không báo thành công giả) ✅.
2. **Cơ chế đúng:** tái dùng security stamp — `IdentityRevalidatingAuthenticationStateProvider` (30') sẽ vô hiệu cookie cũ; đăng nhập lại nạp claim mới. Không làm yếu authorization, không đổi expected result của TC_M02_016.
3. **Không hồi quy:** suite 16/16, Web compile 0/0.

### Điều CHƯA kiểm (blocker runtime)
- **2-phiên + chờ ≤30' revalidation:** cần integration/thủ công 2 browser + DB test để xác nhận phiên nạn nhân bị đá. Chưa chạy được ở session này.
- **Giới hạn còn lại (Codex ghi đúng, chấp nhận):** JWT đã cấp vẫn stateless, sống tới hết hạn 240' — fix này chỉ đóng luồng cookie/revalidation (đúng phạm vi TC_M02_016).

### Kết luận BUG_M02_01: **Verified Fixed (code)** — logic đúng, guarded, errors surfaced.
- **Khuyến nghị đóng hoàn toàn:** chạy 5 bước manual trong "Verification Instructions" (07-fix-report) khi có 2 phiên + DB test.

---

## Tổng kết verification
- **Verified Fixed (code-level, mạnh):** BUG_M02_02, BUG_M02_01 — source đúng, có regression unit (16/16), compile sạch, không né test, không làm yếu bảo mật.
- **Chưa đóng 100%:** PoC runtime (HTTP/2-phiên) — blocker môi trường (app đang chạy code cũ + chưa có integration harness). KHÔNG tự restart server người dùng.
- **Regression introduced:** không phát hiện.
- **Cần người dùng/next session:** restart `:5177` để chạy lại PoC BUG_M02_02 (nhanh, đóng dứt điểm bug High); dựng harness integration cho phần còn lại.
