# 📓 WORKLOG — POLYMIND OLMS (file phối hợp giữa các session AI)

> **File này là nguồn sự thật chung cho Claude và Codex.** Đọc TRƯỚC khi làm, cập nhật SAU khi làm.

---

## ⚠️ QUY TẮC BẮT BUỘC (đọc mỗi lần)

1. **TRƯỚC khi bắt đầu session:** đọc hết file này — phần `TRẠNG THÁI HIỆN TẠI` + `VIỆC TIẾP THEO` + entry mới nhất trong `NHẬT KÝ`.
2. **Chọn việc:** làm theo `VIỆC TIẾP THEO`. Nếu muốn làm khác, ghi rõ lý do vào nhật ký.
3. **SAU khi làm xong (hoặc trước khi hết session):**
   - Cập nhật `TRẠNG THÁI HIỆN TẠI` (1-2 dòng).
   - Cập nhật `VIỆC TIẾP THEO` (việc kế cho người sau — càng cụ thể càng tốt).
   - **Thêm 1 entry mới** vào đầu `NHẬT KÝ SESSION` (mới nhất ở trên).
   - Nếu có blocker/lỗi chưa fix → ghi vào `BLOCKERS`.
4. **Luôn để app build được** (`dotnet build Polymind.slnx`) trước khi kết thúc. Nếu để dở, ghi rõ "đang dở, chưa build".
5. Chi tiết backlog đầy đủ + bẫy kỹ thuật + quy ước code: xem **docs/05-handoff-codex.md** (đọc 1 lần để nắm nền).

**Format 1 entry nhật ký:**
```
### [YYYY-MM-DD] Session N — <Claude|Codex>
- **Làm được:** ...
- **File thay đổi chính:** ...
- **Đã test:** ... (build? chạy? smoke test gì?)
- **Lưu ý/cảnh báo cho người sau:** ...
```

---

## 🎯 TRẠNG THÁI HIỆN TẠI

- **Bản demo MVP chạy được**, đã smoke-test toàn bộ trang HTTP 200. App: `http://localhost:5177`, login `admin@polymind.local / Admin@123`.
- Build sạch (`dotnet build Polymind.slnx` = 0 error). Docker (Postgres/Redis/MinIO) cần `docker compose up -d`.
- Đã xong: nền tảng + auth + Dashboard + Lead CRM + Ứng viên(timeline 17 bước) + Đơn hàng(read-only) + demo data.
- **RBAC thực thi đã có code nền:** seed 8 user mẫu/role permissions, permission claims khi login, policy động dạng `resource:action`, ẩn menu theo quyền và chặn các nút ghi dữ liệu chính.

## ⏭️ VIỆC TIẾP THEO (baton — làm cái này trước)

**P1.1 smoke-test RBAC rồi chuyển P1.2:**
1. Chạy `docker compose up -d` + `dotnet run --project src/Polymind.Web`, đăng nhập thử các tài khoản mẫu (mật khẩu đều `Admin@123`): `recruiter@polymind.local`, `accountant@polymind.local`, `visa.staff@polymind.local`, `agent@polymind.local`.
2. Kiểm tra menu/route/nút theo quyền: recruiter thấy Lead/Ứng viên và tạo/chuyển lead được; accountant thấy Tài chính/Đại lý/Báo cáo nhưng không thấy nút tạo lead; agent chỉ có chế độ đọc.
3. Sau smoke-test → bắt đầu **P1.2 Ứng viên CRUD đầy đủ**: form tạo/sửa ứng viên, thông tin cá nhân đầy đủ, gắn ứng viên vào đơn hàng (`CandidateJobOrder`).

## 🚧 BLOCKERS / NỢ KỸ THUẬT

- (chưa có blocker)
- Nợ wiring: MinIO, Hangfire, Redis, QuestPDF, ClosedXML — gắn khi tới phần dùng.
- Module placeholder chờ làm thật: /finance, /agents, /visa, /reports.

---

## 📜 NHẬT KÝ SESSION (mới nhất ở trên)

### [2026-06-24] Session 5 — Codex
- **Làm được:** Khởi tạo Git repository local hợp lệ, thêm remote `https://github.com/KhangDepZai1802/POLYMIND_APP.git`, commit source dự án và push lên GitHub branch `main`.
- **File thay đổi chính:** `.gitignore` thêm `.claude/` để không đẩy cấu hình local; `WORKLOG.md` cập nhật nhật ký push GitHub.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 0 warning sau khi dừng web app đang giữ lock DLL. `git push -u origin main` thành công.
- **Lưu ý/cảnh báo cho người sau:** Web app process chạy nền trước đó đã được dừng để build/push. Remote GitHub hiện track `origin/main`; `.claude/settings.local.json` không được commit.

