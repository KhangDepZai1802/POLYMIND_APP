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
   - **GIỮ GỌN (bắt buộc):** chỉ giữ **6 mục gần nhất** ở `TRẠNG THÁI HIỆN TẠI` và **6 entry gần nhất** ở `NHẬT KÝ SESSION`. Khi thêm mục/entry mới → **xóa mục/entry cũ nhất** để file không phình to (đỡ tốn token đọc lại mỗi phiên).
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

- **TÀI CHÍNH CHỈ TỪ BƯỚC ĐẶT CỌC + SỬA BẢNG/CHART/QUỐC GIA (Session 42, Codex).** Theo phản hồi user, module tài chính không còn cho ứng viên mới ở bước Lead/Tư vấn xuất hiện: thêm helper `FinanceEligibility` lấy assignment mới nhất và chỉ cho ứng viên `CurrentStep >= Deposit` vào Finance, dialog thêm khoản thu, KPI tài chính ở Tổng quan/Báo cáo. Bảng thi đua CTV đổi tên gạch chân thành chip `CTV-<tên>` giống trang Lead. Mở rộng CSS bảng cho `MudSimpleTable`/dialog thống kê để giảm chói; ScatterChart có tooltip SVG riêng khi rê vào điểm tỉnh/thành; màu Đức đổi khỏi `Color.Dark`, quốc gia lạ fallback màu Primary và bộ lọc Jobs vẫn sinh động từ DB. Build `Polymind.slnx` **0 warning/0 error**. **Lưu ý:** đã dừng process web khóa build (PID 21600), nhưng chưa restart server được vì tool escalation bị từ chối do giới hạn usage; cần chạy lại server thủ công nếu muốn xem ngay. **Chưa commit.**
- **GIẢM CHÓI BẢNG TOÀN WEB + TIN NHẮN CUỘN 6 TÀI KHOẢN (Session 41, Codex).** Theo screenshot user, bảng dark mode đang dùng sọc xám hơi gắt/chói; đã thêm CSS theme bảng toàn hệ thống trong `app.css` để header/row/hover dịu hơn ở cả sáng và tối, vẫn giữ tương phản đọc tốt. Trang chi tiết ứng viên bỏ gạch chân `.person-name` của tên TVV/CTV nhưng giữ nguyên hành vi click/dialog. Trang `/messages` bọc danh sách người nhận bằng vùng cuộn `message-contact-list-scroll`, tối đa khoảng 6 tài khoản; trên 6 tài khoản sẽ hiện scrollbar. Build `Polymind.slnx` **0 warning/0 error**; Chrome headless xác nhận `/leads` dark row/header dịu hơn, `/candidates/{id}` tên không còn underline, `/messages` 13 người nhận có scroll, public tunnel **200**. **Chưa commit.**
- **SỬA MODULE HỖ TRỢ VAY (Session 40, Codex).** Theo phản hồi user, `/loans` không còn tự đem toàn bộ ứng viên sang trang Hỗ trợ vay nữa; trang chỉ hiển thị các record thật trong bảng `loans`. Thêm nút **Thêm khoản vay** ở header để chọn ứng viên hiện có chưa có khoản vay và tạo hồ sơ vay. Bỏ trạng thái/label **Chưa vay** khỏi UI; trạng thái chọn chỉ còn **Đang vay** và **Đã giải ngân**; chi tiết ứng viên chưa có vay hiển thị “chưa có khoản vay” + nút Thêm khoản vay. Build `Polymind.slnx` **0 warning/0 error**; smoke `/loans` sau login **200**, có nút thêm, không còn chữ “Chưa vay”, DB demo 15 ứng viên/10 loans nên trang lấy theo loans; public tunnel **200**. **Chưa commit.**
- **GIẢM CHÓI AI DARK MODE + CHUYỂN THEME MƯỢT (Session 39, Codex).** Theo screenshot user, `/ai` ở chế độ tối bị chói vì CSS AI hard-code nền trắng. Đã thêm class `theme-dark/theme-light` vào `MudLayout`, thêm transition màu nền/chữ/viền/bóng toàn app, có `prefers-reduced-motion`, và override dark riêng cho `ai-shell`, tabs, chat window, upload/result card, bubble, input để nền chuyển sang xanh than dịu. Build `Polymind.slnx` **0 warning/0 error**; Chrome headless xác nhận `/ai` dark có nền chat tối, không error-boundary, không tràn ngang; public tunnel vẫn **200**. **Chưa commit.**
- **HƯỚNG DẪN THEO ROLE (Session 38, Codex).** Đã cá nhân hóa trang `/guide` theo role đăng nhập (Super Admin, Giám đốc, Trưởng phòng tuyển dụng, Tuyển dụng, Tư vấn viên, Hồ sơ, Visa, Kế toán, Đại lý/CTV). Giữ nguyên bố cục hiện tại: `PageHeader` + alert + `MudExpansionPanels`; chỉ đổi nội dung hướng dẫn, chip vai trò và copy bên trong. Đã smoke nhiều role, build `Polymind.slnx` **0 warning/0 error**, Chrome headless kiểm desktop/iPad/mobile **không tràn ngang**. **Chưa commit.**
- **LÀM ĐẸP TRỢ LÝ AI (Session 37, Codex).** Theo phản hồi user screenshot tab CV/ảnh bị xấu do `InputFile` native + tabs mặc định. Đã redesign `/ai`: bọc tool surface `ai-shell`, header hiện đại, tabs dạng pill/segmented, chat empty state mới, composer gọn; tab **Trích xuất CV / ảnh** thành upload zone custom (ẩn native Choose File), file chip có tên/dung lượng/nút bỏ chọn, result panel riêng. CSS mới trong `app.css` (`ai-tabs`, `ai-upload-zone`, `ai-extract-grid`, `ai-result-card`...). Bỏ `PanelClass` khỏi `MudTabs` vì MudBlazor 9.5 báo MUD0002. Build `Polymind.slnx` **0 warning/0 error**; local `/ai` sau login **200**, CSS local/public có class mới; public tunnel vẫn **200**. **Chưa commit.**
## ⏭️ VIỆC TIẾP THEO (baton — làm cái này trước)

