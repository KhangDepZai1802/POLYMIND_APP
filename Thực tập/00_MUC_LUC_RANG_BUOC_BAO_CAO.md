# MỤC LỤC RÀNG BUỘC — BÁO CÁO THỰC TẬP TỐT NGHIỆP

> Đây là **kế hoạch bắt buộc** để viết `Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx`.
> Mọi mục trong file này phải xuất hiện trong DOCX, đúng thứ tự, đúng cấp tiêu đề.
> Số trang là **ước lượng**, phải đối chiếu lại bằng kết quả render thực tế (mục 10.4 của prompt).

---

## 0. RÀNG BUỘC ĐỊNH DẠNG BẮT BUỘC (theo mẫu trường + quy định trong `TTTN_Mau9`)

| Hạng mục | Quy định |
|---|---|
| Font | Times New Roman toàn văn |
| Khổ giấy | A4 (21 × 29,7 cm), in 1 mặt, đóng bìa kiếng |
| Lề trái | 3 cm |
| Lề phải | 2 cm |
| Lề trên | 2 cm |
| Lề dưới | 2 cm |
| Giãn dòng | 1,3 – 1,5 lines (báo cáo dùng **1,4**) |
| Cỡ chữ thân bài | 13 pt, canh đều (justify), thụt đầu dòng 1,27 cm |
| Số trang | Cuối trang, **canh giữa**. Trang 1 bắt đầu từ **Lời mở đầu**. Các trang trước đó đánh **i, ii, iii…**; **trang bìa và trang lót bìa KHÔNG đánh số** |
| Mục lục | Đánh **tự động** (trường TOC của Word, cập nhật được bằng F9) |
| Hình / bảng | Đánh chỉ mục **theo chương** (Hình 2.1, Bảng 2.1…), bắt buộc có tiêu đề (caption) |
| Công thức | Đánh chỉ số theo chương, đặt **bên phải** |
| Nộp | 1 file PDF duy nhất chứa toàn bộ nội dung, gửi cho 2 cán bộ hướng dẫn |

**Cấp tiêu đề (tối đa 4 cấp):**

| Cấp | Ví dụ | Định dạng |
|---|---|---|
| Cấp 1 | `CHƯƠNG 2.` | size **16**, **in đậm**, **CHỮ HOA** |
| Cấp 2 | `2.1.` | size **14**, **in đậm**, **CHỮ HOA** |
| Cấp 3 | `2.1.1.` | size **13**, **in đậm** |
| Cấp 4 | `2.1.1.1.` | size **13**, *in nghiêng* |

**Ràng buộc kỹ thuật khi sinh DOCX:**

- Phải dùng **style Heading 1/2/3/4 thật**, không giả lập bằng bold + size thủ công.
- Giữ nguyên section/header/footer/trường Word của file mẫu; tạo section riêng cho 3 vùng đánh số trang (bìa → không số; phần đầu → La Mã; từ Lời mở đầu → Ả Rập bắt đầu lại từ 1).
- Ghi chú hình/bảng chưa có ảnh thật phải là **khung placeholder có viền**, kích thước gần với hình dự kiến, nội dung ghi chú **in đậm**.
- Không để sót cú pháp Markdown, đường dẫn tạm, marker debug trong DOCX.
- Không lộ secret/token/API key/connection string/mật khẩu thật trong văn bản lẫn hình.

---

## 0.1. THÔNG TIN ĐỊNH DANH (do sinh viên cung cấp — đã có)

| Trường | Giá trị |
|---|---|
| Họ và tên sinh viên | Lê Duy Khang |
| Công ty thực tập | Công ty TNHH Khoa học Kỹ thuật Vạn Thịnh |
| Tập đoàn chủ quản | Thống Đạt Group |
| Văn phòng trung tâm điều hành | TP. Hồ Chí Minh |
| Chuyên gia hướng dẫn | Trương Chí Cường |
| Giảng viên hướng dẫn | Nguyễn Hòa |
| Trường | Trường Đại học Sài Gòn — Khoa Công nghệ Thông tin |
| Đề tài / dự án | POLYMIND OLMS — Hệ thống quản lý xuất khẩu lao động |

**Còn thiếu (dùng placeholder trong DOCX):** MSSV, lớp, ngành; địa chỉ/điện thoại/email liên hệ chính thức của công ty; tháng–năm nộp báo cáo; ngày bắt đầu – kết thúc thực tập; chức danh chuyên gia hướng dẫn.

---

## 0.2. PHÂN BỔ TRANG TỔNG THỂ

| Khối | Kiểu đánh số | Dự kiến | **Thực tế đã render** |
|---|---|---:|---:|
| Trang bìa + trang lót bìa | không đánh số | 2 | **2** |
| Phần đầu còn lại (nhận xét → danh mục hình) | La Mã i–x | 8 | **10** |
| Lời mở đầu → Phụ lục | Ả Rập, bắt đầu lại từ 1 | 90 | **86** |
| **TỔNG SỐ TRANG VẬT LÝ TOÀN FILE** | | **100** | **98** |

**Đã đối chiếu bằng bản render thực tế** (Microsoft Word → PDF, sau khi cập nhật toàn bộ trường
TOC/SEQ/PAGE): **98 trang vật lý**, nằm trong khoảng bắt buộc 97–103.

- Tổng số trang vật lý toàn file: **98**
- Số trang được đánh số Ả Rập, bắt đầu từ Lời mở đầu: **86** (trang 1 → 86)
- Số trang phần nội dung chính (Lời mở đầu + 4 chương): **77**
- Số trang phần đầu đánh số La Mã: **10** (i → x)
- Số trang không đánh số (bìa, lót bìa): **2**

---

# PHẦN A — PHẦN ĐẦU BÁO CÁO (10 trang, đánh số La Mã)

## TASK A.1 — Trang bìa
- Số trang dự kiến: 1 · Khoảng trang: (không đánh số)
- Nội dung bắt buộc: quốc hiệu đơn vị chủ quản (ỦY BAN NHÂN DÂN TP HỒ CHÍ MINH / TRƯỜNG ĐẠI HỌC SÀI GÒN / KHOA CÔNG NGHỆ THÔNG TIN), logo trường (ảnh `image1.png` có sẵn trong file mẫu — giữ nguyên), họ tên sinh viên, tiêu đề "BÁO CÁO THỰC TẬP TỐT NGHIỆP", tên đề tài, công ty thực tập, chuyên gia hướng dẫn, giảng viên hướng dẫn, dòng "TP. Hồ Chí Minh, tháng … năm 2026".
- Tài liệu đối chiếu: `Thực tập/TTTN_Mau9_SINHVIENtrinhbaybaocaothuctaptotnghiep.docx` (đoạn P0–P26).
- Hình/bảng: giữ logo trường của mẫu.
- Thông tin cần bổ sung: MSSV, lớp, tháng nộp.

## TASK A.2 — Trang lót bìa
- Số trang: 1 · (không đánh số)
- Nội dung: lặp lại trang bìa, bổ sung ngành đào tạo và niên khóa.

