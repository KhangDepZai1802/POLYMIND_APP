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
- Đã xong: nền tảng + auth + Dashboard + Lead CRM + **Ứng viên CRUD** + **Đơn hàng CRUD** + **Tài chính (Thu/Chi)** + **Đại lý & Hoa hồng** + **Visa & Xuất cảnh (Visa + Vé máy bay CRUD)** + **Báo cáo & Thống kê** + demo data. **Không còn placeholder nào** — tất cả menu đều có trang thật.
- **Demo data đầy đủ (Session 9):** đã seed bù **Visa(4) + Vé máy bay(3) + Cấu hình hoa hồng(9) + Hoa hồng phát sinh(8)** → trang Báo cáo "Hoa hồng theo đại lý" và trang Visa đã có dữ liệu thật. Đã smoke-test 12 trang (admin 200, không exception) + RBAC 4 role (recruiter/accountant/visa.staff/agent) không lỗi. **Sẵn sàng demo đối tác.**
- **ĐỢT 1 đã xong (Session 10):** UI cấu hình hoa hồng theo đại lý, tự sinh `AgentCommission` khi ứng viên đạt mốc Deposit/Selected/Departure, duyệt khoản thu và duyệt/đánh dấu đã chi hoa hồng. Build xanh, smoke-test `/finance`, `/agents/{id}`, `/candidates/{id}` HTTP 200.
- **ĐỢT 2 đã xong (Session 11):** audit log thực thi cho các thao tác chính (ứng viên, payment/expense, chuyển bước, hoa hồng, cấu hình hoa hồng) và thêm section **Công nợ ứng viên** trong `CandidateDetail` gated bằng `payments:read`.
- **ĐỢT 3 đã xong (Session 12):** wiring MinIO thật bằng package `Minio`, cấu hình bucket `polymind-documents`, service upload/download hồ sơ, UI **Hồ sơ ứng viên** trong `CandidateDetail` với upload version + link tải presigned URL.
- **ĐỢT 4 đã xong (Session 13):** (1) **Báo cáo mở rộng** — thêm 4 KPI (công nợ phải thu, khoản thu quá hạn, hồ sơ đã tải, sắp xuất cảnh 30 ngày) + 4 section mới (hồ sơ theo loại, khoản thu quá hạn, lịch visa & lịch xuất cảnh 30 ngày). (2) **Dashboard Home** thêm 3 KPI (công nợ/quá hạn/sắp xuất cảnh). (3) **Thông báo in-app (stub)** — `NotificationService` sinh reminder idempotent (khoản thu quá hạn/sắp tới, lịch visa, xuất cảnh, hồ sơ thiếu), bell badge ở topbar, trang `/notifications` (list + đánh dấu đã đọc + quét lại). (4) **Export CSV** — 3 endpoint `/export/*.csv` (thu/chi theo tháng, hoa hồng, khoản thu quá hạn) gated `reports:read`, BOM UTF-8 cho tiếng Việt, nút "Xuất CSV" trên trang Báo cáo. Build xanh, smoke-test `/`, `/reports`, `/notifications` = 200 không error-boundary, 3 CSV trả đúng `text/csv` + tiếng Việt đúng dấu, DB sinh 8 reminder thật.
- **Kế hoạch nâng cấp:** xem `C:\Users\khang\.claude\plans\t-nh-ng-c-i-b-n-lexical-hejlsberg.md` (lộ trình 5 đợt; chốt giữ kiến trúc Blazor Server + Cookie, MinIO wiring thật, Email/SMS/Zalo stub).
- **RBAC thực thi đã có code nền:** seed 8 user mẫu/role permissions, permission claims khi login, policy động dạng `resource:action`, ẩn menu theo quyền và chặn các nút ghi dữ liệu chính.
- **Đã sửa lỗi nút/dialog không bấm được:** chuyển sang **interactivity toàn cục** (render mode đặt ở `Routes`/`HeadOutlet` trong `App.razor`, Login dùng `[ExcludeFromInteractiveRouting]` để giữ SSR). Trước đó layout + MudBlazor providers bị render tĩnh nên dialog/snackbar/drawer chết.

