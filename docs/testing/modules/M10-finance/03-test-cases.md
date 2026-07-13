# M10 — Finance · 03 Test Cases

> QA: Claude · 2026-07-10. `TC_M10_<n>`. Layer: unit / bunit / e2e / integration / manual. Nhiều TC **Blocked(harness)** (logic ở razor/Web).

## Lịch đóng tiền & split

| TC | Tên | Flow | Priority | Sev | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|
| TC_M10_001 | Tạo lịch 4 bước từ chi phí | BF-01 | High | High | cost=100tr → tạo lịch | 4 Payment: 20/30/30/20tr | integration | Blocked — verify(code) |
| TC_M10_002 | Split bù dư bước cuối khớp tổng | BF-01 | High | Med | cost lẻ (VD 99.999.999) | tổng 4 bước = cost tuyệt đối | unit(logic)/manual | Verified(code) — Split |
| TC_M10_003 | Tạo lịch idempotent (bỏ bước đã có) | BF-01 | Med | Med | đã có 2 bước → tạo lịch | chỉ tạo 2 bước thiếu | integration | Verified(code) |
| TC_M10_004 | Đơn chưa nhập chi phí → chặn | BF-01 | Med | Low | cost=0 | cảnh báo, không tạo | manual | Verified(code) |
| TC_M10_005 | Số bước % đúng (20/30/30/20) | — | Med | Med | — | Percent(stage) đúng | unit(logic) | Verified(code) |

## Đóng tiền tuần tự & hoa hồng

| TC | Tên | Flow | Priority | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|
| TC_M10_006 | Đánh dấu bước 1 đã đóng | BF-02 | High | mark Deposit | Paid + hoa hồng Deposit phát sinh | integration | Blocked — verify(code) |
| TC_M10_007 | Ép tuần tự — chặn đóng bước 2 khi bước 1 chưa đóng | BF-02 | High | mark ServiceFee trước Deposit | cảnh báo "đóng 1→4" | integration | Verified(code) — siblings check |
| TC_M10_008 | Mark stage trigger CommissionEngine | BF-02 | High | mark Deposit | AgentCommission Deposit tạo | integration | Verified(code) — EnsureAsync |
| TC_M10_009 | **Duyệt stage qua tab Khoản thu KHÔNG trigger hoa hồng + bỏ tuần tự** | BF-03 | High | "Duyệt" trên stage Deposit ở tab Khoản thu | phải: giống MarkStagePaid (hoa hồng + tuần tự) | integration | **FAIL(code-level) → BUG_M10_01** |
| TC_M10_010 | **Edit Status=Paid bỏ qua hoa hồng/tuần tự** | BF-04 | High | dialog set stage Paid | phải trigger như đường chuẩn | integration | **FAIL(code-level) → BUG_M10_01** |
| TC_M10_011 | Bước cuối (Settlement) qua đường phụ → thiếu hoa hồng Departure | BF-03 | High | duyệt Settlement ở tab Khoản thu, không thao tác Tiến độ | hoa hồng Departure phải phát sinh | integration | **FAIL(code-level) → BUG_M10_01** |
| TC_M10_012 | Concurrency mark + advance → hoa hồng trùng | BF-02 | High | 2 EnsureAsync đồng thời | 1 commission/mốc | integration(parallel) | **Liên kết BUG_M09_01** |

## Khoản chi & phiếu

| TC | Tên | Flow | Priority | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|
| TC_M10_013 | Tạo khoản chi | BF-05 | Med | dialog số tiền>0, ngày | insert + audit | bunit | Verified(code) |
| TC_M10_014 | Khoản chi số tiền ≤0 chặn | BF-05 | Med | amount=0 | cảnh báo | bunit | Verified(code) |
| TC_M10_015 | Khoản chi KHÔNG có luồng duyệt | BF-05 | Low | tạo chi | ApprovedBy null (không UI duyệt) | manual | **Req U-M10-1 (OBS-M10-01)** |
| TC_M10_016 | Phiếu thu idempotent | BF-06 | Med | tạo phiếu 2 lần cho 1 payment | lần 2 báo "đã có" | integration | Verified(code) — AnyAsync |
| TC_M10_017 | Phiếu chỉ tạo khi payment Paid | BF-06 | Med | nút phiếu chỉ hiện khi Paid | ẩn khi chưa Paid | e2e | Verified(code) |
| TC_M10_018 | In PDF phiếu | BF-06 | Low | `/receipts/{id}.pdf` | trả PDF | e2e | Verified(code) — endpoint |

## Authorization / IDOR

| TC | Tên | Priority | Role | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|
| TC_M10_019 | Director KHÔNG ghi nhận khoản thu | High | director | nút Duyệt | ẩn (CanRecordPayment=false) | e2e | Verified(code) |
| TC_M10_020 | Recruiter không thấy tài chính | High | recruiter | `/finance` | 403/menu ẩn (không payments:read) | e2e | Verified(code) |
| TC_M10_021 | Self-scoped chỉ tiến độ của mình | High | parent/student | `/finance` | 1 ứng viên, không tab thu/chi/KPI/nút | e2e | Verified(code) |
| TC_M10_022 | IDOR self-scoped ứng viên khác | High | student | ép hiển thị ứng viên khác | lọc cứng OwnedCandidateId | integration | Verified(code) |
| TC_M10_023 | PDF phiếu gated receipts:read | High | parent | `/receipts/{id}.pdf` | 403 (không quyền) | e2e | Verified(code) |
| TC_M10_024 | Dialog re-check quyền | High | thiếu quyền | Save payment/expense | cảnh báo, không lưu | bunit | Blocked(harness) |

## Boundary & Data

| TC | Tên | Priority | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|
| TC_M10_025 | Payment amount ≤0 chặn | Med | amount=0 | cảnh báo | bunit | Verified(code) |
| TC_M10_026 | Chỉ tạo thu cho ứng viên đủ điều kiện | Med | ứng viên < Deposit | không trong dropdown / chặn | manual | Verified(code) |
| TC_M10_027 | Code trùng (random suffix) | Low | va Code cùng ngày | unique index → lỗi (chưa bắt) | integration | **OBS-M10-02** |
| TC_M10_028 | U2: reset đơn KHÔNG hoàn khoản thu | High | đổi đơn ứng viên có payment Paid | payment giữ nguyên, không Refunded | integration | **Verified(code)** — không refund logic |
| TC_M10_029 | Tổng KPI đúng (thu/còn/chi) | Low | — | tổng khớp | manual | Verified(code) |

## Contract (Domain) — Automated Pass

| TC | Tên | Automation | Status |
|---|---|---|---|
| TC_M10_030 | PaymentStage 1..4 đúng thứ tự | unit | **Pass** |
| TC_M10_031 | Payment mới = Pending | unit | **Pass** |
| TC_M10_032 | PaymentStatus đủ lifecycle | unit | **Pass** |
| TC_M10_033 | ReceiptType Income/Expense | unit | **Pass** |

## Coverage note
- **Automated Pass:** 4 (contract PaymentStage/Status/ReceiptType).
- **Verified(code):** split bù dư, tuần tự (MarkStagePaid), attribution, authz+IDOR+self-scope, PDF gate, receipt idempotent, U2 no-refund.
- **FAIL(code-level) → bug:** TC_M10_009/010/011 (BUG_M10_01); TC_M10_012 (liên kết BUG_M09_01).
- **Blocked(harness):** integration split/tuần tự/commission + bUnit dialog + e2e.
