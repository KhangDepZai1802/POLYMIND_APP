# BÀN GIAO DỰ ÁN — POLYMIND OLMS (cho AI/Dev tiếp nhận)

> Mục tiêu: hoàn thiện nhanh nhất bản web hoàn chỉnh. Tài liệu này mô tả **đã làm gì**, **quy ước phải tuân theo**, **bẫy kỹ thuật**, và **việc cần làm tiếp theo có ưu tiên**.

---

## 0. Chạy dự án

```powershell
docker compose up -d                      # Postgres(5432) + Redis(6379) + MinIO(9000/9001)
dotnet run --project src/Polymind.Web     # http://localhost:5177
```
- Đăng nhập: **admin@polymind.local / Admin@123**
- Build toàn bộ: `dotnet build Polymind.slnx`
- Tạo migration: `dotnet ef migrations add <Name> --project src/Polymind.Infrastructure --startup-project src/Polymind.Web --output-dir Persistence/Migrations`
- Áp migration: `dotnet ef database update --project src/Polymind.Infrastructure --startup-project src/Polymind.Web`

---

## 1. Tech stack (THỰC TẾ đang dùng)

- **.NET 10** (net10.0), solution dạng `Polymind.slnx`
- **Blazor Web App** — render mode **Interactive Server**, UI **MudBlazor 9.5.0**
- **EF Core 10** + **Npgsql** + **PostgreSQL 16**; bảng **snake_case** (EFCore.NamingConventions), enum lưu **string**
- **ASP.NET Core Identity** (IdentityUser/Role<Guid>) + cookie auth
- FluentValidation (đã add, chưa dùng). Redis/MinIO/Hangfire/QuestPDF/ClosedXML: **chưa wiring**

## 2. Cấu trúc solution (Clean Architecture)

```
src/
├── Polymind.Domain          // 20 entities + Enums.cs + Common/BaseEntity.cs (KHÔNG phụ thuộc gì)
├── Polymind.Application      // (gần như trống) — nơi đặt use-cases/CQRS/validators về sau
├── Polymind.Infrastructure   // EF Core, Identity, Seeder, Migrations
│   ├── DependencyInjection.cs            // AddInfrastructure()
│   ├── Identity/ApplicationUser.cs, ApplicationRole.cs
│   └── Persistence/ ApplicationDbContext.cs, DbSeeder.cs, DemoDataSeeder.cs,
│                    PermissionRegistry.cs, Constants/RoleNames.cs, Migrations/
└── Polymind.Web              // Blazor UI
    ├── Program.cs                         // DI, auth, logout endpoint, seed lúc startup
    ├── Identity/IdentityRevalidatingAuthenticationStateProvider.cs
    ├── Display/Labels.cs                  // nhãn tiếng Việt + màu cho enum
    └── Components/
        ├── App.razor, Routes.razor, _Imports.razor, RedirectToLogin.razor
        ├── Account/Login.razor            // SSR, dùng SignInManager
        ├── Layout/ MainLayout.razor (MudLayout+providers), NavMenu.razor, EmptyLayout.razor
        ├── Shared/ComingSoon.razor
        └── Pages/ Home.razor(/), Leads/, Candidates/, JobOrders/,
                   Finance/Agents/Visa/Reports (placeholder)
```

---

## 3. ĐÃ LÀM ĐƯỢC (checklist)

**Nền tảng & DB**
- [x] 20 entity map đủ schema docs/02 (Lead, Candidate, JobOrder, CandidateJobOrder, WorkflowStepRecord, Payment, Receipt, Expense, Agent, AgentCommissionConfig, AgentCommission, Visa, Flight, Notification, AuditLog, Permission, RolePermission...)
- [x] DbContext + migration InitialCreate (28 bảng), index theo thiết kế
- [x] Seed: 8 roles, 80 permissions, super_admin (full quyền), tài khoản admin
- [x] DemoDataSeeder (Development): 40 lead, 12 ứng viên có tiến độ workflow, 5 đơn hàng, 3 đại lý, 12 payment

**Auth**
- [x] Login SSR (cookie) + Logout (`POST /Account/Logout`)
- [x] AuthenticationStateProvider (revalidating security-stamp)
- [x] Bảo vệ route: `AuthorizeRouteView` + `RedirectToLogin`; pages có `[Authorize]`

**UI/Modules**
- [x] App shell MudBlazor: Topbar (user menu, logout) + Sidebar (8 mục) + theme
- [x] Dashboard `/`: 6 KPI cards + lead theo trạng thái/nguồn
- [x] Lead CRM: list (search/filter trạng thái+nguồn/paginate), dialog thêm, chi tiết (timeline + đổi trạng thái + **convert→ứng viên**)
- [x] Ứng viên: list + chi tiết **timeline 17 bước** + nút "chuyển bước tiếp theo"
- [x] Đơn hàng: list dạng card
- [x] Placeholder: /finance /agents /visa /reports

---

## 4. QUY ƯỚC BẮT BUỘC TUÂN THEO

1. **Trang app** = `@page` + `@attribute [Authorize]` + `@rendermode InteractiveServer`. Inject `IDbContextFactory<ApplicationDbContext>`, mỗi thao tác `await using var db = await DbFactory.CreateDbContextAsync();` (KHÔNG dùng 1 DbContext scoped lâu dài trong component).
2. **Trang cần set cookie** (login...) phải để **SSR** (không `@rendermode`) + `[AllowAnonymous]` + `EmptyLayout`.
3. Enum hiển thị qua `Polymind.Web.Display.Labels.Vi(...)`. Thêm enum mới → thêm nhãn ở đây.
4. Đặt tên DB snake_case (tự động). Tên bảng Identity: `users`, `roles` (đã ToTable), còn AspNet* giữ mặc định.
5. Component con của MudBlazor (dialog) cần `@rendermode InteractiveServer`.