## ⏭️ VIỆC TIẾP THEO (baton — làm cái này trước)

**Đã xong ĐỢT 4 — Báo cáo/Dashboard mở rộng + thông báo stub + export CSV.** Tiếp theo là **ĐỢT 5 — Portal đại lý + siết RBAC data-scope** (mục cuối plan file):
1. **Portal đại lý (spec §2.8):** role `agent` chỉ xem ứng viên **mình giới thiệu** (`Candidate.AgentId == currentAgentId`) + hoa hồng của mình; siết `/candidates` và `/reports` đang quá rộng. Cần map user↔Agent (hiện chưa có liên kết) — kiểm `ApplicationUser`/seed để biết cách gắn.
2. **RBAC data-scope:** lọc dữ liệu theo quyền sở hữu, không chỉ ẩn menu. Cân nhắc helper scope chung cho các trang list.
3. (Tùy chọn) Nâng cấp thông báo: gửi Email/SMS/Zalo thật (hiện chỉ stub in-app); chạy sinh reminder định kỳ bằng Hangfire thay vì sinh khi mở trang.

**Lưu ý Đợt 4 cho người sau:**
- Trang `/notifications`: class page `Notifications` **trùng tên** property inject → đã đổi tên service inject thành `NotificationSvc` (đừng đặt lại thành `Notifications`).
- Reminder sinh **khi mở trang/topbar** (bell `OnInitializedAsync` gọi `GenerateRemindersAsync`), idempotent theo `(UserId, Type, ReferenceId)`. Hiện nhắm recipient = **user đang đăng nhập** (demo). Nếu cần multi-user thật thì sinh theo người phụ trách.
- Export CSV là **minimal-API endpoint** trong `Program.cs` (`MapCsvExportEndpoints`), gated `.RequireAuthorization("reports:read")` qua policy provider động. Dùng BOM UTF-8 (`Encoding.UTF8.GetPreamble()`) để Excel đọc đúng tiếng Việt. Không ghi file ra repo — stream `Results.File`.
- MudIcon render ra SVG path (không có tên icon trong HTML) và MudMenu render lazy → grep HTML tìm tên icon/menu sẽ "miss"; verify bằng HTTP 200 + vắng `blazor-error-boundary` + query DB.

**Nợ nhỏ phát hiện ở Session 9:** role `agent` đang thấy `/candidates` và `/reports` hơi rộng so với spec §2.8 (Portal đại lý chỉ xem ứng viên mình giới thiệu + hoa hồng) → siết khi làm Portal đại lý.

**LƯU Ý khi tạo trang mới (giữ nguyên):** KHÔNG thêm `@rendermode`; đặt page trong folder + đừng đặt tên class trùng entity. MudBlazor 9.5 — tránh `MudChart` (API đổi, cảnh báo `MUD0002`, attribute bị nuốt im lặng); dùng `MudProgressLinear` + `MudSimpleTable`. **Seed mở rộng:** `DemoDataSeeder.SeedExtrasAsync` chạy idempotent theo từng bảng kể cả khi DB đã có Lead (bù Visa/Flight/Commission mà không cần xóa DB).

## 🚧 BLOCKERS / NỢ KỸ THUẬT

- (chưa có blocker)
- Nợ wiring: MinIO, Hangfire, Redis, QuestPDF, ClosedXML — gắn khi tới phần dùng.
- ~~Module placeholder chờ làm thật~~ — ĐÃ XONG HẾT (finance/agents/visa/reports đều có trang thật).

---

## 📜 NHẬT KÝ SESSION (mới nhất ở trên)