## TASK A.3 — Nhận xét của chuyên gia doanh nghiệp
- Số trang: 1 · Trang i
- Nội dung: tiêu đề theo mẫu + vùng kẻ dòng trống để chuyên gia viết tay, dòng ký tên "Chuyên gia hướng dẫn — Trương Chí Cường".
- **Tuyệt đối không viết hộ nội dung nhận xét, không tạo điểm số, không tạo chữ ký.**

## TASK A.4 — Nhận xét của giảng viên hướng dẫn
- Số trang: 1 · Trang ii
- Nội dung: tương tự A.3, ký tên "Giảng viên hướng dẫn — Nguyễn Hòa". Không viết hộ.

## TASK A.5 — Mục lục tự động
- Số trang: 3 · Trang iii–v
- Nội dung: trường TOC Word 3 cấp, cập nhật được bằng F9.

## TASK A.6 — Danh mục chữ viết tắt
- Số trang: 1 · Trang vi
- Nội dung: bảng 2 cột (viết tắt / diễn giải): OLMS, CRM, RBAC, EF Core, JWT, CTV, TVV, KPI, CSP, COE, MinIO, CI/CD, VPS, SPA, TOC, QA, PoC, ERD, DTO, ORM…
- Đối chiếu: `docs/00-README.md`, `src/Polymind.Domain/Enums/Enums.cs`.

## TASK A.7 — Danh mục bảng
- Số trang: 1 · Trang vii
- Nội dung: liệt kê toàn bộ bảng theo chương kèm số trang.

## TASK A.8 — Danh mục hình
- Số trang: 2 · Trang viii–ix (trang x để trống kỹ thuật nếu section break đẩy sang)
- Nội dung: liệt kê toàn bộ hình/sơ đồ theo chương kèm số trang.

---

# PHẦN B — LỜI MỞ ĐẦU (3 trang: 1–3)

## TASK B.1 — LỜI MỞ ĐẦU
- Số trang dự kiến: 3 · Khoảng trang: 1–3
- Nội dung bắt buộc:
  - Bối cảnh thực tập tại Công ty TNHH Khoa học Kỹ thuật Vạn Thịnh (Thống Đạt Group).
  - Lý do tham gia dự án POLYMIND OLMS.
  - Mục tiêu đợt thực tập (5 mục tiêu cụ thể).
  - Phạm vi công việc và phạm vi báo cáo.
  - Phương pháp thực hiện (đọc mã nguồn, QA theo module, phối hợp AI-assisted, kiểm thử tự động).
  - Cấu trúc 4 chương của báo cáo.
- Source/tài liệu đối chiếu: `docs/00-README.md`, `WORKLOG.md`, `docs/testing/MODULE_QA_BOARD.md`.
- Hình/bảng: không.
- Thông tin cần bổ sung: thời gian bắt đầu – kết thúc thực tập.

---

# PHẦN C — CHƯƠNG 1. GIỚI THIỆU (12 trang: 4–15)

## TASK 1.1 — 1.1. GIỚI THIỆU ĐƠN VỊ THỰC TẬP (5 trang, 4–8)

### TASK 1.1.1 — 1.1.1. Thông tin pháp nhân và vị trí trong Thống Đạt Group
- Số trang: 1,5 · Khoảng trang: 4–5
- Nội dung bắt buộc: tên đầy đủ đơn vị; quan hệ đơn vị thành viên – tập đoàn chủ quản; phân vai giữa Thống Đạt Group (định hướng chiến lược đầu tư, cung ứng tài chính nguồn) và Vạn Thịnh (hiện thực hóa chiến lược số hóa, vận hành chuỗi F&B GUSTINO); vị trí văn phòng trung tâm điều hành tại TP. Hồ Chí Minh.
- Nguồn: **thông tin do sinh viên/doanh nghiệp cung cấp** (không có trong source code).
- Hình/bảng: **Bảng 1.1** Thông tin pháp nhân đơn vị thực tập; **Hình 1.1** Sơ đồ vị trí Vạn Thịnh trong cấu trúc Thống Đạt Group (sơ đồ tự dựng).
- Thông tin cần bổ sung: mã số doanh nghiệp, địa chỉ chi tiết, điện thoại, email, website, năm thành lập.

### TASK 1.1.2 — 1.1.2. Lĩnh vực hoạt động và sản phẩm – dịch vụ
- Số trang: 1,5 · Khoảng trang: 5–7
- Nội dung bắt buộc: 3 mảng hoạt động (nghiên cứu phát triển giải pháp phần mềm; chuyển đổi số doanh nghiệp; quản trị vận hành chuỗi bán lẻ ẩm thực GUSTINO); vị trí sản phẩm POLYMIND OLMS trong danh mục sản phẩm phần mềm.
- Hình/bảng: **Bảng 1.2** Lĩnh vực hoạt động và sản phẩm tiêu biểu.

### TASK 1.1.3 — 1.1.3. Cơ cấu tổ chức và bộ phận thực tập
- Số trang: 1,5 · Khoảng trang: 7–8
- Nội dung: sơ đồ tổ chức tới cấp phòng ban; bộ phận sinh viên thực tập (bộ phận phát triển phần mềm / chuyển đổi số); chuyên gia hướng dẫn trực tiếp Trương Chí Cường; cơ chế báo cáo công việc.
- Hình: **Hình 1.2** Sơ đồ cơ cấu tổ chức đơn vị thực tập (placeholder chờ sinh viên xác nhận từ doanh nghiệp).
- Thông tin cần bổ sung: tên chính xác các phòng ban, quy mô nhân sự, chức danh chuyên gia hướng dẫn.

### TASK 1.1.4 — 1.1.4. Cơ sở vật chất và môi trường làm việc
- Số trang: 0,5 · Trang 8
- Nội dung: mô tả môi trường làm việc, công cụ được cấp; **placeholder** cho thông tin chưa xác nhận.

## TASK 1.2 — 1.2. BỐI CẢNH DỰ ÁN VÀ ĐỐI TÁC NGHIỆP VỤ (2,5 trang, 9–11)

### TASK 1.2.1 — 1.2.1. Bài toán nghiệp vụ của lĩnh vực xuất khẩu lao động
- Số trang: 1,5 · Trang 9–10
- Nội dung: chuỗi giá trị từ Lead đến xuất cảnh; các điểm nghẽn khi quản lý bằng Excel/giấy (mất dấu hồ sơ, sai công nợ, tranh chấp hoa hồng đại lý, không truy vết được ai làm gì).
- Đối chiếu: `docs/01-business-analysis.md` (mục 1–3), `docs/03-workflow.md`.
- Hình: **Hình 1.3** Chuỗi nghiệp vụ xuất khẩu lao động từ Lead đến xuất cảnh.

### TASK 1.2.2 — 1.2.2. Đối tác nghiệp vụ và nguồn yêu cầu
- Số trang: 1 · Trang 10–11
- Nội dung: Vietgroup Edu là đối tác nghiệp vụ đóng góp yêu cầu và phản hồi sau trải nghiệm; cơ chế biến góp ý thành yêu cầu (backlog).
- Đối chiếu: `docs/07-backlog-vietgroup.md`, `Vietgroup Edu đóng góp ý kiến sau khi trải nghiệm.docx`, `POLYMIND - Phan hoi gop y Vietgroup Edu.docx`, `POLYMIND - Da lam theo bien ban hop 3-7-2026.docx`.
- Bảng: **Bảng 1.3** Nguồn yêu cầu và cách chuyển hóa thành hạng mục công việc.