**CẦN RESTART SERVER TRƯỚC KHI DUYỆT:** process web cũ PID 21600 đã bị dừng để build. Lệnh restart bị tool từ chối vì giới hạn usage, nên cần chạy lại: `$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://0.0.0.0:5177'; dotnet run --project src/Polymind.Web --no-launch-profile`. Sau đó mở `/finance`, `/agents`, `/reports`, `/jobs`, `/` để duyệt các sửa Session 42.

**TÀI CHÍNH / BẢNG / BÁO CÁO:** kiểm `/finance` không còn Hồ Thị Nga/Trần Minh Châu nếu họ vẫn ở bước Lead/Tư vấn/chưa tới **Đặt cọc** trong trang ứng viên. Dialog thêm khoản thu chỉ liệt kê ứng viên đã tới bước Đặt cọc. Kiểm bảng thi đua `/agents`: CTV hiển thị chip `CTV-<tên>` giống Lead, không gạch chân; màu bảng dark mode dịu hơn. Kiểm `/reports` biểu đồ “Tỉnh/thành: Lead và Tỷ lệ chuyển đổi”: rê vào điểm phải hiện tooltip chi tiết. Kiểm `/jobs`: chip Đức nhìn được trong dark mode; quốc gia mới thêm từ đơn hàng mới sẽ tự xuất hiện trong bộ lọc vì filter group theo `JobOrders.Country`.

**BẢNG / TIN NHẮN / CHI TIẾT ỨNG VIÊN:** mở các trang có bảng như `/leads`, `/candidates`, `/jobs`, `/finance`, `/loans`, `/reports` ở cả sáng/tối để duyệt mắt màu bảng mới. Màu dark đã giảm sọc xám chói; nếu muốn dịu hơn nữa thì chỉnh nhóm rule `.theme-dark .mud-table...` trong `wwwroot/app.css`. Vào chi tiết ứng viên có TVV/CTV để xác nhận tên không còn gạch chân nhưng vẫn bấm mở dialog. Vào `/messages` kiểm tra danh sách người nhận chỉ cao tối đa khoảng 6 tài khoản và có scrollbar khi nhiều hơn 6.

**HỖ TRỢ VAY:** mở `/loans` kiểm trang chỉ hiển thị hồ sơ vay đã tạo, không còn toàn bộ ứng viên. Bấm **Thêm khoản vay** để chọn ứng viên hiện có chưa có khoản vay; dialog chỉ còn tình trạng **Đang vay** / **Đã giải ngân**. Vào chi tiết một ứng viên chưa có vay để thấy nút **Thêm khoản vay** thay vì trạng thái “Chưa vay”.
**AI DARK MODE / CHUYỂN THEME:** mở `/ai`, bấm nút sáng/tối trên app bar để kiểm cảm giác chuyển màu. Khung AI ở dark mode đã đổi sang nền xanh than, giảm chói; toàn app có transition ~0.28s cho background/color/border/shadow. Nếu trình duyệt còn giữ CSS cũ, Ctrl+F5 hoặc mở lại link public.
**HƯỚNG DẪN THEO ROLE:** mở `/guide` bằng từng tài khoản role để duyệt mắt nội dung hiển thị đúng quyền. Đã kiểm bằng admin/recruiter/accountant/visa/agent và responsive 1440/820/390px không tràn ngang; nếu muốn sửa câu chữ chi tiết cho từng phòng ban thì chỉnh `Components/Pages/Guide/Guide.razor` trong các nhánh `BuildSections(role)`.
**TRỢ LÝ AI UI:** user mở `/ai` → tab Hỏi đáp có shell mới; click tab **Trích xuất CV / ảnh** phải thấy upload zone hiện đại, không còn nút native `Choose File / No file chosen`. Nếu trình duyệt còn cache CSS thì Ctrl+F5.

**LOGO POLYMIND:** đã đồng bộ ảnh mới vào asset web và cache-bust. Nếu máy nào vẫn thấy logo cũ, dùng Ctrl+F5 hoặc mở lại link public; nếu đổi logo tiếp thì thay `POLYMIND.png` rồi regenerate `wwwroot/img/logo-polymind.png` + `favicon.png`.

**SERVER ĐỐI TÁC ĐANG MỞ:** gửi link **https://cleaner-reporter-breakfast-korean.trycloudflare.com** cho đối tác; đăng nhập demo `admin@polymind.local` / `Admin@123`. Link Cloudflare Quick Tunnel đổi mỗi lần restart tunnel; nếu mất kết nối hoặc muốn đổi link thì chạy lại `scripts/demo-start.ps1`. Tắt demo: `scripts/demo-stop.ps1`.

