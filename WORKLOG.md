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
- Build sạch (`dotnet build Polymind.slnx` = 0 error, 1 warning cũ `BL0008` ở Login.razor — vô hại). Docker (Postgres/Redis/MinIO) cần `docker compose up -d`.
- Đã xong: nền tảng + auth + Dashboard + Lead CRM + **Ứng viên CRUD** + **Đơn hàng CRUD** + **Tài chính (Thu/Chi)** + **Đại lý & Hoa hồng** + **Visa & Xuất cảnh (Visa + Vé máy bay CRUD)** + demo data.
- **RBAC thực thi đã có code nền:** seed 8 user mẫu/role permissions, permission claims khi login, policy động dạng `resource:action`, ẩn menu theo quyền và chặn các nút ghi dữ liệu chính.
- **Đã sửa lỗi nút/dialog không bấm được:** chuyển sang **interactivity toàn cục** (render mode đặt ở `Routes`/`HeadOutlet` trong `App.razor`, Login dùng `[ExcludeFromInteractiveRouting]` để giữ SSR). Trước đó layout + MudBlazor providers bị render tĩnh nên dialog/snackbar/drawer chết.

## ⏭️ VIỆC TIẾP THEO (baton — làm cái này trước)

**P1.7 Module Báo cáo (`/reports` đang là placeholder) — đây là placeholder CUỐI CÙNG:**
1. Trang `Pages/Reports/Reports.razor` (đặt trong folder): tổng hợp số liệu từ các bảng đã có. Gợi ý các thẻ/biểu đồ: tổng lead theo nguồn/trạng thái, tỉ lệ chuyển đổi lead→ứng viên, số ứng viên theo bước workflow, doanh thu (Payments) vs chi phí (Expenses) theo tháng, hoa hồng theo đại lý. Gate `reports:read`.
2. Có thể dùng MudChart (MudBlazor) cho biểu đồ cột/tròn; dữ liệu group bằng EF `GroupBy`. Tham khảo Dashboard `Home.razor` đã có sẵn vài KPI.
3. **LƯU Ý khi tạo trang mới:** KHÔNG thêm `@rendermode`; **đặt page trong folder + đừng đặt tên class trùng entity** (bài học Visa: page `Visa` trùng entity `Visa` → phải đổi thành `Visas`).
4. **Nợ kỹ thuật còn treo sau P1.7:** (a) smoke-test RBAC bằng từng tài khoản mẫu (`recruiter/accountant/visa.staff/agent` @ `Admin@123`); (b) CandidateDocument upload (cần wiring MinIO); (c) UI tạo AgentCommission/Config (demo chưa seed commission — mới hiển thị); (d) demo chưa seed Visa/Flight nên 2 bảng này trống tới khi nhập tay.

## 🚧 BLOCKERS / NỢ KỸ THUẬT

- (chưa có blocker)
- Nợ wiring: MinIO, Hangfire, Redis, QuestPDF, ClosedXML — gắn khi tới phần dùng.
- Module placeholder chờ làm thật: /finance, /agents, /visa, /reports.

---

## 📜 NHẬT KÝ SESSION (mới nhất ở trên)

