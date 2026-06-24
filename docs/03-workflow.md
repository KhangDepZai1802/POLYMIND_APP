# Workflow Hệ Thống — POLYMIND OLMS

## 1. Workflow Vòng Đời Lead → Ứng Viên

```
┌─────────────────────────────────────────────────────────────────────┐
│                        LEAD LIFECYCLE                               │
│                                                                     │
│  [Nguồn Lead] ──→ Lead mới                                         │
│     FB/TikTok        │                                              │
│     Google/Zalo      ▼                                              │
│     Hotline     Chưa liên hệ ──→ Không phù hợp (exit)             │
│     Đại lý           │                                              │
│     Giới thiệu       ▼                                              │
│                  Đã liên hệ ──→ Hủy (exit)                         │
│                      │                                              │
│                      ▼                                              │
│                  Quan tâm                                           │
│                      │                                              │
│                      ▼                                              │
│                  Hẹn tư vấn                                        │
│                      │                                              │
│                      ▼                                              │
│                  Đang tư vấn                                        │
│                      │                                              │
│               ┌──────┴──────┐                                       │
│               ▼             ▼                                       │
│           Đăng ký    Không phù hợp (exit)                          │
│               │                                                     │
│               ▼                                                     │
│    [Chuyển thành Ứng Viên] ──→ Tạo hồ sơ ứng viên                │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Workflow Tiến Độ Ứng Viên (17 Bước)

```
Bước 01 ──→ Bước 02 ──→ Bước 03 ──→ Bước 04 ──→ Bước 05
Lead mới   Tư vấn     Đăng ký     Đặt cọc     Hoàn thiện
                                  [HH: 20%]    hồ sơ
    │
    │
    ▼
Bước 06 ──→ Bước 07 ──→ Bước 08 ──→ Bước 09 ──→ Bước 10
Khám sức   Học định    Thi tuyển   Trúng tuyển  Ký hợp
khỏe       hướng                   [HH: 30%]    đồng
    │
    │
    ▼
Bước 11 ──→ Bước 12 ──→ Bước 13 ──→ Bước 14 ──→ Bước 15
Nộp hồ sơ  Đậu Visa    Thanh toán  Đặt vé      Xuất cảnh
Visa                    đủ          máy bay     [HH: 50%]
    │
    │
    ▼
Bước 16 ──→ Bước 17
Đến nơi    Hoàn tất
làm việc   hồ sơ ✓
```

**Ghi chú:**
- `[HH: X%]` = mốc tính hoa hồng đại lý
- Mỗi bước có thể quay về bước trước hoặc drop-out (hủy hồ sơ)
- Bước Visa có sub-flow riêng (xem mục 4)

---

## 3. Workflow Quản Lý Hoa Hồng Đại Lý

```
                    Ứng viên đặt cọc (Bước 04)
                              │
                              ▼
              [Hệ thống tự tính HH = 20% × base_amount]
                              │
                              ▼
                    agent_commissions.status = 'pending'
                              │
                              ▼
                       Kế toán review
                    ┌─────────┴──────────┐
                    ▼                    ▼
                Approve              Reject/Adjust
                    │
                    ▼
              agent_commissions.status = 'approved'
                              │
                              ▼
                    Giám đốc phê duyệt thanh toán
                              │
                              ▼
                    Kế toán thực hiện chuyển khoản
                              │
                              ▼
              agent_commissions.status = 'paid'
              receipts record tạo ra
              Thông báo đến đại lý
```

---

## 4. Sub-flow Visa

```
Bộ phận Visa chuẩn bị hồ sơ
            │
            ▼
    Nộp hồ sơ Visa  ──→  visa.status = 'submitted'
            │
            ├──────────────────────────────────────┐
            ▼                                      ▼
  Yêu cầu bổ sung                         Kết quả trong X ngày
  visa.status =                         ┌────────┴────────┐
  'additional_required'                 ▼                 ▼
            │                      Đậu Visa         Từ chối
            ▼                      'approved'        'rejected'
  Bổ sung & nộp lại                     │                │
                                        ▼                ▼
                               Bước 12 ✓        Tư vấn lại / Hủy
                               Chuyển sang        hoặc xin Visa
                               Thanh toán đủ      lại sau
