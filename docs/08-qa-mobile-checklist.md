# Checklist QA giao diện — POLYMIND (mobile-first)

> Cách kiểm: mở Chrome → `F12` → biểu tượng điện thoại (`Ctrl+Shift+M`) → chọn lần lượt **390px** (điện thoại), **820px** (tablet), **1440px** (laptop). Đăng nhập `admin@polymind.local / Admin@123`. Tick ✅ nếu đạt; chỗ nào lỗi **chụp màn hình gửi lại** để sửa.

## 0. Tổng thể (mọi trang)
- [ ] Nền trắng/tươi sáng, tông xanh thương hiệu; mặc định mở ra **chế độ sáng**.
- [ ] **Footer** 2 dòng "© 2026 … POLYMIND x VIETGROUP / Đồng Hành Cùng Bạn Vươn Xa" xuất hiện cuối **mọi trang** (kể cả /login); chữ co lại trên điện thoại, không tràn.
- [ ] Tiêu đề trang lớn + nhãn nhỏ IN HOA (eyebrow) phía trên; không bị tràn ngang.
- [ ] Thanh trên (app bar): logo "PM" + chữ POLYMIND + chuông + menu user không chen/đè nhau ở 390px.
- [ ] Menu trái: ở điện thoại là overlay, tự đóng sau khi chọn; ở laptop mở sẵn.

## 1. /login
- [ ] 390px: hero navy ở trên, thẻ trắng đè nhẹ bên dưới; nút "Đăng nhập →" xanh, đầy đủ.
- [ ] 1440px: hero và thẻ nằm **cạnh nhau** (2 cột), cân đối.
- [ ] Nút hiện/ẩn mật khẩu hoạt động; ô nhập bo tròn, focus viền xanh.

## 2. / (Tổng quan)
- [ ] Thẻ KPI: 390px **2 thẻ/hàng**, số tiền lớn không tràn ô; laptop nhiều thẻ/hàng.
- [ ] Biểu đồ/bảng doanh thu, top đại lý không tràn ngang trang.

## 3. /job-orders (Đơn hàng) + chi tiết
- [ ] Thẻ đơn hàng: cờ + tên nước + chip "Đang tuyển" **không tràn** ở 390px (được phép xuống dòng).
- [ ] Đơn đang tuyển nổi bật (viền trái xanh) và nằm đầu danh sách.
- [ ] Mở 1 đơn: header có cờ; bảng ứng viên cuộn ngang được, không kéo giãn cả trang.

## 4. /candidates + chi tiết ứng viên
- [ ] Danh sách: 390px hiển thị dạng **thẻ** (không phải bảng tràn).
- [ ] Chi tiết: các khối Thông tin/Giấy tờ/**Cha-Mẹ-Người bảo hộ**/Ngân hàng đọc tốt; bảng Công nợ & Hồ sơ cuộn ngang gọn.
- [ ] Nút "Sửa thông tin", dialog 5 tab mở ở 390px không tràn.

## 5. /finance, /agents, /visa, /admin, /notifications
- [ ] 390px: bảng dài chuyển thành **thẻ** (hoặc cuộn ngang gọn), không vỡ layout.
- [ ] Nút hành động ở header không che tiêu đề; menu ⋮ trên mobile hoạt động.

## 6. /reports
- [ ] Nút Excel/PDF/CSV bấm được; bảng số liệu cuộn ngang, không tràn trang.

## 7. /guide (Hướng dẫn)
- [ ] 9 mục mở/đóng được; nội dung không tràn ở 390px.

## 8. /my-commissions (đăng nhập agent@polymind.local)
- [ ] Bảng hoa hồng cuộn gọn; KPI hiển thị đúng.

## 9. Chế độ tối (nút mặt trăng góc phải)
- [ ] Bật tối: chữ/viền/thẻ vẫn đọc rõ, không "chìm"; tắt lại về sáng OK.
