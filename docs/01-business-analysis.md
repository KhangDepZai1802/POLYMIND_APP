# Phân Tích Nghiệp Vụ — POLYMIND Overseas Labor Management System

## 1. Tổng Quan Hệ Thống

**Tên sản phẩm:** POLYMIND — Web App Quản Lý Xuất Khẩu Lao Động  
**Mục tiêu:** Quản lý tập trung toàn bộ quy trình từ khi phát sinh Lead đến khi ứng viên xuất cảnh thành công.

---

## 2. Các Actor & Phân Quyền

| Role | Mô tả | Quyền chính |
|---|---|---|
| Super Admin | Quyền cao nhất | Toàn bộ dữ liệu, tài khoản, phân quyền |
| Giám đốc | Quản lý cấp cao | Xem dashboard, phê duyệt thanh toán & hoa hồng |
| Trưởng phòng tuyển dụng | Quản lý nhân viên | Quản lý lead, phân công, theo dõi KPI |
| Nhân viên tuyển dụng | Xử lý lead | Tạo/chăm sóc lead, chuyển đổi ứng viên |
| Bộ phận Hồ sơ | Xử lý giấy tờ | Quản lý hồ sơ, giấy tờ, tiến độ |
| Bộ phận Visa | Xử lý visa | Hồ sơ visa, lịch phỏng vấn, kết quả |
| Kế toán | Quản lý tài chính | Thu chi, công nợ, hoa hồng |
| Đại lý / CTV | Portal riêng | Xem ứng viên đã giới thiệu, hoa hồng, thanh toán |

---

## 3. Phân Tích Các Module

### Module 1: Quản Lý Lead (CRM)

**Nguồn Lead:**
- Online: Facebook Ads, TikTok Ads, Google Ads, Website, Landing Page, Zalo
- Offline: Hotline, Đại lý, Giới thiệu, Offline Event

**Vòng đời trạng thái Lead:**
```
Lead mới → Chưa liên hệ → Đã liên hệ → Quan tâm → Hẹn tư vấn 
→ Đang tư vấn → Đăng ký → Chuyển thành ứng viên
                                    ↓
                          Không phù hợp / Hủy
```

**Chức năng CRM cần có:**
- Lịch sử cuộc gọi, chăm sóc, ghi chú
- File đính kèm
- Nhắc lịch tự động
- Gửi SMS / Email / Zalo OA
- Theo dõi KPI chuyển đổi

---

### Module 2: Quản Lý Ứng Viên

**Thông tin cá nhân:** CCCD, Hộ chiếu, tình trạng hôn nhân, người thân liên hệ, thông tin ngân hàng

**Hồ sơ đính kèm** (PDF, JPG, PNG, DOCX):
- CCCD, Hộ chiếu, Sổ hộ khẩu, Giấy khai sinh
- Bằng cấp, Chứng chỉ
- Giấy khám sức khỏe, Ảnh thẻ, Hồ sơ tư pháp, Hợp đồng

**Versioning hồ sơ:** Lưu lịch sử chỉnh sửa (người, thời gian, khôi phục)

---

### Module 3: Quản Lý Đơn Hàng Tuyển Dụng

Mỗi đơn hàng gắn với: Quốc gia, Nghiệp đoàn, Công ty tiếp nhận, Ngành nghề, Số lượng, Lương, Chi phí, Điều kiện, Ngày tuyển, Ngày xuất cảnh dự kiến

**Trạng thái đơn hàng:** Đang tuyển → Đủ hồ sơ → Đang phỏng vấn → Đã chốt → Đóng đơn

---

### Module 4: Tài Chính

**Khoản thu từ ứng viên:**
- Tiền đặt cọc, Phí hồ sơ, Phí đào tạo, Phí Visa, Phí dịch vụ, Thu khác

**Khoản chi:**
- Chi marketing, Chi đối tác, Chi xử lý hồ sơ, Chi Visa, Chi đào tạo, Chi hoàn tiền

**Công nợ:** Tổng phí phải thu / Đã thanh toán / Còn nợ / Hạn thanh toán

**Phiếu thu chi:** Sinh mã tự động, In PDF, Ký điện tử, Lưu lịch sử

---

### Module 5: Hoa Hồng Đại Lý

**Cơ chế:** Cấu hình theo Quốc gia / Đơn hàng / Mức phí / Số lượng ứng viên

**Mốc thanh toán tự động (ví dụ):**
- 20% khi ứng viên đặt cọc
- 30% khi trúng tuyển
- 50% khi xuất cảnh

---

### Module 6: Visa & Xuất Cảnh

**Vòng đời Visa:** Chưa nộp → Đang chuẩn bị → Đã nộp → Yêu cầu bổ sung → Đã cấp / Từ chối

**Xuất cảnh:** Quản lý vé máy bay (hãng, mã vé, ngày/giờ bay), Sân bay, Quốc gia đến

---

### Module 7: Dashboard & Báo Cáo

| Dashboard | Nội dung |
|---|---|
| Tổng quan | Lead mới, ứng viên đang xử lý, xuất cảnh, tỷ lệ chuyển đổi |
| Doanh thu | Theo tháng, quốc gia, đơn hàng |
| Đại lý | Top đại lý, hoa hồng phải trả/đã trả |

**Báo cáo xuất được:** Excel, PDF

---

### Module 8: Thông Báo Tự Động

| Sự kiện | Kênh |
|---|---|
| Nhắc nộp hồ sơ | Email, SMS, Zalo OA, In-app |
| Nhắc thanh toán | Email, SMS, Zalo OA, In-app |
| Nhắc lịch phỏng vấn / Visa / Xuất cảnh | Email, SMS, Zalo OA, In-app |
| Nhắc thanh toán hoa hồng | Email, SMS, Zalo OA, In-app |

---

## 4. Yêu Cầu Phi Chức Năng

- **Bảo mật:** JWT Authentication, RBAC Permission, Audit Log đầy đủ
- **API:** REST API (giai đoạn 1), có thể mở rộng GraphQL
- **Frontend:** Responsive Web, Dashboard Realtime, DataTable có filter
- **Export:** Excel & PDF
- **Tích hợp giai đoạn 2:** Facebook/TikTok Lead API, Google Ads, Zalo OA, Call Center, AI Chatbot, OCR CCCD/Passport, Chữ ký số, Mobile App, BI Dashboard, AI dự đoán

---

## 5. Độ Ưu Tiên Triển Khai (Gợi Ý)

### Phase 1 — Core (MVP)
1. Auth & RBAC
2. Quản lý Lead (CRM cơ bản)
3. Quản lý Ứng viên & Hồ sơ
4. Quản lý Đơn hàng
5. Workflow tiến độ ứng viên
6. Dashboard tổng quan

### Phase 2 — Finance & Commission
7. Tài chính (thu/chi, công nợ, phiếu thu chi PDF)
8. Hoa hồng đại lý
9. Portal đại lý

### Phase 3 — Visa, Departure & Reports
10. Module Visa & Xuất cảnh
11. Báo cáo đầy đủ (Excel, PDF)
12. Thông báo tự động (Email, SMS, Zalo)

### Phase 4 — Integrations
13. Tích hợp Lead API (Facebook, TikTok, Google)
14. OCR CCCD/Passport
15. AI & Mobile App