## TASK 1.3 — 1.3. NHIỆM VỤ THỰC TẬP (2,5 trang, 11–13)

### TASK 1.3.1 — 1.3.1. Nội dung công việc được chuyên gia doanh nghiệp giao
- Số trang: 1 · Trang 11–12
- Nội dung: 6 nhóm nhiệm vụ (tìm hiểu quy trình nghiệp vụ; nghiên cứu công nghệ .NET 10 + Blazor; tham gia phát triển module; kiểm thử/QA theo module; viết tài liệu kỹ thuật; chuẩn bị triển khai production).
- Đối chiếu: `WORKLOG.md`, `docs/05-handoff-codex.md`, `docs/testing/MODULE_QA_BOARD.md`.
- Bảng: **Bảng 1.4** Nhiệm vụ thực tập và sản phẩm bàn giao tương ứng.

### TASK 1.3.2 — 1.3.2. Nội dung cần học hỏi thêm tại doanh nghiệp
- Số trang: 0,75 · Trang 12–13
- Nội dung: quy trình làm việc thực tế, chuẩn code, quy trình QA – fix – verify, kỹ năng đọc yêu cầu từ người dùng phi kỹ thuật.

### TASK 1.3.3 — 1.3.3. Mục tiêu và phạm vi công việc
- Số trang: 0,75 · Trang 13
- Nội dung: mục tiêu đo được; phạm vi **trong** (20 module nghiệp vụ, web) và **ngoài** phạm vi (mobile MAUI, Zalo ZNS, SMS OTP — thuộc giai đoạn sau).
- Đối chiếu: `WORKLOG.md` mục "VIỆC TIẾP THEO"; `docs/09-deploy-vps-vn.md`.

## TASK 1.4 — 1.4. PHƯƠNG PHÁP LÀM VIỆC VÀ CÔNG CỤ PHỐI HỢP (1,5 trang, 14–15)
- Nội dung: quy trình cộng tác qua `WORKLOG.md` (nguồn sự thật chung, format 1 entry/nhật ký, giữ 6 entry gần nhất); bảng QA `MODULE_QA_BOARD.md` với 3 trạng thái QA/Codex/Verification; nguyên tắc "không tự đổi quyết định đã chốt với người dùng"; công cụ: Git, Docker Compose, dotnet CLI, EF Core CLI, xUnit.
- Đối chiếu: `WORKLOG.md` dòng 7–27; `docs/testing/MODULE_QA_BOARD.md` mục "Chú thích trạng thái".
- Hình: **Hình 1.4** Vòng lặp QA – Fix – Verify giữa hai vai trò trong dự án.

## TASK 1.5 — 1.5. KẾT LUẬN CHƯƠNG 1 (0,5 trang, 15)
- Nội dung: chốt lại bối cảnh đơn vị, bài toán và nhiệm vụ; dẫn sang Chương 2.

---

# PHẦN D — CHƯƠNG 2. XÂY DỰNG HỆ THỐNG QUẢN LÝ XUẤT KHẨU LAO ĐỘNG POLYMIND OLMS (45 trang: 16–60)

> **Đây là chương dài nhất và chi tiết nhất.** Mọi kết luận kỹ thuật phải có đường dẫn source xác nhận.

## TASK 2.1 — 2.1. PHÂN TÍCH YÊU CẦU (5 trang, 16–20)

### TASK 2.1.1 — 2.1.1. Bài toán và phạm vi hệ thống
- Số trang: 1 · Trang 16
- Nội dung: mục tiêu "quản lý tập trung từ khi phát sinh Lead đến khi ứng viên xuất cảnh và hoàn tất nghĩa vụ"; ranh giới hệ thống.
- Đối chiếu: `docs/01-business-analysis.md:1-8`, `docs/00-README.md:1-18`.

### TASK 2.1.2 — 2.1.2. Tác nhân và vai trò người dùng
- Số trang: 1,5 · Trang 17–18
- Nội dung: **12 vai trò thực tế trong code** (super_admin, director, recruitment_manager, recruiter, consultant, document_staff, visa_staff, accountant, agent, collaborator, parent, student) — nêu rõ đã mở rộng từ 8 vai trò trong tài liệu phân tích ban đầu lên 12 theo góp ý Vietgroup.
- Đối chiếu: `src/Polymind.Infrastructure/Persistence/Constants/RoleNames.cs`, `docs/01-business-analysis.md` mục 2.
- Bảng: **Bảng 2.1** Danh sách vai trò người dùng và trách nhiệm chính (cột: mã role, tên tiếng Việt, phạm vi dữ liệu, file xác nhận).

### TASK 2.1.3 — 2.1.3. Yêu cầu chức năng
- Số trang: 1,5 · Trang 18–19
- Nội dung: liệt kê theo 20 module M01–M20 với mã yêu cầu; nhấn mạnh module phát sinh thêm so với phân tích gốc (Đào tạo, Hỗ trợ vay & Thu nợ, Tin nhắn, Trợ lý AI, Cổng phụ huynh/học viên).
- Đối chiếu: `docs/testing/MODULE_QA_BOARD.md` bảng module; `src/Polymind.Web/Components/Pages/**`.
- Bảng: **Bảng 2.2** Danh mục yêu cầu chức năng theo module (cột: mã module, tên, trang/route, quyền yêu cầu).

### TASK 2.1.4 — 2.1.4. Yêu cầu phi chức năng
- Số trang: 1 · Trang 19–20
- Nội dung: bảo mật (RBAC + data-scope + audit + CSP + rate limit), hiệu năng (Blazor Interactive Server nhạy độ trễ → chọn VPS trong nước), khả dụng (health check, backup), khả bảo trì (Clean Architecture, migration), khả kiểm thử.
- Đối chiếu: `src/Polymind.Web/Program.cs:108-142,236-266`, `docs/09-deploy-vps-vn.md:1-6`.

## TASK 2.2 — 2.2. KIẾN TRÚC HỆ THỐNG VÀ CÔNG NGHỆ (5 trang, 21–25)

### TASK 2.2.1 — 2.2.1. Kiến trúc tổng thể
- Số trang: 1,5 · Trang 21–22
- Nội dung: 4 project (Domain / Application / Infrastructure / Web) theo Clean Architecture; **ghi trung thực** rằng `Polymind.Application` hiện gần như rỗng và business logic nằm ở Blazor component + `Web/Api` + `Web/Notifications` — đây là điểm lệch giữa thiết kế và hiện trạng, đã được ghi nhận trong tài liệu QA.
- Đối chiếu: `Polymind.slnx`, các file `.csproj`, `docs/testing/MODULE_QA_BOARD.md:7`.
- Hình: **Hình 2.1** Sơ đồ kiến trúc tổng thể (client → Blazor Server circuit → EF Core → PostgreSQL; MinIO; Hangfire; Gemini API).