```

---

## 5. Workflow Thông Báo Tự Động

### Trigger Events & Actions

| Sự kiện | Delay | Kênh | Người nhận |
|---|---|---|---|
| Lead chưa được liên hệ sau 24h | 24h | In-app + Email | Nhân viên phụ trách |
| Lead hẹn tư vấn T-1 ngày | 1 ngày trước | SMS + Zalo | Lead + Nhân viên |
| Hồ sơ thiếu tài liệu quá 7 ngày | 7 ngày | Email + SMS | Ứng viên + Bộ phận HS |
| Khoản thanh toán sắp đến hạn | 3 ngày trước | SMS + Email | Ứng viên |
| Khoản thanh toán quá hạn | Ngay lập tức | SMS + Email + In-app | Ứng viên + Kế toán |
| Lịch phỏng vấn T-1 ngày | 1 ngày trước | SMS + Zalo + Email | Ứng viên + BP Visa |
| Lịch phỏng vấn T-2 giờ | 2 giờ trước | SMS | Ứng viên |
| Lịch xuất cảnh T-3 ngày | 3 ngày trước | SMS + Email + Zalo | Ứng viên + Nhân viên |
| Hoa hồng đại lý được duyệt | Ngay | Email + In-app | Đại lý |
| Hoa hồng được thanh toán | Ngay | Email + SMS + In-app | Đại lý |

---

## 6. Workflow Tài Chính — Thu Tiền Ứng Viên

```
Kế toán tạo khoản thu (payments)
            │
            ▼
     payments.status = 'pending'
            │
            ▼
   Hệ thống gửi thông báo đến ứng viên
            │
            ▼
      Ứng viên nộp tiền
            │
            ▼
    Kế toán xác nhận + nhập ngày thanh toán
            │
            ▼
     payments.status = 'paid'
            │
            ├──→ Tạo receipt (phiếu thu)
            ├──→ Cập nhật công nợ ứng viên
            └──→ Kiểm tra mốc trigger hoa hồng
```

---

## 7. Portal Đại Lý — Workflow Truy Cập

```
Đại lý đăng nhập Portal
            │
            ▼
   Dashboard cá nhân:
   - Danh sách ứng viên đã giới thiệu
   - Trạng thái từng ứng viên trong workflow
   - Hoa hồng phát sinh / đã duyệt / đã trả
   - Công nợ chi tiết
            │
            ▼
   Không có quyền chỉnh sửa dữ liệu
   Chỉ xem và xuất báo cáo
```

---

## 8. Phân Công Nhiệm Vụ Theo Bước

| Bước | Bộ phận phụ trách |
|---|---|
| Lead mới → Đăng ký | Nhân viên tuyển dụng |
| Đặt cọc → Thanh toán | Kế toán |
| Hoàn thiện hồ sơ | Bộ phận Hồ sơ |
| Khám sức khỏe | Bộ phận Hồ sơ |
| Học định hướng | Bộ phận Hồ sơ |
| Thi tuyển → Trúng tuyển | Bộ phận Hồ sơ + Nhân viên tuyển dụng |
| Ký hợp đồng | Kế toán + Bộ phận Hồ sơ |
| Nộp hồ sơ Visa → Đậu Visa | Bộ phận Visa |
| Đặt vé → Xuất cảnh | Bộ phận Visa + Nhân viên phụ trách |

---

## 9. KPI Tracking

| KPI | Cách tính |
|---|---|
| Tỷ lệ chuyển đổi Lead → Ứng viên | (leads status=converted / total leads) × 100 |
| Tỷ lệ trúng tuyển | (candidates step=selected / total candidates) × 100 |
| Tỷ lệ đậu Visa | (visa status=approved / total submitted) × 100 |
| Tỷ lệ xuất cảnh thành công | (step=departure / total selected) × 100 |
| Thời gian trung bình xử lý hồ sơ | AVG(completed_at - deposit_date) per candidate |
| KPI nhân viên | Lead mới / Tư vấn / Chuyển đổi per recruiter per month |