### [2026-06-24] Session 13 — Claude
- **Làm được:** Hoàn thành **ĐỢT 4 — Báo cáo/Dashboard mở rộng + Thông báo stub + Export CSV**.
  - **Thông báo in-app:** `Notifications/NotificationService.cs` (scoped DI) sinh reminder idempotent theo `(UserId, Type, ReferenceId)` cho: khoản thu quá hạn/sắp đến hạn (≤7 ngày), lịch phỏng vấn/kết quả visa (≤7 ngày), lịch xuất cảnh (≤7 ngày), hồ sơ còn thiếu (ứng viên ≥ bước Hoàn thiện hồ sơ nhưng chưa có tài liệu). Bell badge `Components/Layout/NotificationBell.razor` ở topbar (đếm chưa đọc), trang `/notifications` (gate `notifications:read`): list + đánh dấu đã đọc/đọc tất cả + nút "Quét nhắc việc". Nhãn + icon + màu `NotificationType` trong `Labels.cs`. Link NavMenu.
  - **Báo cáo mở rộng** (`Reports.razor`): thêm 4 KPI (công nợ phải thu, khoản thu quá hạn, hồ sơ đã tải, sắp xuất cảnh 30 ngày) + 4 section MudSimpleTable/MudProgressLinear (hồ sơ theo loại, khoản thu quá hạn, lịch visa 30 ngày, lịch xuất cảnh 30 ngày). Nút **Xuất CSV** (MudMenu).
  - **Dashboard Home:** thêm 3 KPI (công nợ phải thu, khoản thu quá hạn, sắp xuất cảnh 30 ngày).
  - **Export CSV:** `Reporting/CsvExportEndpoints.cs` + `app.MapCsvExportEndpoints()`; 3 endpoint `/export/finance-monthly.csv`, `/export/commissions.csv`, `/export/overdue-payments.csv` gated `reports:read`, BOM UTF-8, stream trực tiếp (không ghi file repo).
- **File thay đổi chính:** `Notifications/NotificationService.cs` (mới), `Reporting/CsvExportEndpoints.cs` (mới), `Components/Layout/NotificationBell.razor` (mới), `Components/Pages/Notifications/Notifications.razor` (mới), `Components/Pages/Reports/Reports.razor`, `Components/Pages/Home.razor`, `Components/Layout/MainLayout.razor`, `Components/Layout/NavMenu.razor`, `Display/Labels.cs`, `Program.cs`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 1 warning cũ `BL0008` (Login.razor). Docker up; chạy web `:5177`, login admin qua HTTP: `/`, `/reports`, `/notifications` = 200, không `blazor-error-boundary`. 3 endpoint CSV = 200 `text/csv; charset=utf-8`, tiếng Việt đúng dấu (BOM OK), finance CSV có 12 dòng tháng. DB: `notifications` sinh 8 bản ghi thật (ReminderDocument) — chứng minh sinh + persist idempotent chạy đúng.
- **Lưu ý/cảnh báo cho người sau:** (1) Class page `Notifications` trùng tên property inject → service inject đặt tên `NotificationSvc`. (2) Reminder hiện nhắm recipient = user đang đăng nhập (demo); demo data không có payment quá hạn/visa/flight trong 7 ngày tới nên chỉ phát ReminderDocument — đúng logic, không phải lỗi. (3) Web app đang chạy `:5177` (PID khi chạy, log `C:\tmp\polymind-web-phase4.*.log`) — `Stop-Process -Name Polymind.Web` trước khi build lại. (4) Grep HTML tìm tên icon/menu sẽ miss (MudIcon→SVG, MudMenu lazy) — verify bằng 200 + vắng error-boundary + query DB.