**DUYỆT MẮT Session 34 (UX + AI + Tài chính tuần tự) — web đang chạy local `http://localhost:5177` và public tunnel ở trên, đăng nhập admin (`admin@polymind.local` / `Admin@123`):** (1) `/leads` → chip **`CTV-<tên>`** bấm ra dialog CTV, **rê chuột thấy chip nhấc lên/sáng** (báo bấm được); cột "Mã" hiện **`#XXXX`**. (2) `/candidates` → cột "Tư vấn viên" là **chip xanh `TVV-<tên>`** có hover, bấm ra dialog TVV. (3) `/ai` (giờ là **nút icon ✨ trên app bar**, cạnh nút sáng/tối — không còn trong menu trái) tab Hỏi-đáp → hỏi "ứng viên nào tiềm năng nhất?" → AI dùng **tên thật + số liệu**, không còn từ chối. (4) `/finance` tab "Tiến độ đóng tiền": chỉ **bước kế tiếp** có nút "Đánh dấu đã đóng", bước sau khóa "Chưa tới lượt" → **ép đóng tuần tự 1→4**; record lệch của Trần Minh Châu đã dọn (B4 về Chưa đóng). (5) Nút **sáng/tối** đã có sẵn. **Lưu ý:** chip TVV/CTV + nút stepper Tài chính là tương tác (MudDataGrid/MudTabs) → chỉ hiện đủ khi circuit kết nối (trình duyệt thật). AI gửi kèm ảnh chụp dữ liệu mỗi câu hỏi → tốn token; DB lớn thì giảm `Take(100)`. **Khi user OK → commit gộp** Session 27→34.