### TASK 2.2.2 — 2.2.2. Công nghệ và thư viện sử dụng
- Số trang: 1,5 · Trang 22–24
- Nội dung: bảng công nghệ **lấy phiên bản từ file `.csproj` thật**.
- Đối chiếu: `src/Polymind.Web/Polymind.Web.csproj`, `src/Polymind.Infrastructure/Polymind.Infrastructure.csproj`, `src/Polymind.Application/Polymind.Application.csproj`, `tests/Polymind.Tests/Polymind.Tests.csproj`.
- Bảng: **Bảng 2.3** Công nghệ sử dụng trong dự án (Nhóm / Công nghệ / Phiên bản / Vai trò / File xác nhận).

### TASK 2.2.3 — 2.2.3. Mô hình triển khai bằng Docker Compose
- Số trang: 1 · Trang 24
- Nội dung: stack dev (postgres 16-alpine, redis 7-alpine, minio) vs stack production (postgres, minio, web, caddy|nginx, duckdns); giới hạn log 10MB × 3; lý do gỡ service redis khỏi production.
- Đối chiếu: `docker-compose.yml`, `docker-compose.production.yml`, `WORKLOG.md` Session 70.
- Hình: **Hình 2.2** Sơ đồ triển khai container ở môi trường production.

### TASK 2.2.4 — 2.2.4. Cấu hình, ghi log và kiểm tra sức khỏe
- Số trang: 1 · Trang 25
- Nội dung: Serilog console + rolling file 14 ngày; `/health` kiểm tra database + MinIO trả JSON; nguyên tắc secret qua biến môi trường.
- Đối chiếu: `src/Polymind.Web/Program.cs:38-46,89-91,281-304`, `src/Polymind.Web/Health/PolymindHealthChecks.cs`.

## TASK 2.3 — 2.3. THIẾT KẾ CƠ SỞ DỮ LIỆU (5 trang, 26–30)

### TASK 2.3.1 — 2.3.1. Nguyên tắc thiết kế và quy ước
- Số trang: 1 · Trang 26
- Nội dung: PostgreSQL 16, khóa chính GUID, đặt tên snake_case tự động bằng `EFCore.NamingConventions`, enum lưu dạng chuỗi qua `EnumToStringConverter`, `DateTimeOffset` phải chuẩn UTC.
- Đối chiếu: `src/Polymind.Infrastructure/Persistence/ApplicationDbContext.cs:203-215`, `docs/05-handoff-codex.md:87`.

### TASK 2.3.2 — 2.3.2. Các nhóm bảng dữ liệu
- Số trang: 2 · Trang 27–28
- Nội dung: 6 nhóm — định danh & phân quyền; CRM & ứng viên; đơn hàng & quy trình; tài chính & vay nợ; đối tác & hoa hồng; vận hành (visa, đào tạo, thông báo, tin nhắn, audit). Nêu 23 DbSet thực tế.
- Đối chiếu: `ApplicationDbContext.cs:17-43`, `src/Polymind.Domain/Entities/*.cs`.
- Hình: **Hình 2.3** Sơ đồ quan hệ thực thể (ERD) rút gọn.
- Bảng: **Bảng 2.4** Nhóm bảng dữ liệu và thực thể tương ứng.

### TASK 2.3.3 — 2.3.3. Ràng buộc toàn vẹn và chỉ mục
- Số trang: 1,5 · Trang 29–30
- Nội dung: unique index nghiệp vụ quan trọng — `(AgentId, CandidateId, Milestone)` chống trả hoa hồng hai lần; `Receipt.LoanRepaymentId` unique chống thu trùng kỳ; `(UserId, Type, ReferenceId, Channel)` chống gửi trùng thông báo; `(CandidateId, Track)` mỗi ứng viên một hồ sơ đào tạo/mảng; precision tiền tệ 15,2.
- Đối chiếu: `ApplicationDbContext.cs:92-201`.
- Hình: **Hình 2.4** Ảnh chụp mã nguồn cấu hình chỉ mục idempotency hoa hồng.

### TASK 2.3.4 — 2.3.4. Quản lý migration
- Số trang: 0,5 · Trang 30
- Nội dung: 18 migration từ `InitialCreate` (24/06/2026) đến `AddFinanceArchive` (13/07/2026); migration tự áp lúc khởi động; nguyên tắc migration additive.
- Đối chiếu: `src/Polymind.Infrastructure/Persistence/Migrations/`, `Program.cs:340-356`.
- Bảng: **Bảng 2.5** Danh sách migration và mục đích.

## TASK 2.4 — 2.4. XÁC THỰC VÀ PHÂN QUYỀN (4,5 trang, 31–35)

### TASK 2.4.1 — 2.4.1. Xác thực phiên web bằng cookie
- Số trang: 1 · Trang 31
- Nội dung: ASP.NET Core Identity, trang `/login` render tĩnh, cookie HttpOnly/SameSite=Lax/Secure ở production, hết hạn 8 giờ trượt, revalidation theo security stamp, rate limit 30 lượt/phút/IP cho `POST /login`.
- Đối chiếu: `Program.cs:110-155`, `src/Polymind.Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`, `src/Polymind.Infrastructure/Identity/AuthenticationSecurityPolicy.cs`.

### TASK 2.4.2 — 2.4.2. Xác thực REST API bằng JWT
- Số trang: 1 · Trang 32
- Nội dung: `POST /api/auth/login` cấp token; validate issuer/audience/lifetime/signing key; khóa ký bắt buộc từ biến môi trường ở production (fail-fast); Swagger chỉ bật ở Development.
- Đối chiếu: `Program.cs:157-221,320-333`, `src/Polymind.Web/Api/JwtTokenService.cs`, `AuthEndpoints.cs`.

### TASK 2.4.3 — 2.4.3. Phân quyền theo permission claim (RBAC)
- Số trang: 1,5 · Trang 33–34
- Nội dung: 21 resource × 5 action = 105 permission sinh tự động; `PermissionPolicyProvider` tạo policy động theo tên `resource:action`; `PermissionAuthorizationHandler`; super_admin được bỏ qua; bản đồ role → permission trong `DbSeeder.RolePermissionMap`.
- Đối chiếu: `PermissionRegistry.cs`, `src/Polymind.Web/Authorization/PermissionAuthorization.cs`, `DbSeeder.cs:37-115`, `src/Polymind.Web/Identity/PermissionClaimsPrincipalFactory.cs`.
- Hình: **Hình 2.5** Ảnh chụp mã nguồn cơ chế kiểm tra quyền truy cập.
- Bảng: **Bảng 2.6** Trích ma trận vai trò – quyền (đầy đủ ở Phụ lục B).

### TASK 2.4.4 — 2.4.4. Phạm vi dữ liệu cho đối tác và cổng cá nhân hóa
- Số trang: 1 · Trang 34–35
- Nội dung: `AgentScope` phân biệt nhân sự nội bộ / đại lý / CTV / phụ huynh / học viên; nguyên tắc fail-closed; RB-1 ẩn thông tin nhạy cảm của CTV với phụ huynh và học viên.
- Đối chiếu: `src/Polymind.Web/Identity/AgentScope.cs`, `src/Polymind.Domain/Security/CandidateAccessScope.cs`, `WORKLOG.md` mục RB-1.

## TASK 2.5 — 2.5. CÁC MODULE NGHIỆP VỤ (14 trang, 36–49)