### [2026-06-24] Session 12 — Codex
- **Làm được:** Hoàn thành **ĐỢT 3 — Upload hồ sơ + MinIO**. Thêm package `Minio` 7.0.0 cho Web, cấu hình `Minio` trong `appsettings.json`, service `IDocumentStorage`/`MinioDocumentStorage` upload object vào bucket `polymind-documents`, tạo presigned URL tải hồ sơ, validate file PDF/ảnh/Word/Excel tối đa 20MB. Mở rộng `CandidateDetail`: section **Hồ sơ ứng viên**, chọn `DocumentType`, chọn file bằng `InputFile`, upload version mới vào `CandidateDocument` + `DocumentVersion`, cập nhật `CurrentVersionId`, audit `create/upload_version`, danh sách hồ sơ hiện hành + nút tải.
- **File thay đổi chính:** `src/Polymind.Web/Storage/*`, `Polymind.Web.csproj`, `Program.cs`, `appsettings.json`, `Components/Pages/Candidates/CandidateDetail.razor`, `Display/Labels.cs`, `Components/_Imports.razor`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 1 warning cũ `BL0008` ở `Login.razor`. `docker compose up -d`; MinIO health `http://localhost:9000/minio/health/live` = 200. Chạy web + login admin qua HTTP: `/candidates/{id}` = 200, render "Hồ sơ ứng viên", "Upload hồ sơ", "Công nợ ứng viên", không lỗi hiển thị.
- **Lưu ý/cảnh báo cho người sau:** Chưa click upload file thật trong browser nên chưa xác nhận object/bucket được tạo thực tế qua UI; service đã compile với SDK MinIO và MinIO container health OK. `DocumentVersion.FileUrl` hiện lưu **object key** trong bucket, không lưu public URL. Web app đang chạy tại `http://localhost:5177` (log `C:\tmp\polymind-web-phase3.out.log`).

### [2026-06-24] Session 11 — Codex
- **Làm được:** Hoàn thành **ĐỢT 2 — Audit Log + Công nợ theo ứng viên**. Thêm helper `AuditLogHelpers` để lấy actor từ claim `NameIdentifier` và ghi `AuditLog` JSONB. Gắn audit cho tạo/sửa ứng viên, tạo/sửa khoản thu, tạo/sửa khoản chi, duyệt payment, cấu hình hoa hồng, duyệt/đánh dấu đã chi hoa hồng, gắn ứng viên vào đơn hàng, chuyển bước workflow và hoa hồng tự sinh. Thêm section **Công nợ ứng viên** trong `CandidateDetail` gồm phải thu/đã thu/còn nợ, cảnh báo quá hạn, lịch sử payment; section chỉ hiện khi user có `payments:read`.
- **File thay đổi chính:** `src/Polymind.Web/Auditing/AuditLogHelpers.cs`, `Components/_Imports.razor`, `Components/Pages/Candidates/CandidateDetail.razor`, `CandidateDialog.razor`, `Finance/PaymentDialog.razor`, `Finance/ExpenseDialog.razor`, `Finance/Finance.razor`, `Agents/AgentDetail.razor`, `Agents/CommissionConfigDialog.razor`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 0 warning sau khi dừng process web cũ giữ lock. Bật Docker + web app, login admin qua HTTP, smoke-test `/finance`, `/agents/{id}`, `/candidates/{id}` đều 200, không lỗi hiển thị; trang ứng viên render "Công nợ ứng viên", trang đại lý vẫn render "Cấu hình hoa hồng".
- **Lưu ý/cảnh báo cho người sau:** Web app đang chạy lại tại `http://localhost:5177` (process `Polymind.Web`, log `C:\tmp\polymind-web-phase2.out.log`). Chưa click thủ công các dialog/nút duyệt để tạo audit rows thật; mới kiểm tra build + HTTP render. Audit helper không set IP/UserAgent vì component Blazor hiện chưa inject `HttpContext`; nếu cần truy vết security sâu hơn thì bổ sung accessor/service riêng.