### [2026-06-24] Session 7 — Claude
- **Làm được (P1.6):** Module **Visa & Xuất cảnh**. Trang `Pages/Visas/Visas.razor` (`/visa`) 2 tab: **Hồ sơ Visa** + **Vé máy bay**, mỗi tab DataGrid + nút Thêm + nút Sửa. `VisaDialog.razor` (chọn cặp Ứng viên–Đơn hàng qua `CandidateJobOrder` để lấy CandidateId/JobOrderId/Country; loại/trạng thái `VisaStatus`/ngày nộp-phỏng vấn-kết quả/lý do từ chối), gate `visas:*`. `FlightDialog.razor` (hãng/mã vé/ngày-giờ bay/sân bay; `TimeOnly?`↔`TimeSpan?` cho MudTimePicker), gate `flights:*`. Thêm nhãn VN + `ColorOf(VisaStatus)`.
- **⚠️ BẪY đã gặp & xử lý:** ban đầu đặt page `Pages/Visa/Visa.razor` → class page `Visa` **trùng tên entity `Visa`** trong namespace `Pages.Visa` khiến mọi tham chiếu `Visa` resolve về class page (CS1503/CS1061 hàng loạt). Sửa: đổi folder→`Visas`, page→`Visas.razor` (class `Visas`), route `/visa` giữ nguyên. → **Quy tắc mới: đừng đặt tên component trùng tên entity.**
- **File thay đổi chính (P1.6):** `Components/Pages/Visas/Visas.razor` (mới), `Components/Pages/Visas/VisaDialog.razor` (mới), `Components/Pages/Visas/FlightDialog.razor` (mới), `Display/Labels.cs`. Xóa placeholder `Pages/Visa.razor`.
- **Đã test (P1.6):** build 0 error; login admin → `/visa` 200, có tab "Hồ sơ Visa" + nút "Thêm hồ sơ visa", 2 tab header render. (Demo chưa seed Visa/Flight nên bảng trống.)
- **Làm được (P1.5):** Module **Đại lý & Hoa hồng**. Trang `Pages/Agents/Agents.razor` (`/agents`, thay placeholder) list + tìm kiếm + nút "Thêm đại lý"; `AgentDialog.razor` tạo/sửa (tên/đại diện/liên hệ/ngân hàng/IsActive, sinh `AG-XXXXXX`), gate `agents:*`. `AgentDetail.razor` (`/agents/{id}`): thông tin + nút Sửa + **ứng viên giới thiệu** (`Candidate.AgentId`) + **bảng hoa hồng** (`AgentCommission`, mốc `CommissionMilestone`, trạng thái `CommissionStatus`) + tổng hoa hồng. Thêm nhãn VN + `ColorOf(CommissionStatus)` vào `Labels.cs`. Đặt page trong folder `Pages/Agents/` tránh xung đột class/namespace.
- **File thay đổi chính (P1.5):** `Components/Pages/Agents/Agents.razor` (mới), `Components/Pages/Agents/AgentDialog.razor` (mới), `Components/Pages/Agents/AgentDetail.razor` (mới), `Display/Labels.cs`. Xóa placeholder `Pages/Agents.razor`.
- **Đã test (P1.5):** build 0 error; login admin → `/agents` 200 (nút "Thêm đại lý"), `/agents/{id}` 200 (nút "Sửa đại lý" + "Ứng viên giới thiệu" + "Hoa hồng").
- **Làm được (P1.4):** Module **Tài chính** hoàn chỉnh. Trang `Pages/Finance/Finance.razor` (`/finance`) 2 tab: **Khoản thu** (Payments) + **Khoản chi** (Expenses), mỗi tab có DataGrid + nút Thêm + nút Sửa từng dòng, cộng 3 thẻ tổng quan (đã thu / còn phải thu / đã chi). `PaymentDialog.razor` (chọn ứng viên bắt buộc, loại/số tiền/trạng thái/phương thức/hạn thu/ngày thu, sinh `PT-...`) và `ExpenseDialog.razor` (loại/số tiền/ngày chi/mô tả, sinh `EX-...`), gate `payments:*`/`expenses:*`. Thêm nhãn VN + `ColorOf(PaymentStatus)` vào `Labels.cs`. **Lưu ý đã xử lý:** đã di chuyển page từ `Pages/Finance.razor` vào `Pages/Finance/Finance.razor` để tránh xung đột class `Finance` vs namespace `Pages.Finance` (folder chứa 2 dialog).
- **File thay đổi chính (P1.4):** `Components/Pages/Finance/Finance.razor` (mới, thay placeholder), `Components/Pages/Finance/PaymentDialog.razor` (mới), `Components/Pages/Finance/ExpenseDialog.razor` (mới), `Display/Labels.cs`.
- **Đã test (P1.4):** build 0 error; login admin → `/finance` 200, có tab "Khoản thu"/"Khoản chi" + nút "Thêm khoản thu".
- **🐞 FIX QUAN TRỌNG — nút/dialog không bấm được:** Nguyên nhân: app dùng `@rendermode InteractiveServer` **theo từng trang**, nên `MainLayout` + các MudBlazor provider (`MudDialogProvider`/`MudSnackbarProvider`/`MudPopoverProvider`) bị render **tĩnh** → dialog (Thêm/Sửa) không hiện, snackbar không báo, nút drawer/menu tài khoản chết. Sửa: bật **interactivity toàn cục** — đặt `@rendermode="PageRenderMode"` ở `<Routes>` + `<HeadOutlet>` trong `App.razor` (PageRenderMode = `InteractiveServer` trừ trang có `[ExcludeFromInteractiveRouting]`), thêm `[ExcludeFromInteractiveRouting]` vào `Login.razor` (giữ SSR để `SignInManager` set cookie), và **gỡ `@rendermode InteractiveServer` khỏi 15 trang/dialog** (nếu để lại sẽ lỗi "render mode đã set ở ancestor"). Dùng `HttpContext.AcceptsInteractiveRouting()` **có sẵn trong .NET 10** (không tự viết extension — sẽ trùng tên).
- **Làm được:** Hoàn thành **P1.3 Đơn hàng CRUD**. Tạo `JobOrderDialog.razor` — form tạo/sửa đơn hàng (Country, Status, CompanyName, UnionName, Field, Quantity, SalaryDescription, CostAmount, Requirements, Recruitment/Departure dates), sinh Code `JO-yyyyMM-XXX`, gate `job_orders:create/update`. Thêm nút "Thêm đơn hàng" + card bấm được vào `JobOrders.razor`. Tạo trang chi tiết `JobOrderDetail.razor` (`/job-orders/{id}`): hiển thị đầy đủ thông tin, nút "Sửa đơn hàng", và **DataGrid liệt kê ứng viên trong đơn hàng** (join `CandidateJobOrder` → bấm sang `/candidates/{id}`). Trước đó cũng đã **xuất 2 file backup DB** vào `db-backups/` (`.sql` + `.dump`) theo yêu cầu người dùng.
- **File thay đổi chính:** `Components/Pages/JobOrders/JobOrderDialog.razor` (mới), `Components/Pages/JobOrders/JobOrderDetail.razor` (mới), `Components/Pages/JobOrders/JobOrders.razor`. (Phụ: `db-backups/` chứa dump DB.)
- **Đã test:** `dotnet build Polymind.slnx --no-incremental` = 0 error, chỉ còn 1 warning cũ `BL0008` ở Login.razor (đã sửa warning MUD0002 `PanelClass` trong CandidateDialog của Session 6). Chạy app + login admin qua HTTP: `/job-orders` 200 (có "Thêm đơn hàng"), `/job-orders/{id}` 200 (có "Sửa đơn hàng" + section "Ứng viên trong đơn hàng"). Dialog tương tác (Blazor circuit) chưa click thủ công.
- **Lưu ý/cảnh báo cho người sau:** (1) Bảng Postgres dùng **snake_case** (`job_orders`, cột `id`) khi query psql trực tiếp — KHÔNG phải `"JobOrders"`. (2) Web app đang chạy nền `:5177` (log `C:\tmp\polymind-web.*.log`) — dừng `Stop-Process -Name Polymind.Web` trước khi build lại nếu không sẽ lỗi lock exe. (3) `db-backups/` chứa password hash user mẫu — cân nhắc thêm vào `.gitignore` nếu không muốn push (đã hỏi user, chưa làm). (4) Pattern dialog DateOnly: dùng `DateTime?` trong FormModel rồi map qua `ToDateTime(TimeOnly.MinValue)` / `DateOnly.FromDateTime`.