> Mỗi tiểu mục viết theo khung: **mục tiêu → đối tượng sử dụng → quy trình → giao diện/route → xử lý backend → dữ liệu → phân quyền → kết quả**. Không lặp lại nguyên đoạn giữa các module.

### TASK 2.5.1 — 2.5.1. Quản lý Lead (CRM)
- 1,25 trang · Trang 36–37 · Route `/leads`, `/leads/{id}`, `/leads/converted`
- Nội dung: 10 trạng thái + 10 nguồn Lead; timeline hoạt động; chuyển đổi Lead → ứng viên qua `LeadConversionRules`; nhắc chăm sóc Lead quá hạn.
- Đối chiếu: `Components/Pages/Leads/*.razor`, `src/Polymind.Domain/Leads/LeadConversionRules.cs`, `src/Polymind.Web/Display/LeadCareRules.cs`, `Api/LeadsEndpoints.cs`.
- Hình: **Hình 2.6** Giao diện danh sách Lead; **Hình 2.7** Sơ đồ vòng đời Lead.

### TASK 2.5.2 — 2.5.2. Quản lý ứng viên và hồ sơ tài liệu
- 1,25 trang · Trang 37–38 · Route `/candidates`, `/candidates/{id}`
- Nội dung: hồ sơ nhân thân, CCCD/hộ chiếu, người thân; upload tài liệu lên MinIO có versioning; liên kết tài khoản phụ huynh/học viên.
- Đối chiếu: `Components/Pages/Candidates/*.razor`, `Storage/MinioDocumentStorage.cs`, `Domain/Security/CandidateAccountLinkRules.cs`.
- Hình: **Hình 2.8** Giao diện chi tiết ứng viên.

### TASK 2.5.3 — 2.5.3. Quản lý đơn hàng tuyển dụng
- 0,75 trang · Trang 38–39 · Route `/jobs`, `/jobs/{id}`
- Nội dung: 3 nhóm việc làm (ngoài nước / trong nước / du học); 5 trạng thái đơn hàng; chi phí làm căn cứ tính hoa hồng.
- Đối chiếu: `Components/Pages/JobOrders/*.razor`, `Domain/JobOrders/JobOrderCreationRules.cs`, `Enums.cs:27-33`.

### TASK 2.5.4 — 2.5.4. Quy trình 20 bước hồ sơ ứng viên
- 1,5 trang · Trang 39–41
- Nội dung: **20 bước chính + bước phụ 7.5** (`ReselectJobOrder`); lý do mở rộng từ 17 bước tài liệu gốc lên 20 bước theo góp ý Vietgroup; tính % tiến độ; quy tắc đổi đơn hàng reset tiến trình (RB-2, chỉ super_admin, xác nhận mật khẩu).
- Đối chiếu: `Enums.cs:42-65`, `Display/WorkflowSteps.cs`, `Display/WorkflowStepAccess.cs`, `WORKLOG.md` RB-2 + Session 64.
- Hình: **Hình 2.9** Sơ đồ quy trình 20 bước; **Bảng 2.7** Bảng đối chiếu 17 bước thiết kế ban đầu ↔ 20 bước hiện hành.

### TASK 2.5.5 — 2.5.5. Module Đào tạo
- 0,75 trang · Trang 41 · Route `/training`, `/training/{id}`
- Nội dung: 2 mảng đào tạo tách rời (Language / Vocational); phiếu đánh giá 4 tiêu chí (chuyên cần, chuyên môn, kỷ luật, tài chính) với 4 mức; quyền chỉ đọc cho recruiter/document/visa/accountant (CR-M08-1); CTV không được vào module này.
- Đối chiếu: `Components/Pages/Training/*.razor`, `Enums.cs:176-186`, `tests/.../M08_TrainingRulesTests.cs`.

### TASK 2.5.6 — 2.5.6. Module Tài chính
- 1,5 trang · Trang 42–43 · Route `/finance`
- Nội dung: lịch đóng tiền 4 giai đoạn 20/30/30/20; tách `Submitted` (ứng viên đã nộp) khỏi `Paid` (kế toán duyệt); `PaymentPostingService` là đường duy nhất chuyển sang Paid, tự sinh phiếu thu và kích hoạt hoa hồng; chặn duyệt sai thứ tự; khoản chi phải được duyệt mới xuất được phiếu chi (RB-7/U-M10-1).
- Đối chiếu: `Finance/PaymentPostingService.cs`, `Domain/Finance/PaymentPostingRules.cs`, `Domain/Finance/ExpenseApprovalRules.cs`, `Components/Pages/Finance/*.razor`.
- Hình: **Hình 2.10** Sơ đồ luồng duyệt khoản thu và sinh phiếu thu.

### TASK 2.5.7 — 2.5.7. Hỗ trợ vay vốn và thu nợ
- 1 trang · Trang 43–44 · Route `/loans`, `/debt-collection`
- Nội dung: hai loại nợ (ngân hàng / công ty); kỳ trả góp; **nguyên tắc không bao giờ miễn nợ**, chỉ tất toán khi thu đủ 100%; nợ công ty chưa tất toán thì chặn bước B20; mỗi lần thu sinh phiếu thu.
- Đối chiếu: `Domain/Loans/LoanCollectionRules.cs`, `Enums.cs:151-170`, `tests/.../M11_LoanRulesTests.cs`, `ApplicationDbContext.cs:115`.

### TASK 2.5.8 — 2.5.8. Đại lý, cộng tác viên và hoa hồng
- 1,5 trang · Trang 44–46 · Route `/agents`, `/agents/{id}`, `/agents/tree`, `/my-commissions`
- Nội dung: cây đại lý – CTV; cấu hình % theo mốc; `CommissionEngine` sinh hoa hồng theo **giai đoạn đóng tiền** chứ không theo bước workflow; idempotent bằng unique index + retry; snapshot % chia cho CTV tại thời điểm phát sinh (CR-M09-1); đại lý chỉ thấy doanh số của chính mình (CR-M09-2).
- Đối chiếu: `Commissions/CommissionEngine.cs`, `Domain/Commissions/*.cs`, `Components/Pages/Agents/*.razor`, `Components/Pages/Portal/MyCommissions.razor`.
- Hình: **Hình 2.11** Sơ đồ phát sinh và duyệt hoa hồng đại lý.

### TASK 2.5.9 — 2.5.9. Visa và xuất cảnh
- 1 trang · Trang 46 · Route `/visa`
- Nội dung: 6 trạng thái visa; vé máy bay; nút "Xác nhận đã bay" ghi `Flight.ActualDepartureAt`; ghi audit riêng `confirm_departure`/`clear_departure`; người tạo hồ sơ là người đăng nhập thật (BUG_M12_01/02).
- Đối chiếu: `Components/Pages/Visas/*.razor`, `Domain/Visas/VisaFlightCreationRules.cs`, `tests/.../M12_VisaFlightRulesTests.cs`.