### [2026-06-24] Session 10 — Codex
- **Làm được:** Tiếp quản phần ĐỢT 1 Claude đang làm dở/đã commit (`3ff886f done phase 1`) và xác minh hoàn tất: `CommissionConfigDialog` tạo/sửa cấu hình hoa hồng; `AgentDetail` hiển thị cấu hình + duyệt/đánh dấu đã chi hoa hồng; `CandidateDetail` tự sinh hoa hồng idempotent khi chuyển bước đạt Deposit/Selected/Departure; `Finance` có nút duyệt khoản thu. Xác nhận `payments:approve` / `commissions:approve` đã có trong `PermissionRegistry` và đã map cho Director/Accountant trong `DbSeeder`.
- **File thay đổi chính:** `WORKLOG.md` cập nhật baton; code Đợt 1 đã có ở `CandidateDetail.razor`, `Finance.razor`, `AgentDetail.razor`, `CommissionConfigDialog.razor`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, 1 warning cũ `BL0008` ở `Login.razor`. Bật Docker + web app, login admin qua HTTP, smoke-test `/finance`, `/agents/{id}`, `/candidates/{id}` đều 200, không có lỗi hiển thị; section "Cấu hình hoa hồng" và "Quy trình 17 bước" render đúng.
- **Lưu ý/cảnh báo cho người sau:** Web app đang chạy tại `http://localhost:5177` (process `Polymind.Web`, log `C:\tmp\polymind-web-phase1.out.log`). Khi start bằng `Start-Process`, project path có dấu cách phải bọc quote trong `ArgumentList`; nếu không dotnet báo `The provided file path does not exist: ...\POLYMIND`. Chưa click thủ công dialog/nút duyệt trong browser, mới smoke-test HTTP render.

### [2026-06-24] Session 9 — Claude
- **Bối cảnh:** đối chiếu toàn web với `POLYMIND APP.docx` → lập kế hoạch nâng cấp 5 đợt (plan file `t-nh-ng-c-i-b-n-lexical-hejlsberg.md`). Demo đối tác **ngày mai** → làm **ĐỢT 0** (rủi ro thấp, giá trị demo cao). Chốt: giữ Blazor Server + Cookie (không thêm REST/JWT trước demo), MinIO wiring thật/còn lại stub.
- **Làm được (ĐỢT 0.1 — seed demo mở rộng):** thêm `DemoDataSeeder.SeedExtrasAsync` (idempotent theo từng bảng, chạy cả khi DB đã có Lead). Sinh **Visa** (ứng viên ≥ bước VisaSubmit), **Vé máy bay** (≥ BookFlight, set `ActualDepartureAt` nếu ≥ Departure), **AgentCommissionConfig** (mỗi đại lý 20%/30%/50% theo mốc Deposit/Selected/Departure), **AgentCommission** (ứng viên có `AgentId` đã đạt mốc, BaseAmount = `JobOrder.CostAmount`). Sửa guard `SeedAsync`: thay `if(AnyAsync) return` bằng nhánh gọi extras rồi return → core giữ nguyên indentation.
- **File thay đổi chính:** `src/Polymind.Infrastructure/Persistence/DemoDataSeeder.cs`.
- **Đã test:** build 0 error (1 warning cũ BL0008). Chạy app (Development) seed bù → DB: visas=4, flights=3, agent_commission_configs=9, agent_commissions=8. Đăng nhập admin: trang `/reports` "Hoa hồng theo đại lý" có dòng thật (Đại lý Miền Bắc 144tr, Hải Phòng 84tr), `/visa` có loại visa "Lao động".
- **Làm được (ĐỢT 0.2 — smoke-test demo):** quét **12 trang** dưới quyền admin → tất cả HTTP 200, không exception. Test **RBAC 4 role** (`recruiter/accountant/visa.staff/agent` @ `Admin@123`) → đều OK/DENIED hợp lý, không lỗi 500, cookie nhận quyền đúng.
- **Lưu ý/cảnh báo cho người sau:** (1) Login qua HTTP cần hidden `_handler=login` + `__RequestVerificationToken`. (2) Start-Process: path có dấu cách phải bọc `"..."` trong ArgumentList. (3) Role `agent` đang xem `/candidates` + `/reports` hơi rộng so với spec Portal đại lý — siết sau. (4) Tên hiển thị trong HTML bị HTML-entity-encode (vd `&#x110;...`) nên grep tiếng Việt có dấu trên HTML render dễ "miss"; check bằng class/`mud-table` hoặc số liệu ASCII.