**DUYỆT MẮT Session 32 (5 yêu cầu mới) — web hiện đã chạy tại `http://localhost:5177`; user đăng nhập admin (`admin@polymind.local` / `Admin@123`) kiểm:** (1) `/candidates/{id}` → card **"Người phụ trách ứng viên"** trên cùng, bấm tên **TVV**/**CTV** ra dialog liên lạc; cuộn xuống cột trái có card **"Hỗ trợ vay vốn"** (bấm "Tạo/Cập nhật hồ sơ vay"); nút **"Phân tích AI"** ở đầu trang → dialog Gemini phân tích hồ sơ. (2) `/agents` chỉ còn **Top 3 đại lý + Top 5 CTV**. (3) `/admin` tab Tài khoản → nút **"Sửa"** (super admin) đổi họ tên/email/mật khẩu. (4) `/loans` (nav **"Hỗ trợ vay"**): 4 thẻ tổng quan + lọc tình trạng + bảng ứng viên. (5) `/ai` (nav **"Trợ lý AI"**): tab **Hỏi-đáp** (gõ câu hỏi XKLĐ) + tab **Trích xuất CV** (upload ảnh/PDF). **Lưu ý/quyết định cần user chốt:** (a) **Model Gemini = `gemini-2.5-flash`** vì key free user cấp **không có quota cho `gemini-2.0-flash` (limit 0)** — nếu user xin được key xịn/đổi project thì sửa `Ai:Gemini` trong `appsettings.Development.json`. (b) **Key đang để trong `appsettings.Development.json` (file ĐƯỢC track git)** → **KHÔNG push public** kẻo lộ key; production đọc qua env `Ai__Gemini__ApiKey`. (c) Card TVV/CTV + card vay + dialog AI + nút Sửa tài khoản là **tương tác (MudDialog/MudDataGrid)** → chỉ hiện đủ khi circuit kết nối (mở trình duyệt thật). (d) Module vay hiện 1 hồ sơ/ứng viên (upsert); muốn nhiều khoản vay/lịch sử thì mở rộng sau. **Khi user OK → commit gộp** Session 27→32.

**DUYỆT MẮT Session 31 (Báo cáo + Tài chính 4 bước + Giới tính) — user mở `:5177` đăng nhập admin kiểm:** `/reports` → đổi **"Khoảng thời gian"** (Tháng/Quý/Năm/Tùy chọn) thấy số liệu + biểu đồ cập nhật; cuộn xuống mục **"Biểu đồ trực quan"** xem đủ **7 loại** (tròn/cột/đường/miền/thanh ngang/phân tán/kết hợp). `/finance` tab **"Tiến độ đóng tiền"**: mỗi ứng viên có stepper 4 bước + % hoàn tất; thử **"Tạo lịch đóng tiền"** cho ứng viên chưa có lịch + **"Đánh dấu đã đóng"** 1 bước. `/candidates` + `/leads` có cột **"Giới tính" (Nam/Nữ)**. Lưu ý cột Giới tính (MudDataGrid) + tab Tài chính (MudTabs) chỉ hiện khi circuit kết nối — mở bằng trình duyệt thật. **Gap/quyết định cần user chốt:** (a) **chia 20/30/30/20 + tên B2/B3** ("Phí dịch vụ"/"Phí trước xuất cảnh") do mình đặt theo yêu cầu — đổi tỉ lệ thì sửa `Display/PaymentSchedule.cs`, đổi tên thì sửa `Labels.Vi(PaymentStage)`. (b) Bộ lọc thời gian KHÔNG áp cho biểu đồ "6 tháng gần nhất" theo cửa sổ trượt (vẫn lấy 6 tháng cuối) — nếu user muốn biểu đồ tháng bám đúng khoảng lọc thì chỉnh vòng lặp tháng trong `Reports.LoadData()`. (c) Stage payment seed cộng thêm vào KPI "Tổng đã thu" (song song payment demo cũ) → nếu thấy số to bất thường là do chồng, có thể TRUNCATE payments rồi để seed lại. **Khi user OK → commit gộp** Session 27→31.

**DUYỆT MẮT Session 30 (Tư vấn viên + CTV-nguồn) — user mở `:5177` đăng nhập admin kiểm:** `/candidates` (cột **"Tư vấn viên"** gạch chân, bấm ra dialog SĐT/email), `/leads` (cột Nguồn vài lead hiện **`CTV-<tên>`**), **Thêm Lead** → chọn Nguồn="Giới thiệu" hiện ô autocomplete CTV + dropdown "Tư vấn viên"; lưu xong cột Nguồn ra `CTV-<tên>`. Thử **đăng nhập 1 TVV** `tuvan1@polymind.local` / `Admin@123`. `/admin` có bảng vai trò "Tư vấn viên" (5 tài khoản). Cột "Tư vấn viên" trong DataGrid nằm `MudHidden` → chỉ hiện khi circuit kết nối (trình duyệt thật). **Gap/đề xuất nếu user muốn tiếp:** (a) CandidateDetail chưa hiển thị TVV (chỉ có ở list + dialog) — thêm 1 dòng nếu user cần. (b) Lead cũ assigned=admin (không phải consultant) → có thể chạy 1 lần gán lại TVV cho lead cũ nếu muốn đồng nhất. (c) cho TVV tự nhắn tin/cổng riêng nếu cần. **Khi user OK → commit gộp** Session 27→30.

**SAU CUỘC HỌP (Session 29) — user duyệt mắt trong trình duyệt rồi báo gap:** vào `:5177` đăng nhập admin, kiểm: `/agents` (2 bảng thi đua + bấm tên CTV ra dialog liên lạc), `/jobs` (chip cờ lọc nước + số tiền lấp lánh + bấm đơn → card Đãi ngộ&Thưởng), `/candidates` (cột CTV gạch chân bấm được + Quốc gia cờ + Nghề + sort), `/messages` (gửi thử tin). Lưu ý 2 bảng thi đua nằm trong `MudHidden` nên chỉ hiện khi circuit kết nối (mở bằng trình duyệt thật, không phải prerender). **Gap còn lại đã báo user:** (a) CTV/đại lý chưa có tài khoản đăng nhập riêng → nhắn tin hiện chỉ giữa **8 user role** (giám đốc/đại lý = role `director`/`agent`); muốn CTV tự đăng nhập & nhắn cần tạo tài khoản cho từng CTV (việc lớn). (b) "doanh số" bảng thi đua = tổng **tiền đã thu (Payment Paid)** của ứng viên thuộc đại lý/CTV — nếu họp muốn công thức khác (theo hoa hồng/đầu người) thì chỉnh `Agents.razor Load()`. (c) realtime nhắn tin chưa có (phải refresh/đổi người để thấy tin mới). Khi user OK → **commit gộp** Session 27→29.

**PASS 2 UI (Session 28) — đã xong trang chi tiết + bảng mobile; có thể tiếp PASS 3 nếu user muốn:** rà nốt các bảng `MudSimpleTable`/`MudDataGrid` chưa có thẻ mobile ở các trang còn lại (Reports, Notifications, MyCommissions...) — đa số đã xử lý ở Phase 3, nhưng nên QA mắt 390px. **QA pixel 390/820/1440** (nợ lâu nay) vẫn cần con người lướt theo `docs/08-qa-mobile-checklist.md`. Cân nhắc **commit gộp** Session 27+28 khi user duyệt giao diện.

**GIAI ĐOẠN TEST (hiện tại): laptop + Cloudflare Tunnel — user tự vận hành hằng ngày.**
- Mỗi ngày muốn cho 2 đối tác test: chạy **`scripts/demo-start.ps1`** (double-click hoặc PowerShell) → nó in + copy **link `*.trycloudflare.com`** → gửi đối tác. Tắt: `scripts/demo-stop.ps1`.
- Link đổi mỗi lần restart tunnel → gửi lại link mới. Giữ laptop cắm điện + không sleep khi đang test. Sửa code xong: chạy lại `demo-start.ps1` (hoặc `dotnet watch`).
- **Việc của AI session sau (nếu user yêu cầu):** cải tiến demo (vd link cố định nếu user mua domain rẻ; hoặc `dotnet watch` để hot-reload khi sửa theo góp ý đối tác). KHÔNG cần đụng deploy production tới khi web chốt xong.

**Khi web CHỐT XONG → dùng thật (để sau):** chọn Oracle Always Free (free 24/7, cần thẻ xác minh) hoặc VPS VN trả phí (data trong nước, hóa đơn VAT — trình sếp). Code production đã sẵn sàng từ Session 25 (seeding an toàn, profile Caddy/DuckDNS). Chi tiết bên dưới:

**HARDENING DEPLOY phần code đã XONG (Session 25). Khi deploy thật cần MÁY/MẠNG (không tự động được trên máy dev):**
1. **Tạo `.env.production` thật** từ `.env.production.example`: đổi MỌI secret (`POSTGRES_PASSWORD`, `MINIO_ROOT_PASSWORD`, `JWT_KEY` >= 32 ký tự), đặt `SUPERADMIN_EMAIL` + `SUPERADMIN_PASSWORD` mạnh (đây là tài khoản đăng nhập duy nhất ở production — KHÔNG còn `Admin@123`), đặt `DOMAIN`=`polymindolms.duckdns.org` (fallback `polymindolmsvn`/`polymindolms2026`) + `ACME_EMAIL`.
2. **Kiểm tra mạng (gate bắt buộc):** so WAN IP của router với public IP ngoài Internet (vd whatismyip). Nếu KHÁC nhau → CGNAT → DuckDNS public KHÔNG đủ, chuyển fallback tunnel (Cloudflare Tunnel). Nếu GIỐNG → port-forward 80/443 về máy chạy Docker, trỏ bản ghi A DuckDNS về public IP (IP động thì bật `--profile duckdns`).
3. **Chạy production:** `docker compose --env-file .env.production -f docker-compose.production.yml --profile caddy up -d --build`. Đợi Caddy xin cert Let's Encrypt (log `docker logs polymind-prod-caddy`), test `https://<DOMAIN>/health`, chạy `scripts/smoke-test.ps1` qua domain thật, đăng nhập bằng super admin vừa tạo.
4. **Backup hằng ngày:** lên lịch `scripts/backup.ps1 -EnvFile .env.production`.
5. **QA giao diện bằng mắt** (nợ từ Session 23, cần con người): 390/820/1440px theo checklist Phase 5.
- **Lưu ý kỹ thuật Phase 2+3+4 (cho người sau):**
  - Component dùng chung ở `Components/Shared/`: `StatCard.razor` (param `Title/Value/Icon/Color/Caption`, tự render `MudItem xs=6 sm=4 md=3 lg=2`), `PageHeader.razor` (param `Title/Subtitle` + slot `Actions`). Đã import sẵn trong `_Imports.razor` (`Polymind.Web.Components.Shared`).
  - Pattern bảng→thẻ: `<MudHidden Breakpoint="Breakpoint.SmAndDown">` bọc DataGrid (chỉ ≥md), `<MudHidden Breakpoint="Breakpoint.SmAndDown" Invert="true">` bọc danh sách thẻ (chỉ ≤sm). Mỗi trang tự render thẻ riêng (cột mỗi entity khác nhau).
  - CSS mới trong `wwwroot/app.css`: `.stat-value` (cỡ chữ KPI co theo breakpoint), `.mobile-card` (viền+bo góc thẻ mobile), `.table-scroll-x` (đã có sẵn — cuộn ngang bảng rộng).
  - **Phase 4:** dialog defaults đặt 1 chỗ ở `MainLayout.razor` (`<MudDialogProvider FullWidth MaxWidth=Medium CloseButton CloseOnEscapeKey>`) → áp dụng cho mọi `DialogService.ShowAsync`. MudTabs (9.5) tự hiện nút cuộn khi tab tràn → không cần thêm attribute (tránh bẫy MUD0002). Timeline dùng `TimelinePosition.Start` + form field `xs=12` đã responsive sẵn.
  - **Phase 1→5 + Phase H đều CHƯA commit.** Cân nhắc commit trước khi deploy.

**Việc khác (chưa ưu tiên — sau khi xong đại tu giao diện):** deploy production (xem memory deploy-plan). Backlog Phase H cũ:
1. **Mở rộng REST API** theo mẫu đã có (`src/Polymind.Web/Api/`): thêm CRUD/read cho payments, expenses, agents, visas, flights, reports... Mẫu: dùng `ApiAuth.Bearer("<resource>:<action>")` cho từng endpoint, DTO trong `ApiContracts.cs`, map trong `Program.cs`. (Đã có sẵn: auth, leads CRUD, candidates/job-orders read.)
2. **Hạng mục Phase H còn lại** (phụ thuộc provider/thiết kế riêng — chốt scope trước khi code): OCR CCCD/Passport, chữ ký số, Facebook/TikTok/Google/Zalo lead intake (giờ đã có nền REST + webhook), AI chatbot/dự đoán (cần Claude API key), mobile app, BI/Data Warehouse.
3. **Xác minh nốt nhắc hoa hồng (từ Session 21):** demo data 8 hoa hồng đều `Pending` nên nhắc `CommissionPayment` chưa hiện. Để thấy: vào `/agents/{id}` duyệt 1 hoa hồng (Approved) rồi "Quét nhắc việc" ở `/notifications`. Logic đã verify qua build + đối xứng code.

**Lưu ý REST API (Phase H) cho người sau:**
- Code API ở `src/Polymind.Web/Api/`: `JwtTokenService` (sinh JWT + claim `permission`/role), `ApiContracts` (DTO + helper `ApiAuth.Bearer`), `AuthEndpoints`, `LeadsEndpoints`, `ResourceEndpoints`. Map ở cuối `Program.cs`.
- **Khóa JWT:** dev đặt trong `appsettings.Development.json` (`Jwt:Key`); **production BẮT BUỘC** env `JWT_KEY` (đã thêm vào `docker-compose.production.yml` + `.env.production.example`) — Program.cs throw nếu thiếu key ở production.
- JWT chạy song song Cookie: endpoint API phải `.RequireAuthorization(ApiAuth.Bearer("res:action"))` để ép scheme Bearer (trả 401, không redirect cookie). Đừng đổi default scheme (Blazor cần Cookie).
- Swashbuckle 10.2.3 dùng **Microsoft.OpenApi 2.x**: types ở namespace `Microsoft.OpenApi` (KHÔNG phải `.Models`); security requirement dùng `OpenApiSecuritySchemeReference(id, doc, null)` qua overload `AddSecurityRequirement(doc => ...)`; value của requirement là `List<string>` (không phải mảng).
- JSON API set `JsonStringEnumConverter` toàn cục (enum đọc/ghi dạng chuỗi). PowerShell 5.1 test API tiếng Việt: phải gửi body bằng `[Text.Encoding]::UTF8.GetBytes(...)` + `charset=utf-8`, nếu không bị 400/mojibake.
2. Nếu chưa chốt Phase H, việc thực tế nên làm ngay là **deploy rehearsal**: tạo `.env.production`, đặt chứng chỉ thật vào `deploy/nginx/certs/`, chạy `docker compose --env-file .env.production -f docker-compose.production.yml up -d --build`, test `/health`, chạy `scripts/smoke-test.ps1` qua domain thật.
3. Commit/push các thay đổi Phase F→G lên GitHub nếu muốn đồng bộ remote trước khi triển khai.
4. Nếu chỉ cần chạy demo local: `docker compose up -d`, rồi trong terminal chạy DLL build sẵn từ `src/Polymind.Web`: `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet bin\Debug\net10.0\Polymind.Web.dll --urls http://localhost:5177`.

**Quyết định deploy nội bộ đã chốt với user (Session 24, chỉ ghi kế hoạch — chưa code):**
- Hosting: **máy Windows công ty hiện có**, uptime kỳ vọng **giờ hành chính**.
- Access model: **link public** để nhân viên chỉ cần mở URL và login, không bắt cài VPN/Tailscale giai đoạn đầu.
- DNS free: **DuckDNS**, ưu tiên `polymindolms.duckdns.org`; fallback tên nếu bận: `polymindolmsvn.duckdns.org`, `polymindolms2026.duckdns.org`.
- Dữ liệu production: **DB sạch**, không seed demo data.
- Security gate bắt buộc trước khi public: không để tài khoản mẫu/mật khẩu `Admin@123` tồn tại/dùng được ở production; tạo user thật, mật khẩu mạnh, backup trước khi mở port.
- Network gate bắt buộc: kiểm tra router WAN IP so với public IP ngoài Internet. Nếu khác nhau/CGNAT hoặc không port-forward được 80/443 thì DuckDNS public không đủ; chuyển fallback tunnel.
- HTTPS/proxy đề xuất: thêm **Caddy** cho profile DuckDNS để tự xin/gia hạn Let's Encrypt cert; giữ Nginx hiện tại (`deploy/nginx/default.conf`) làm fallback/legacy.
- Fallback: Cloudflare Quick Tunnel chỉ dùng test/dev; ngrok free chỉ dùng pilot ngắn vì có giới hạn data/request và interstitial; nếu muốn ổn định hơn mà không mở port được thì cân nhắc mua domain rẻ + Cloudflare Tunnel/Access.

**Lưu ý Đợt 5 cho người sau:**
- `AgentScope` (scoped, `Identity/AgentScope.cs`) là nguồn sự thật cho data-scope đại lý: `GetAsync()` trả `(IsAgentOnly, AgentId)`, cache trong 1 request. "Agent-only" = có role `agent` và KHÔNG kèm role nội bộ nào. Dùng nó ở mọi trang dùng chung cần bó hẹp.
- Đại lý gắn user qua `Agent.UserId` (seed ở `SeedExtrasAsync`, idempotent — chỉ gắn nếu chưa có agent nào trỏ tới user đó). Demo: `agent@` ↔ AG-000001.
- Redirect đại lý khỏi `/` và `/reports` đặt trong `OnInitializedAsync` trước khi load DB (defense-in-depth), cộng ẩn menu trong `NavMenu` (`_isAgentOnly`). Nếu thêm trang nhạy cảm mới, nhớ cả 2 lớp.
- Permission seed cho `agent` đã được reconcile ở Session 18: hiện chỉ còn `candidates:read`, `commissions:read`, `notifications:read`; vẫn giữ redirect/ẩn menu như lớp bảo vệ phụ.

**Lưu ý Đợt 4 cho người sau:**
- Trang `/notifications`: class page `Notifications` **trùng tên** property inject → đã đổi tên service inject thành `NotificationSvc` (đừng đặt lại thành `Notifications`).
- Reminder hiện sinh bằng Hangfire recurring job và nút "Quét nhắc việc" ở `/notifications`; topbar bell chỉ đếm unread. Idempotent theo `(UserId, Type, ReferenceId, Channel)` và nhắm recipient theo người phụ trách/fallback role.
- Export CSV là **minimal-API endpoint** trong `Program.cs` (`MapCsvExportEndpoints`), gated `.RequireAuthorization("reports:read")` qua policy provider động. Dùng BOM UTF-8 (`Encoding.UTF8.GetPreamble()`) để Excel đọc đúng tiếng Việt. Không ghi file ra repo — stream `Results.File`.
- MudIcon render ra SVG path (không có tên icon trong HTML) và MudMenu render lazy → grep HTML tìm tên icon/menu sẽ "miss"; verify bằng HTTP 200 + vắng `blazor-error-boundary` + query DB.

**Nợ nhỏ phát hiện ở Session 9:** role `agent` từng thấy `/reports` hơi rộng; Session 18 đã gỡ `reports:read`/`dashboard:read` bằng seeder reconcile. Vẫn cần dùng `AgentScope` cho mọi trang dữ liệu dùng chung mới.

**LƯU Ý khi tạo trang mới (giữ nguyên):** KHÔNG thêm `@rendermode`; đặt page trong folder + đừng đặt tên class trùng entity. MudBlazor 9.5 — tránh `MudChart` (API đổi, cảnh báo `MUD0002`, attribute bị nuốt im lặng); dùng `MudProgressLinear` + `MudSimpleTable`. **Seed mở rộng:** `DemoDataSeeder.SeedExtrasAsync` chạy idempotent theo từng bảng kể cả khi DB đã có Lead (bù Visa/Flight/Commission mà không cần xóa DB).

## 🚧 BLOCKERS / NỢ KỸ THUẬT

- (chưa có blocker)
- Nợ triển khai thật: cần `.env.production` với secret thật, chứng chỉ TLS thật (ưu tiên Caddy tự cấp Let's Encrypt cho DuckDNS; Nginx manual cert vẫn là fallback), SMTP/provider SMS/Zalo thật nếu bật gửi ngoài InApp.
- **CẢNH BÁO DEPLOY PUBLIC:** Không public production nếu còn user mẫu/mật khẩu `Admin@123`. DuckDNS/No-IP chỉ giải quyết DNS động; vẫn cần public IP hoặc port-forward 80/443 hoạt động. Nếu mạng công ty bị CGNAT/không mở port được thì không cố DuckDNS, chuyển fallback tunnel.
- ~~Module placeholder chờ làm thật~~ — ĐÃ XONG HẾT (finance/agents/visa/reports đều có trang thật).

---

## 📜 NHẬT KÝ SESSION (mới nhất ở trên)

### [2026-06-29] Session 42 — Codex — Lọc Finance từ bước Đặt cọc, làm dịu bảng thi đua/dialog và sửa chart/quốc gia
- **Làm được:** thêm `FinanceEligibility` để chỉ ứng viên có assignment mới nhất từ `WorkflowStep.Deposit` trở lên mới xuất hiện/tính vào Finance, dialog thêm khoản thu, KPI tài chính ở Tổng quan/Báo cáo; đổi CTV trong bảng thi đua sang chip `CTV-<tên>` giống Lead; mở rộng CSS bảng cho `MudSimpleTable`/dialog thống kê; thêm tooltip SVG cho ScatterChart tỉnh/thành; đổi màu Đức khỏi `Color.Dark`, fallback quốc gia lạ sang `Color.Primary`, sắp xếp chip quốc gia ổn định và giữ filter động theo DB.
- **File thay đổi chính:** `src/Polymind.Web/Display/FinanceEligibility.cs`, `Components/Pages/Finance/{Finance,PaymentDialog}.razor`, `Components/Pages/{Home,Reports,Agents,JobOrders}/`, `Components/Shared/ScatterChart.razor`, `Display/CountryDisplay.cs`, `wwwroot/app.css`, `WORKLOG.md`.
- **Đã test:** `dotnet build Polymind.slnx` = **0 warning / 0 error** sau khi dừng `Polymind.Web` PID 21600 đang khóa exe.
- **Lưu ý/cảnh báo cho người sau:** server local chưa restart lại được trong session này vì lệnh `Start-Process dotnet run` cần escalation nhưng tool bị từ chối do giới hạn usage. Cần chạy lại server thủ công trước khi smoke UI/public tunnel.
### [2026-06-29] Session 41 — Codex — Giảm chói bảng toàn web, bỏ underline TVV/CTV và thêm cuộn tin nhắn
- **Làm được:** thêm theme màu bảng toàn hệ thống để các bảng ở light/dark bớt chói, nhất là sọc hàng trong dark mode; bỏ gạch chân class `.person-name` trên chi tiết ứng viên; thêm vùng cuộn cho danh sách người nhận ở `/messages`, giới hạn chiều cao khoảng 6 tài khoản.
- **File thay đổi chính:** `src/Polymind.Web/wwwroot/app.css`, `src/Polymind.Web/Components/Pages/Messages/Messages.razor`, `WORKLOG.md`. Lưu ý `CandidateDetail.razor` vẫn có thay đổi từ Session 40; việc bỏ underline nằm trong CSS global `.person-name`.
- **Đã test:** `dotnet build Polymind.slnx` = **0 warning / 0 error** sau khi dừng process web khóa DLL và restart `:5177`; Chrome headless dark mode `/leads` xác nhận header `#0f172a`, row `#182234`, không tràn ngang; `/candidates/{id}` xác nhận `text-decoration: none`; `/messages` có 13 tài khoản, vùng cuộn `max-height: 432px`, `overflow-y: auto`, `scrollHeight > clientHeight`; public tunnel `/login` = **200**.
- **Lưu ý/cảnh báo cho người sau:** CSS bảng dùng selector global `.theme-light/.theme-dark .mud-table...`; nếu một bảng đặc thù cần màu riêng thì override cục bộ sau nhóm rule này. Scroll tin nhắn cố định 432px tương ứng khoảng 6 item theo chiều cao UI hiện tại.
### [2026-06-29] Session 40 — Codex — Sửa module Hỗ trợ vay chỉ hiển thị khoản vay thật
- **Làm được:** đổi `/loans` từ danh sách toàn bộ ứng viên + trạng thái giả `Chưa vay` sang danh sách các khoản vay thật trong bảng `loans`; thêm nút `Thêm khoản vay` mở dialog chọn ứng viên hiện có chưa có loan; dialog và filter chỉ còn `Đang vay` / `Đã giải ngân`; chi tiết ứng viên chưa có loan không hiển thị chip `Chưa vay` nữa.
- **File thay đổi chính:** `src/Polymind.Web/Components/Pages/Loans/{Loans,LoanDialog}.razor`, `src/Polymind.Web/Components/Pages/Candidates/CandidateDetail.razor`, `src/Polymind.Web/Display/Labels.cs`, `src/Polymind.Domain/{Entities/Loan.cs,Enums/Enums.cs}`, `WORKLOG.md`.
- **Đã test:** `dotnet build Polymind.slnx` = **0 warning / 0 error** sau khi dừng process web khóa DLL và restart `:5177`; smoke admin `/loans` = **200**, có `Thêm khoản vay`, không có `Chưa vay`, không bị login lại, không `blazor-error-boundary`; DB demo `candidates=15`, `loans=10`; public tunnel `/login` = **200**.
- **Lưu ý/cảnh báo cho người sau:** enum `LoanStatus.NotBorrowed` được giữ như legacy để không phá migration/dữ liệu cũ, nhưng UI không cho chọn và label fallback map về `Đang vay`. Nếu sau này muốn xóa hẳn enum thì cần migration/data cleanup riêng.
### [2026-06-29] Session 39 — Codex — Giảm chói AI dark mode và làm mượt chuyển theme
- **Làm được:** thêm class `theme-dark/theme-light` trên `MudLayout`; thêm transition màu nền/chữ/viền/bóng cho các surface chính; chỉnh riêng dark-mode của trang `/ai` để chat/upload/result không còn nền trắng gắt.
- **File thay đổi chính:** `src/Polymind.Web/Components/Layout/MainLayout.razor`, `src/Polymind.Web/wwwroot/app.css`, `WORKLOG.md`.
- **Đã test:** `dotnet build Polymind.slnx` = **0 warning / 0 error** sau khi dừng process web khóa file và restart `:5177`; Chrome headless login admin vào `/ai` dark mode xác nhận `theme-dark`, nền chat là `#111827/#0f172a`, không `blazor-error-boundary`, `scrollWidth == clientWidth`; local `app.css` có rule mới; public tunnel `/login` = **200**.
- **Lưu ý/cảnh báo cho người sau:** CSS `app.css` được load qua `@Assets` nên đổi nội dung sẽ tự cache-bust. Transition có nhánh `prefers-reduced-motion: reduce` để không gây khó chịu cho người dùng tắt animation.
### [2026-06-29] Session 38 — Codex — Hướng dẫn sử dụng theo từng role
- **Làm được:** đổi trang `/guide` sang hướng dẫn cá nhân hóa theo role đang đăng nhập; mỗi role có chip/intro riêng và các nhóm bước thao tác riêng, nhưng vẫn giữ nguyên bố cục PageHeader + alert + accordion hiện tại.
- **File thay đổi chính:** `src/Polymind.Web/Components/Pages/Guide/Guide.razor`, `WORKLOG.md`.
- **Đã test:** `dotnet build Polymind.slnx` = **0 warning / 0 error**; smoke `/guide` sau login với admin/recruiter/accountant/visa/agent đều đúng role, đúng section, không `blazor-error-boundary`; Chrome headless đo desktop 1440, iPad 820, mobile 390 đều `scrollWidth == clientWidth`, không tràn ngang.
- **Lưu ý/cảnh báo cho người sau:** phần này cố ý không thêm layout mới/card mới để không ảnh hưởng bố cục tablet/mobile. Nếu thêm role mới, cập nhật `_rolePriority`, `BuildProfile(role)` và `BuildSections(role)` cùng lúc.
### [2026-06-29] Session 37 — Codex — Làm đẹp UI Trợ lý AI
- **Làm được:** redesign trang `/ai` theo style POLYMIND hiện tại: `ai-shell` card, tab pill/segmented, chat empty state mới, composer gọn; tab Trích xuất CV/ảnh đổi từ native `Choose File` sang upload zone custom + file chip + result panel.
- **File thay đổi chính:** `src/Polymind.Web/Components/Pages/Ai/AiAssistant.razor`, `src/Polymind.Web/wwwroot/app.css`, `WORKLOG.md`.
- **Đã test:** `dotnet build Polymind.slnx` = **0 warning / 0 error** sau khi dừng app giữ lock; restart app `:5177`; local `/ai` sau login = **200**, có `ai-shell`; local/public `app.css` có `ai-upload-zone`, `ai-tabs`; public login tunnel = **200**.
- **Lưu ý/cảnh báo cho người sau:** MudBlazor 9.5 không nhận `PanelClass` trên `MudTabs` (MUD0002), không thêm lại. Tab thứ hai của MudTabs là tương tác nên grep HTML prerender có thể chưa thấy upload zone cho tới khi click trong trình duyệt thật.