### TASK 2.5.10 — 2.5.10. Thông báo tự động
- 1 trang · Trang 47 · Route `/notifications`
- Nội dung: 10 loại thông báo, 4 kênh (InApp/Email/SMS/Zalo); Hangfire recurring job 5 phút; chống gửi trùng bằng unique index; RB-6 bấm thông báo điều hướng đúng trang nguồn; RB-7 mở rộng nhóm thông báo tài chính/hoa hồng/visa; người nhận thông báo tài chính chỉ gồm kế toán + super_admin (CR-M13-1).
- Đối chiếu: `Notifications/NotificationService.cs`, `NotificationJob.cs`, `Domain/Notifications/*.cs`, `Program.cs:335-338`.

### TASK 2.5.11 — 2.5.11. Tin nhắn nội bộ
- 1 trang · Trang 48 · Route `/messages`
- Nội dung: ma trận 5 bậc `MessagingTiers`; 4 mệnh đề (super admin hai chiều; chênh bậc ≤ 1; 3 ngoại lệ chặn; tầng quan hệ ứng viên siết thêm); fail-closed; ghi âm tin nhắn thoại.
- Đối chiếu: `Domain/Messaging/MessagingTiers.cs`, `Domain/Messaging/CandidateMessagingRelationship.cs`, `Identity/MessagingPolicy.cs`, `docs/messaging-tiers.md`.
- Bảng: **Bảng 2.8** Ma trận quyền nhắn tin theo bậc.

### TASK 2.5.12 — 2.5.12. Báo cáo và xuất file
- 1 trang · Trang 48–49 · Route `/reports`, nhóm `/export`
- Nội dung: 8 báo cáo × 3 định dạng (CSV/XLSX/PDF) = 24 endpoint; in phiếu thu/chi PDF bằng QuestPDF; tách quyền `financial_reports:read` khỏi `reports:read` (CR-M16-1); truyền khoảng thời gian vào file xuất (BUG_M16_01).
- Đối chiếu: `Reporting/CsvExportEndpoints.cs:25-48`, `Domain/Reporting/ReportAccessRules.cs`.

### TASK 2.5.13 — 2.5.13. Trợ lý AI
- 0,75 trang · Trang 49 · Route `/ai`
- Nội dung: tích hợp Gemini `gemini-2.5-flash`; `AiSessionStore` lưu hội thoại theo `UserId`, sống qua F5, xóa khi đăng xuất (RB-5); `AiDataScope` giới hạn dữ liệu nạp vào ngữ cảnh theo phạm vi của người dùng (BUG_M15_01).
- Đối chiếu: `Ai/GeminiClient.cs`, `Ai/AiSessionStore.cs`, `Domain/Ai/AiDataScope.cs`, `Program.cs:59-63,307-315`.
- **Bảo mật:** không chụp/không in API key trong báo cáo.

### TASK 2.5.14 — 2.5.14. Quản trị và nhật ký kiểm toán
- 0,75 trang · Trang 49 · Route `/admin`, `/admin/parents-students`, `/hangfire`
- Nội dung: quản lý tài khoản/vai trò; dropdown tạo tài khoản không còn Đại lý/CTV (Session 64); audit log ghi old/new value dạng JSONB; dashboard Hangfire chỉ super_admin/director.
- Đối chiếu: `Components/Pages/Admin/*.razor`, `Auditing/AuditLogHelpers.cs`, `Authorization/HangfireDashboardAuthorizationFilter.cs`.

## TASK 2.6 — 2.6. CÔNG VIỆC CỦA SINH VIÊN TRONG DỰ ÁN (3,5 trang, 50–53)

### TASK 2.6.1 — 2.6.1. Vai trò được phân công và cách chia việc
- 1 trang · Trang 50
- Nội dung: vai trò trong nhóm; cơ chế bàn giao qua `WORKLOG.md`; quy tắc luôn để `dotnet build Polymind.slnx` xanh trước khi kết thúc phiên.

### TASK 2.6.2 — 2.6.2. Các hạng mục sinh viên trực tiếp thực hiện
- 1,5 trang · Trang 51–52
- Nội dung: bảng hạng mục ↔ file thay đổi ↔ kết quả kiểm thử, lấy từ nhật ký session.
- Đối chiếu: `WORKLOG.md` nhật ký Session 64–70.
- Bảng: **Bảng 2.9** Hạng mục công việc đã thực hiện và bằng chứng.
- **Thông tin cần bổ sung:** xác nhận của sinh viên về những hạng mục nào do chính mình làm so với phần do thành viên khác làm.

### TASK 2.6.3 — 2.6.3. Quy trình QA 20 module
- 1 trang · Trang 52–53
- Nội dung: vòng QA → bug report → fix → verification report; ba trục trạng thái; quy tắc "không đánh dấu Fixed nếu chưa verify độc lập".
- Đối chiếu: `docs/testing/MODULE_QA_BOARD.md`, `docs/testing/SESSION_CHECKPOINT.md`.
- Hình: **Hình 2.12** Ảnh chụp bảng điều phối QA 20 module.

## TASK 2.7 — 2.7. KIỂM THỬ (3 trang, 53–56)

### TASK 2.7.1 — 2.7.1. Chiến lược kiểm thử
- 1 trang · Trang 53–54
- Nội dung: 3 lớp (unit test logic thuần; kiểm thử thủ công theo test case; probe trực tiếp trên PostgreSQL cho race condition); lý do test project **cố ý không tham chiếu** `Polymind.Web` (tránh khóa DLL khi dev host đang chạy) và hệ quả là khoảng trống coverage.
- Đối chiếu: `tests/Polymind.Tests/Polymind.Tests.csproj` (phần chú thích), `MODULE_QA_BOARD.md:8`.

### TASK 2.7.2 — 2.7.2. Kiểm thử tự động
- 1,5 trang · Trang 54–55
- Nội dung: 22 file test theo module, 149 phương thức test `[Fact]`/`[Theory]`, tổng **236 test case pass** ở lần chạy gần nhất; ví dụ bất biến được khóa: ma trận nhắn tin, chống leo thang quyền, không seed tài khoản mẫu ở production.
- Đối chiếu: `tests/Polymind.Tests/*.cs`, `WORKLOG.md` Session 70.
- Bảng: **Bảng 2.10** Danh sách bộ test tự động theo module (đầy đủ ở Phụ lục E).
- Hình: **Hình 2.13** Ảnh chụp kết quả `dotnet test`.

### TASK 2.7.3 — 2.7.3. Kiểm thử thủ công và kết quả QA
- 0,5 trang · Trang 55–56
- Nội dung: 20/20 module đạt "Verified"; danh sách bug đã đóng theo mức nghiêm trọng; residual đã ghi nhận.
- Bảng: **Bảng 2.11** Tổng hợp lỗi phát hiện và trạng thái xử lý.

## TASK 2.8 — 2.8. TRIỂN KHAI (2,5 trang, 56–58)

### TASK 2.8.1 — 2.8.1. Môi trường phát triển
- 0,75 trang · Trang 56
- Nội dung: `docker compose up -d` + `dotnet run` cổng 5177; seed 13 tài khoản mẫu chỉ ở Development.
- Đối chiếu: `docs/00-README.md:19-52`, `DbSeeder.cs:19-35,126-131`.
- **Bảo mật:** nêu mật khẩu mẫu môi trường phát triển là chấp nhận được vì đã có chốt chặn không seed ở production; **không in mật khẩu production**.