### [2026-06-24] Session 8 — Claude
- **Làm được (P1.7 — placeholder CUỐI CÙNG):** Module **Báo cáo & Thống kê**. Trang `Pages/Reports/Reports.razor` (`/reports`, gate `reports:read`, thay placeholder `ComingSoon`): 6 thẻ KPI (tổng lead, tỉ lệ chuyển đổi, ứng viên đang xử lý, doanh thu đã thu, tổng chi, lợi nhuận gộp) + **Lead theo trạng thái** & **theo nguồn** (thanh `MudProgressLinear`) + **Doanh thu vs Chi phí 6 tháng gần nhất** (2 thanh thu/chi mỗi tháng, scale theo max) + **Ứng viên theo bước workflow** + **Hoa hồng theo đại lý** (`MudSimpleTable`: số mốc / đã chi / chờ-duyệt / tổng). Dữ liệu group bằng EF `GroupBy`; gom theo tháng làm trong bộ nhớ (tránh dịch DateOnly sang SQL). Doanh thu = Payments `Status==Paid` gom theo `PaidDate ?? CreatedAt`; chi phí theo `ExpenseDate`.
- **⚠️ BẪY MudBlazor 9.5 (quan trọng, đã xử lý):** ban đầu định dùng `MudChart` (Bar/Donut). Build báo `MUD0002 Illegal Attribute 'XAxisLabels'/'InputData'/'InputLabels'`. Lý do: trong MudBlazor 9.5 `MudChart<T>` là **generic** và đã **bỏ** các param `XAxisLabels`/`InputData`/`InputLabels` (chỉ còn `ChartSeries`/`ChartType`/`LegendPalette`); vì MudComponentBase có `CaptureUnmatchedValues` nên các attribute lạ **bị nuốt im lặng** (không lỗi compile nhưng biểu đồ không bind). Cũng gặp `ChartSeries`→`ChartSeries<T>`, `ChartOptions` không còn `YAxisFormat`. → **Quyết định: bỏ MudChart, render bằng `MudProgressLinear` + `MudSimpleTable`** (đã có sẵn pattern ở `Home.razor`) — build sạch, chắc chắn render. Nếu sau cần biểu đồ thật phải học API MudBlazor 9 Charts mới.
- **File thay đổi chính:** `Components/Pages/Reports/Reports.razor` (mới, trong folder). Xóa placeholder `Components/Pages/Reports.razor`.
- **Đã test:** `dotnet build Polymind.slnx` = 0 error, chỉ còn 1 warning cũ `BL0008` (Login.razor). Chạy app + login admin qua HTTP: `/reports` 200, đã xác nhận render 4 section ("Báo cáo & Thống kê", "Doanh thu vs Chi phí", "Ứng viên theo bước", "Hoa hồng theo đại lý"), 16 KPI icon, không có error-boundary, không còn `ComingSoon`. (Bảng hoa hồng hiện rỗng vì demo chưa seed `AgentCommission` — hiện đúng empty-state.)
- **Lưu ý/cảnh báo cho người sau:** (1) Login qua HTTP cần POST kèm hidden field `_handler=login` + `__RequestVerificationToken`, nếu thiếu sẽ 200 nhưng trả lại trang login. (2) Start-Process chạy app: path project có dấu cách → phải bọc `"..."` trong ArgumentList, nếu không dotnet cắt ở khoảng trắng (`...POLYMIND` rồi lỗi "file path does not exist"). (3) Để bảng Báo cáo có dữ liệu demo (hoa hồng/visa/flight) cần seed thêm — xem VIỆC TIẾP THEO (b).

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
