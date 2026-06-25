# Backlog VietGroup × Polymind (họp 25/6 + ghi chú demo)

> **Bối cảnh:** Ra mắt dự kiến **20–22/08**; **demo mỗi tuần** để review. GĐ1 nội bộ
> (trực quan, có thể tích hợp TMĐT), GĐ2 mở rộng cộng đồng. Định hướng: **mobile-first
> dễ dùng**, **trực quan (icon thay chữ)**, **nhân văn với ứng viên**, **minh bạch chi
> phí/hoa hồng**, **AI phân tích hồ sơ**, hướng tới **Big Data**.
>
> Chia 3 nhóm: **NHÓM 1** làm ngay (không cần đối tác) — làm trước cho demo tuần.
> **NHÓM 2** cần thông tin/quy trình đối tác — lưu chờ, lấy đủ info rồi build.
> **NHÓM 3** tầm nhìn xa — ghi nhận, chưa scope.

---

## NHÓM 1 — LÀM ĐƯỢC NGAY (không cần xin đối tác)
> Dùng mặc định hợp lý, tinh chỉnh sau theo góp ý hằng tuần.

- [x] **1.2 Highlight đơn hàng theo quốc gia** — badge màu + icon theo nước (Nhật/Hàn/Đài/Sing/Đức…) trong danh sách/thẻ đơn hàng; đánh dấu nổi bật đơn **đang tuyển (mở)**. *(handwritten #11; PDF mục 2)* — **XONG (S27):** `Display/CountryDisplay.cs` (cờ emoji + tên chuẩn + màu theo nước), `Labels.ColorOf/IsOpen(JobOrderStatus)`; thẻ `/job-orders` có cờ + viền màu, đơn mở chip xanh + nổi lên đầu danh sách; header `/job-orders/{id}` có cờ.
- [x] **1.3 Trường Cha/Mẹ/Người bảo hộ** — thêm vào hồ sơ ứng viên: họ tên, SĐT, quan hệ, CCCD, địa chỉ, nghề nghiệp (tách khỏi "liên hệ khẩn cấp" hiện có). Đây là nền cho cổng phụ huynh sau này. *(handwritten #15; PDF 4A)* — **XONG (S27):** 6 trường `Guardian*` trên `Candidate` + migration `AddCandidateGuardianFields`; tab riêng trong CandidateDialog; section riêng (icon FamilyRestroom) trong CandidateDetail.
- [x] **1.4 Hiển thị đơn hàng trực quan hơn** — chuyển ưu tiên sang dạng **thẻ + icon**, bớt chữ (theo định hướng "bỏ list nhiều chữ"). *(PDF mục 2)* — **XONG (S27):** thẻ `/job-orders` viết lại theo icon (ngành/chỉ tiêu/lương) + hover nâng nhẹ, gộp chung với 1.2.
- [x] **1.5 (tùy chọn) Khung trang "Hướng dẫn sử dụng"** — tạo trang rỗng có cấu trúc để điền nội dung sau. *(handwritten #7)* — **XONG (S27):** trang `/guide` (9 mục expansion theo module) + link NavMenu "Hướng dẫn sử dụng" cho mọi user.

---

## NHÓM 2 — CẦN THÔNG TIN/QUY TRÌNH ĐỐI TÁC (lưu, làm sau)
> Mỗi task kèm "**Cần XIN gì**". Lấy đủ info mới build để tránh làm lại.

### 2.1 Module Đào tạo (tiến độ học + kỷ luật + chuyên môn + điều kiện bay)
*(PDF 4A; handwritten #2,3,14)* — module mới, lớn nhất.
**Cần XIN:** chương trình học gồm bao nhiêu bài/buổi/môn, học mấy tháng, ở đâu; "mức học" đo bằng gì (điểm/cấp N5-N4/% hoàn thành); "kỷ luật" theo dõi gì (điểm danh/vi phạm/hạnh kiểm); **điều kiện cụ thể để được bay** (≥ bao nhiêu bài/điểm/đậu kỳ thi nào); ai nhập tiến độ & bao lâu/lần.

### 2.2 Cổng Phụ huynh (+ Đại lý cùng theo dõi) — "bắt buộc tải App"
*(PDF 4A; handwritten #4,28)*
**Cần XIN:** phụ huynh/đại lý được **xem gì** (tiến độ học/tình trạng/công nợ/hình ảnh), **không** cho xem gì; **đăng nhập kiểu nào** (tài khoản riêng / SĐT + OTP / link riêng); 1 phụ huynh xem 1 hay nhiều con; có nhận thông báo không & kênh nào (SMS/Zalo).

### 2.3 Theo dõi rủi ro (tệ nạn/nợ nần) → báo phụ huynh
*(handwritten #5)*
**Cần XIN:** các trạng thái rủi ro cần theo dõi (vi phạm/nợ/sức khỏe/bỏ học); ngưỡng tự động báo phụ huynh; ai đánh dấu (quản lý KTX/đào tạo).

### 2.4 Theo dõi HẬU xuất cảnh + chăm sóc + cập nhật người nhà (Call/SMS)
*(PDF 4A; handwritten #8,22)*
**Cần XIN:** sau khi bay theo dõi gì (công việc/sức khỏe/liên lạc định kỳ); các mức tình trạng (Ổn/Cần hỗ trợ/Có vấn đề…); cập nhật bao lâu/lần & ai; cập nhật cho người nhà qua kênh nào.

### 2.5 Hoa hồng ĐA CẤP (tối đa 3 nấc) + sơ đồ nhánh + mã số CTV
*(PDF 4B; handwritten #6)* — hiện hệ thống chỉ hoa hồng **1 cấp** cho mỗi đại lý → nâng cấp lớn.
**Cần XIN:** chính sách hoa hồng từng nấc (ai được bao nhiêu ở nấc 1/2/3); mốc chi trả (đóng phí/đặt cọc/trúng tuyển/bay) gắn điều kiện tiến độ-tệ nạn ra sao; cách hình thành "tuyến" giới thiệu; CTV được xem gì; CTV có tự nhập lead/ứng viên không (lưu ý: đại lý **không được chốt deal** vì pháp lý).

### 2.6 AI phân tích hồ sơ ứng viên + khai báo khi dùng App
*(PDF mục 2,3)*
**Cần XIN:** AI cần làm gì cụ thể (chấm điểm hồ sơ? gợi ý đơn hàng phù hợp? trích xuất thông tin từ CV?); **đầu vào** là gì (CV PDF/ảnh/form). **Cần quyết định nội bộ:** dùng **Claude API** (có phí theo lượng dùng) → cần API key + ngân sách.

### 2.7 Nhập dữ liệu CV/Profile sẵn có từ VietGroup
*(PDF mục 3)*
**Cần XIN:** file/CSDL hiện có ở **định dạng nào** (Excel/CSV/CRM/Google Sheet…), các trường gì; **đầu mối IT** của VietGroup để lập group đối chiếu luồng dữ liệu.

### 2.8 Inbound MKT — thu lead tự động
*(handwritten #13,27)*
**Cần XIN:** lead đến từ kênh nào (Facebook form/TikTok/Zalo/landing/hotline); muốn tự động đổ vào hệ thống hay nhập tay; nếu tự động cần tài khoản/API kênh đó.

### 2.9 Đăng JD công khai / tạo nguồn tuyển
*(handwritten #10,24)*
**Cần XIN:** có muốn đăng tin tuyển ra **công khai** (web/landing/Facebook) hay chỉ nội bộ; nếu công khai hiển thị trường nào, đăng ở đâu.

### 2.10 Hình ảnh ứng viên thành công cho MKT / cộng đồng CSKH
*(PDF 4A; handwritten #9,23)*
**Cần XIN:** trưng bày ở đâu (công khai/nội bộ); kèm thông tin gì (tên/nước/ngành/lời chứng thực); đã **xin phép** ứng viên dùng hình chưa (đồng ý/bảo mật).

### 2.11 Mảng Lao động Nội địa (Hà Nội 300, Cần Giờ 150)
*(PDF mục 5)* — domain khác hẳn XKLĐ.
**Cần XIN:** có đưa vào **cùng app này** không hay tách riêng; quy trình nội địa, ký quỹ tính sao, các gói kèm (đưa đón/ăn uống); tác động luật sau 1/7 (bỏ giấy phép con) tới nghiệp vụ.

### 2.12 App di động "bắt buộc tải"
*(PDF mục 2; 4A)* — hiện là web responsive.
**Cần quyết định:** làm **PWA** (cài từ trình duyệt, rẻ/nhanh, tái dùng web) hay **app native** (lên CH Play/App Store, tốn kém/lâu hơn). Ảnh hưởng lộ trình lớn.

### 2.13 Social ứng viên — link MXH trong Lead/Candidate
*(handwritten #12)* — chuyển từ NHÓM 1 xuống (25/6, theo góp ý user): **chưa chốt cần nền tảng MXH nào**, không nên đổ hết tất cả nền tảng vào hồ sơ.
**Cần XIN:** thực tế VietGroup theo dõi ứng viên qua kênh MXH nào (Facebook/Zalo/TikTok/Instagram/khác); chỉ thêm các trường thật sự dùng; có cần nút mở nhanh ở trang chi tiết không. Build nhanh sau khi chốt danh sách kênh.

---

## NHÓM 3 — TẦM NHÌN XA (ghi nhận, chưa scope)
*(PDF mục 6)* — TMĐT, Vay vốn, Bảo hiểm, dự án cộng đồng riêng, khai thác Big Data.
Làm sau khi hệ thống nội bộ ổn định.