### TASK 2.8.2 — 2.8.2. Phương án triển khai chính thức
- 1,25 trang · Trang 57
- Nội dung: 4 giai đoạn; lý do chọn VPS Việt Nam 4vCPU/8GB thay vì Cloud Run + Supabase (Blazor Interactive Server nhạy RTT; Cloud Run cắt WebSocket sau 60 phút; Hangfire cần always-on); thứ tự bắt buộc khi bật Cloudflare proxy; checklist go-live.
- Đối chiếu: `docs/09-deploy-vps-vn.md`, `WORKLOG.md` Session 70.
- Bảng: **Bảng 2.12** So sánh hai phương án hạ tầng và chi phí.

### TASK 2.8.3 — 2.8.3. Sao lưu và phục hồi
- 0,5 trang · Trang 58
- Nội dung: `scripts/backup.sh` (pg_dump + MinIO + đẩy lên Backblaze B2), cron 2h sáng; nguyên tắc "backup chưa test khôi phục thì chưa phải backup".
- Đối chiếu: `scripts/backup.sh`, `scripts/restore.sh`, `docs/09-deploy-vps-vn.md:106-123`.

## TASK 2.9 — 2.9. KHÓ KHĂN KỸ THUẬT VÀ GIẢI PHÁP (2 trang, 58–60)
- Nội dung: 6 tình huống thật, mỗi tình huống nêu hiện tượng → nguyên nhân → cách xử lý → bằng chứng:
  1. `DateTimeOffset` không chuẩn UTC làm Npgsql ném lỗi.
  2. Race condition sinh hoa hồng trùng → unique index + retry hẹp.
  3. Ba đường code cùng chuyển khoản thu sang Paid gây thiếu hoa hồng → gom về một service.
  4. Gán sai người thực hiện (lấy user đầu bảng thay vì người đăng nhập) ở nhiều màn hình.
  5. `MessagingPolicy` fallback `return true` khiến mọi nhân sự nhắn được nhau → ma trận 5 bậc fail-closed.
  6. Khóa DLL khi dev host đang chạy làm build test thất bại → tách output path / không tham chiếu Web.
- Đối chiếu: `docs/05-handoff-codex.md:85-93`, `MODULE_QA_BOARD.md` bảng lịch sử bug, `WORKLOG.md`.
- Bảng: **Bảng 2.13** Khó khăn kỹ thuật và giải pháp đã áp dụng.

## TASK 2.10 — 2.10. KẾT LUẬN CHƯƠNG 2 (0,5 trang, 60)

---

# PHẦN E — CHƯƠNG 3. KẾT QUẢ THỰC TẬP (15 trang: 61–75)

## TASK 3.1 — 3.1. KẾT QUẢ CÔNG VIỆC ĐÃ ĐẠT ĐƯỢC (4 trang, 61–64)

### TASK 3.1.1 — 3.1.1. Hạng mục đã hoàn thành
- 1,5 trang · Trang 61–62
- Bảng: **Bảng 3.1** Hạng mục hoàn thành theo module (mức độ hoàn thành, bằng chứng).

### TASK 3.1.2 — 3.1.2. Kết quả kiểm thử
- 1,5 trang · Trang 62–64
- Nội dung: 236 test pass / 0 fail / 0 skip; solution build 0 warning / 0 error; 20/20 module Verified; **ghi rõ giới hạn**: chưa có harness HTTP + DB tích hợp, một số hạng mục Session 70 mới ở mức build + unit test, chưa smoke test runtime.
- Đối chiếu: `WORKLOG.md` Session 70 mục "Đã test" và "Lưu ý".

### TASK 3.1.3 — 3.1.3. Mức độ hoàn thành nhiệm vụ được giao
- 1 trang · Trang 64
- Bảng: **Bảng 3.2** Đối chiếu nhiệm vụ được giao ↔ kết quả.

## TASK 3.2 — 3.2. KIẾN THỨC VÀ KỸ NĂNG THU ĐƯỢC (3 trang, 65–67)

### TASK 3.2.1 — 3.2.1. Kiến thức chuyên môn
- 1,75 trang · Trang 65–66
- Nội dung: .NET 10 / Blazor Interactive Server; EF Core 10 + PostgreSQL; RBAC và data-scope; idempotency và race condition; kiểm thử tự động; Docker và vận hành.

### TASK 3.2.2 — 3.2.2. Kỹ năng nghề nghiệp
- 1,25 trang · Trang 66–67
- Nội dung: đọc và bảo trì mã nguồn lớn; viết tài liệu bàn giao; giao tiếp với người dùng phi kỹ thuật; kỷ luật "không tự đổi quyết định đã chốt"; quản lý công việc bằng nhật ký chung.

## TASK 3.3 — 3.3. KHÓ KHĂN, CÁCH XỬ LÝ VÀ HẠN CHẾ (2 trang, 68–69)
- Nội dung: khó khăn về nghiệp vụ (yêu cầu thay đổi giữa chừng), kỹ thuật, môi trường (Docker Desktop không chạy được nên chưa smoke test); hạn chế còn tồn tại (Application layer rỗng, thiếu harness tích hợp, chưa có CI/CD, chưa smoke test CSP).
- Đối chiếu: `WORKLOG.md` mục BLOCKERS.
- Bảng: **Bảng 3.3** Khó khăn – cách xử lý – hạn chế còn lại.

## TASK 3.4 — 3.4. BẢNG GHI NHẬN KẾT QUẢ THỰC TẬP HÀNG TUẦN (4 trang, 70–73)
- Nội dung: bảng theo mẫu 6 (Tuần / Thời gian / Nội dung công việc / Kết quả / Nhận xét của chuyên gia / Ký tên). Nội dung công việc **chỉ điền được từ nhật ký session có thật**; cột nhận xét và chữ ký để trống.
- **Thông tin cần bổ sung:** ngày bắt đầu – kết thúc từng tuần; xác nhận và chữ ký của chuyên gia hướng dẫn.

## TASK 3.5 — 3.5. BẢNG ĐÁNH GIÁ QUÁ TRÌNH THỰC TẬP TỐT NGHIỆP (1 trang, 74)
- Nội dung: biểu mẫu 7 (do chuyên gia doanh nghiệp đánh giá) — chỉ dựng khung, **không tự điền điểm/nhận xét/chữ ký**.

## TASK 3.6 — 3.6. PHIẾU ĐÁNH GIÁ KẾT QUẢ THỰC TẬP TỐT NGHIỆP (1 trang, 75)
- Nội dung: biểu mẫu 8 (do giảng viên hướng dẫn đánh giá) — chỉ dựng khung, để trống.

## TASK 3.7 — 3.7. KẾT LUẬN CHƯƠNG 3 (0,5 trang, 75)

---

# PHẦN F — CHƯƠNG 4. KẾT LUẬN VÀ KIẾN NGHỊ (6 trang: 76–81)