### [2026-06-24] Session 4 — Codex
- **Làm được:** Chạy lại web app phục vụ kiểm tra thủ công. Docker services đang chạy (`polymind-postgres`, `polymind-redis`, `polymind-minio`), web app listen tại `http://localhost:5177`.
- **File thay đổi chính:** Không đổi source code; chỉ cập nhật `WORKLOG.md`.
- **Đã test:** `GET http://localhost:5177/login` trả HTTP 200 có nội dung POLYMIND; `GET /` khi chưa đăng nhập trả 302 về `/login?ReturnUrl=%2F`.
- **Lưu ý/cảnh báo cho người sau:** Log web app đang ghi tại `C:\tmp\polymind-web.out.log` và `C:\tmp\polymind-web.err.log`. Chưa smoke-test RBAC bằng từng tài khoản mẫu.

### [2026-06-24] Session 3 — Codex
- **Làm được:** Hoàn thiện code nền P1.1 RBAC thực thi. `DbSeeder` seed 8 user mẫu và map permissions cho 7 role còn lại theo docs/03 mục 8. Login nạp permission vào claim `permission` qua custom `IUserClaimsPrincipalFactory`. Thêm `PermissionPolicyProvider` + `PermissionAuthorizationHandler` để dùng policy động dạng `"leads:create"`. Gắn policy đọc vào route, ẩn menu theo quyền, thêm trang `/access-denied`, và chặn các action chính: tạo lead, đổi trạng thái lead, convert lead→ứng viên, chuyển bước ứng viên.
- **File thay đổi chính:** `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`, `src/Polymind.Web/Authorization/PermissionAuthorization.cs`, `src/Polymind.Web/Identity/PermissionClaimsPrincipalFactory.cs`, `src/Polymind.Web/Program.cs`, `Components/Layout/NavMenu.razor`, `Components/RedirectToLogin.razor`, `Components/Pages/AccessDenied.razor`, các page Lead/Candidate/placeholder có `[Authorize(Policy=...)]`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 1 warning cũ `BL0008` ở `Login.razor`. Lần build đầu bị sandbox chặn restore NuGet; chạy lại với network/escalated thì build xanh.
- **Lưu ý/cảnh báo cho người sau:** Cần đăng nhập lại để cookie nhận permission claims mới. `super_admin` vẫn được handler cho qua bằng role để tránh kẹt cookie cũ. Chưa smoke-test runtime với DB/app đang chạy trong session này.

### [2026-06-24] Session 2 — Claude
- **Làm được:** Bản demo MVP hoàn chỉnh để trình đối tác. MudBlazor shell (Topbar/Sidebar), Login SSR (cookie + SignInManager), AuthenticationStateProvider, bảo vệ route. Dashboard KPI. Lead CRM đầy đủ (list filter/search/paginate + dialog thêm + chi tiết: timeline, đổi trạng thái, convert→ứng viên). Ứng viên (list + timeline 17 bước + nút chuyển bước). Đơn hàng (cards). Placeholder 4 module. DemoDataSeeder (40 lead/12 ứng viên/5 đơn hàng/3 đại lý/12 payment).
- **File thay đổi chính:** Program.cs, Components/* (Account/Login, Layout/MainLayout+NavMenu+EmptyLayout, Pages/Home+Leads+Candidates+JobOrders+placeholders, Shared/ComingSoon, RedirectToLogin), Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs, Web/Display/Labels.cs, Infrastructure/Persistence/DemoDataSeeder.cs, DI đổi sang AddDbContextFactory.
- **Đã test:** build 0 error; chạy app; smoke test login→302 + 6 trang (/, /leads, /candidates, /job-orders, /leads/{id}, /candidates/{id}) đều HTTP 200 đúng nội dung.
- **Lưu ý/cảnh báo cho người sau:** (1) Bẫy DateTimeOffset phải UTC offset 0 — đừng dùng `.UtcNow.Date`. (2) Trang set cookie để SSR; trang app dùng InteractiveServer+[Authorize]+IDbContextFactory. (3) RBAC mới có khung, chưa thực thi → đây là việc tiếp theo. (4) Re-seed demo: TRUNCATE bảng nghiệp vụ rồi restart.

### [2026-06-24] Session 1 — Claude
- **Làm được:** Khởi tạo dự án C#/.NET 10. Solution 4 project (Domain/Application/Infrastructure/Web Blazor). 20 entity + enums. ApplicationDbContext + Identity + migration InitialCreate (28 bảng). DbSeeder (8 roles, 80 permissions, super_admin). docker-compose (Postgres/Redis/MinIO).
- **File thay đổi chính:** toàn bộ src/ khởi tạo, docker-compose.yml, .gitignore.
- **Đã test:** build 0 error; áp migration thành công; seed chạy (8 roles/80 perms/admin); app khởi động OK.
- **Lưu ý/cảnh báo cho người sau:** Stack đã đổi từ Node (docs cũ) sang .NET. Solution là .slnx. Nghiệp vụ/DB/workflow trong docs/01-03 vẫn dùng nguyên.