### [2026-06-24] Session 6 — Claude
- **Làm được:** Hoàn thành **P1.2 Ứng viên CRUD đầy đủ**. Tạo `CandidateDialog.razor` — form tạo/sửa ứng viên có 4 tab (Thông tin cá nhân / Giấy tờ / Liên hệ khẩn cấp / Ngân hàng), dùng FormModel với `DateTime?` cho MudDatePicker rồi map sang `DateOnly?`. Thêm nút "Thêm ứng viên" (gate `candidates:create`) vào `Candidates.razor`. Nâng cấp `CandidateDetail.razor`: nút "Sửa thông tin" (gate `candidates:update`), hiển thị đầy đủ thông tin cá nhân/giấy tờ/liên hệ/ngân hàng, và mục **"Gắn vào đơn hàng"** tạo `CandidateJobOrder` (CurrentStep=Lead, Status=Active) khi ứng viên chưa có đơn hàng. Thêm nhãn VN cho `Gender`/`MaritalStatus`/`CandidateJobOrderStatus` vào `Labels.cs`.
- **File thay đổi chính:** `Components/Pages/Candidates/CandidateDialog.razor` (mới), `Components/Pages/Candidates/Candidates.razor`, `Components/Pages/Candidates/CandidateDetail.razor`, `Display/Labels.cs`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 0 warning. Chạy app + login admin qua HTTP: `/candidates` 200 (có nút "Thêm ứng viên"), `/candidates/{id}` 200 (có nút "Sửa thông tin" + section 17 bước). Dialog/assign tương tác (Blazor circuit) chưa test bằng click thủ công.
- **Lưu ý/cảnh báo cho người sau:** (1) `CandidateJobOrder` KHÔNG có field `CreatedBy` (chỉ AssignedTo) — khác Candidate/WorkflowStepRecord. (2) Web app đang chạy nền listen `:5177` (log `C:\tmp\polymind-web.*.log`) — phải dừng process trước khi build lại nếu không sẽ lỗi lock `Polymind.Web.exe`. (3) Bẫy DateTimeOffset UTC vẫn áp dụng; DateOnly map qua `ToDateTime(TimeOnly.MinValue)`/`DateOnly.FromDateTime`. (4) Convert lead→ứng viên vẫn KHÔNG tự tạo CandidateJobOrder — phải gắn đơn hàng thủ công ở trang chi tiết.

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