## TASK 4.1 — 4.1. TỔNG KẾT QUÁ TRÌNH THỰC TẬP (1,5 trang, 76–77)
## TASK 4.2 — 4.2. KẾT QUẢ VÀ NĂNG LỰC ĐẠT ĐƯỢC (1 trang, 77–78)
## TASK 4.3 — 4.3. HẠN CHẾ VÀ ĐỊNH HƯỚNG PHÁT TRIỂN (1,5 trang, 78–79)
- Nội dung: lộ trình đã chốt sau go-live — PWA + CI/CD → SMS OTP 2FA → Zalo ZNS → ứng dụng MAUI; đề xuất kỹ thuật: dựng harness tích hợp, đưa nghiệp vụ về `Polymind.Application`, bổ sung rowversion chống ghi đè đồng thời.
- Đối chiếu: `WORKLOG.md` mục "VIỆC TIẾP THEO", `MODULE_QA_BOARD.md` mục residual.
## TASK 4.4 — 4.4. KIẾN NGHỊ VỚI DOANH NGHIỆP (1 trang, 80)
## TASK 4.5 — 4.5. KIẾN NGHỊ VỚI CƠ SỞ ĐÀO TẠO (1 trang, 81)

---

# PHẦN G — TÀI LIỆU THAM KHẢO (2 trang: 82–83)

## TASK G.1 — Danh mục tài liệu tham khảo
- Chỉ liệt kê nguồn **thực sự đã sử dụng**: tài liệu nội bộ dự án (`docs/00` → `docs/09`, `WORKLOG.md`, `docs/testing/*`), tài liệu chính thức của Microsoft về .NET/Blazor/EF Core/ASP.NET Core Identity, tài liệu MudBlazor, PostgreSQL, Hangfire, MinIO, QuestPDF, ClosedXML, Docker.
- **Không bịa thông tin xuất bản.** Tài liệu trực tuyến ghi rõ là tài liệu chính thức của nhà cung cấp; **cần sinh viên bổ sung ngày truy cập**.

---

# PHẦN H — PHỤ LỤC (7 trang: 84–90)

## TASK H.1 — Phụ lục A. Danh mục vai trò và tài khoản demo theo vai trò (1 trang, 84)
- Đối chiếu: `DbSeeder.cs:19-35`, `docs/00-README.md:37-52`. **Che mật khẩu**, chỉ ghi "mật khẩu dùng chung ở môi trường phát triển".

## TASK H.2 — Phụ lục B. Ma trận vai trò – quyền đầy đủ (2 trang, 85–86)
- Đối chiếu: `DbSeeder.cs:37-115`, `PermissionRegistry.cs`.

## TASK H.3 — Phụ lục C. Danh mục thực thể và bảng dữ liệu (1,5 trang, 86–88)
- Đối chiếu: `ApplicationDbContext.cs:17-43`, `src/Polymind.Domain/Entities/`.

## TASK H.4 — Phụ lục D. Danh mục route giao diện, endpoint API và endpoint xuất báo cáo (1 trang, 88–89)
- Đối chiếu: `Components/Pages/**/*.razor` (chỉ thị `@page`), `Api/*.cs`, `Reporting/CsvExportEndpoints.cs`.

## TASK H.5 — Phụ lục E. Danh mục bộ kiểm thử tự động (1,5 trang, 89–90)
- Đối chiếu: `tests/Polymind.Tests/*.cs`.

---

# BẢNG TỔNG HỢP SỐ TRANG

| Phần | Dự kiến | **Thực tế đã render** | Trang vật lý |
|---|---:|---:|---|
| Phần đầu (bìa, lót bìa, 2 trang nhận xét, mục lục, 3 danh mục) | 10 | **12** | 1–12 |
| Lời mở đầu | 3 | **3** | 13–15 |
| Chương 1 | 12 | **11** | 16–26 |
| Chương 2 | 45 | **47** | 27–73 |
| Chương 3 | 15 | **11** | 74–84 |
| Chương 4 | 6 | **5** | 85–89 |
| Tài liệu tham khảo | 2 | **2** | 90–91 |
| Phụ lục (A–H) | 7 | **7** | 92–98 |
| **TỔNG CỘNG** | **100** | **98** | |

Khoảng cho phép: 97–103 trang. **Kết quả render thực tế: 98 trang — ĐẠT.**
Chương 2 là chương dài nhất (47 trang, chiếm 48% toàn báo cáo) — ĐẠT yêu cầu mục 5.2.

## Kết quả kiểm tra tự động trên file DOCX cuối

| Hạng mục kiểm tra | Kết quả |
|---|---|
| Gói DOCX/ZIP hợp lệ, đủ thành phần bắt buộc | Đạt |
| Lề 3/2/2/2 cm, khổ A4 trên cả 3 section | Đạt |
| Font Times New Roman, thân bài 13 pt, giãn dòng 1,4 | Đạt |
| Heading 1/2/3/4 đúng cỡ 16/14/13/13, đậm–đậm–đậm–nghiêng, dùng style thật | Đạt |
| Không có heading vượt 4 cấp, không nhảy cấp | Đạt |
| Trường TOC mục lục, danh mục bảng, danh mục hình cập nhật được bằng F9 | Đạt |
| Trường SEQ đánh số hình/bảng theo chương | Đạt (18 hình, 35 bảng) |
| Số trang: bìa/lót bìa không số · phần đầu La Mã · từ Lời mở đầu đánh lại từ 1 | Đạt |
| Không còn cú pháp Markdown, đường dẫn tạm hay marker kỹ thuật | Đạt |
| Không lộ mật khẩu, token, khóa API hay chuỗi kết nối | Đạt |
| Không có bảng tràn lề (mọi bảng ≤ 16 cm vùng in) | Đạt |
| Không có trang trắng ngoài ý muốn | Đạt |
| 13 heading bắt buộc theo mẫu trường đều có mặt | Đạt |

---

# DANH SÁCH THÔNG TIN CẦN SINH VIÊN BỔ SUNG (đặt placeholder đúng vị trí trong DOCX)

| # | Thông tin | Vị trí trong báo cáo |
|---|---|---|
| 1 | MSSV, lớp, ngành đào tạo, niên khóa | Trang bìa, trang lót bìa |
| 2 | Tháng/năm nộp báo cáo | Trang bìa |
| 3 | Địa chỉ, điện thoại, email, website chính thức của công ty | 1.1.1 |
| 4 | Mã số doanh nghiệp, năm thành lập | 1.1.1 |
| 5 | Tên chính xác các phòng ban và quy mô nhân sự | 1.1.3 |
| 6 | Chức danh của chuyên gia hướng dẫn | 1.1.3 |
| 7 | Cơ sở vật chất được doanh nghiệp cấp | 1.1.4 |
| 8 | Danh sách đối tác chính thức của doanh nghiệp | 1.2.2 |
| 9 | Ngày bắt đầu – kết thúc thực tập, số tuần | Lời mở đầu, 3.4 |
| 10 | Xác nhận hạng mục nào do chính sinh viên thực hiện | 2.6.2 |
| 11 | Ảnh chụp màn hình thật của hệ thống (theo từng ghi chú hình) | Chương 2, Chương 3 |
| 12 | Nội dung công việc chi tiết từng tuần | 3.4 |
| 13 | Nhận xét, điểm số, chữ ký của chuyên gia và giảng viên | A.3, A.4, 3.4, 3.5, 3.6 |
| 14 | Ngày truy cập các tài liệu trực tuyến | Tài liệu tham khảo |