## 5. BẪY KỸ THUẬT (đã gặp — đừng lặp lại)

- ⚠️ **DateTimeOffset gửi xuống Postgres PHẢI offset 0 (UTC).** KHÔNG dùng `DateTimeOffset.UtcNow.Date` (EF ép thành +7 → Npgsql ném lỗi). Dùng `new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero)`.
- `dotnet new sln` ở .NET 10 tạo `.slnx` (XML), build bằng `dotnet build Polymind.slnx`.
- `AddIdentity` cần `FrameworkReference Microsoft.AspNetCore.App` trong Infrastructure.csproj (đã thêm).
- Startup project (Web) cần package `Microsoft.EntityFrameworkCore.Design` để chạy ef.
- DI dùng `AddDbContextFactory` + 1 đăng ký scoped wrapper (cho Identity/seeder). Đừng đổi lại thành AddDbContext thường.
- Re-seed demo: `TRUNCATE` các bảng nghiệp vụ rồi restart app (DemoDataSeeder idempotent theo "đã có lead chưa").

---

## 6. CẦN LÀM TIẾP — ƯU TIÊN ĐỂ HOÀN THIỆN NHANH NHẤT

### 🔴 P1 — Hoàn chỉnh lõi đã bắt đầu (làm trước, giá trị cao nhất)
1. **RBAC thực thi** (Phase 1 chưa xong phần này):
   - Tạo 8 tài khoản mẫu (mỗi vai trò 1 user) trong DbSeeder.
   - Gán permissions cho 7 vai trò còn lại (map resource→action theo bộ phận; xem docs/03 mục 8).
   - Tạo claims/policy: nạp permission của user thành claims khi login; thêm `IAuthorizationHandler` cho policy dạng `"leads:create"`.
   - **Ẩn menu theo quyền** (NavMenu) + chặn nút/hành động theo permission.
2. **Ứng viên CRUD đầy đủ**: form tạo/sửa ứng viên (hiện chỉ tạo qua convert), gắn ứng viên vào đơn hàng (CandidateJobOrder), thông tin cá nhân đầy đủ (CCCD/passport/ngân hàng/người thân).
3. **Hồ sơ ứng viên + upload file** (cần wiring **MinIO**): upload CCCD/hộ chiếu/bằng cấp..., versioning (CandidateDocument/DocumentVersion), tải xuống qua presigned URL.
4. **Đơn hàng CRUD** (hiện read-only): thêm/sửa/đổi trạng thái đơn hàng.
5. **Audit log**: middleware/interceptor ghi mọi create/update/delete vào `audit_logs`.

### 🟠 P2 — Tài chính & Hoa hồng (Phase 2)
6. **Tài chính** `/finance`: khoản thu (Payment) CRUD + xác nhận thanh toán + công nợ ứng viên; khoản chi (Expense); **phiếu thu/chi PDF** (wiring **QuestPDF**).
7. **Hoa hồng đại lý**: quản lý đại lý CRUD, cấu hình % theo mốc (AgentCommissionConfig); **tự động tính hoa hồng** khi ứng viên đạt mốc deposit/selected/departure (trigger trong service); duyệt → thanh toán.
8. **Portal đại lý**: tách auth/role `agent`, chỉ xem ứng viên đã giới thiệu + hoa hồng (read-only). (Có thể là area riêng hoặc project Polymind.AgentPortal.)

### 🟡 P3 — Visa, Xuất cảnh, Báo cáo, Thông báo (Phase 3)
9. **Visa** `/visa`: CRUD hồ sơ Visa + sub-flow (submitted→additional_required→approved/rejected), lịch phỏng vấn.
10. **Xuất cảnh**: quản lý vé (Flight), xác nhận xuất cảnh thực tế.
11. **Báo cáo** `/reports`: báo cáo Lead/tuyển dụng/tài chính/đại lý; **export Excel** (wiring **ClosedXML**) + PDF.
12. **Thông báo tự động** (wiring **Hangfire** + Redis): job nhắc hồ sơ/thanh toán/phỏng vấn/xuất cảnh/hoa hồng qua Email(SMTP)/SMS(eSMS)/Zalo OA/in-app; bảng `notifications` + chuông in-app (SignalR).

### ⚪ P4 — Tích hợp (Phase 4)
13. Facebook/TikTok/Google Lead API, Zalo OA, OCR CCCD/Passport, Chữ ký số, Mobile App, BI/AI.

---

## 7. Hạ tầng cần wiring (gắn khi tới phần dùng)
- **MinIO**: client `Minio` hoặc AWS SDK S3 → upload hồ sơ (P1.3), PDF (P2.6).
- **Hangfire** (Hangfire.AspNetCore + Hangfire.PostgreSql): job thông báo (P3.12).
- **Redis** (StackExchange.Redis): cache + dashboard Hangfire.
- **QuestPDF** (phiếu thu chi), **ClosedXML** (export Excel).
- **FluentValidation**: validator cho form/DTO (đã add package ở Application).

## 8. Tài liệu gốc tham chiếu
- docs/01-business-analysis.md — nghiệp vụ, 8 role, 8 module, phase
- docs/02-database-design.md — schema 20 bảng (đã map thành entity)
- docs/03-workflow.md — 17 bước, hoa hồng, thông báo, **phân công bộ phận (mục 8)** ← dùng cho RBAC
- docs/04-system-architecture.md — kiến trúc (lưu ý: viết theo stack Node cũ, nay đã đổi sang .NET)
- docs/05-handoff-codex.md — **tài liệu bàn giao này**
