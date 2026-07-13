# M08 — Training · 03 Test Cases

> QA: Claude · 2026-07-10. Quy ước `TC_M08_<n>`. Automation Layer: `unit` (xUnit Domain/Infra) · `bunit` (component — chưa có harness) · `e2e` (Playwright — chưa có harness) · `manual`.
> **Trạng thái test:** phần lớn **Blocked (pending harness)** vì logic nằm trong razor/Web (test project KHÔNG ref Web) — xem `05-automation-report.md`. Verify code-level = Pass ở source; runtime pending.

## Functional — tiến trình & phiếu

| TC | Tên | Flow | Priority | Sev nếu fail | Role | Preconditions | Steps | Expected UI | Expected DB | Automation | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|
| TC_M08_001 | List hiện ứng viên đã đặt cọc | BF-01 | High | Med | consultant | có ứng viên B5+ | mở `/training` | ứng viên xuất hiện, danh xưng đúng | — | e2e/manual | Verified(code) |
| TC_M08_002 | List loại ứng viên chưa đặt cọc & không record/phiếu | BF-01 | High | Med | consultant | ứng viên B1..B4, không record | mở `/training` | không xuất hiện | — | e2e/manual | Verified(code) |
| TC_M08_003 | Ứng viên đang Orientation vẫn hiện | BF-01 | Med | Low | consultant | step=Orientation | mở `/training` | xuất hiện | — | e2e/manual | Verified(code) |
| TC_M08_004 | Overall = TB các mảng enrolled | BF-01 | High | Med | consultant | lang=40 (enrolled), voc=không học | mở `/training` | tiến độ chung = 40% | — | manual | Verified(code) |
| TC_M08_005 | Overall bỏ mảng không enrolled | BF-04/01 | Med | Med | consultant | lang enrolled=60, voc IsEnrolled=false | xem list | chung=60 (không tính voc) | — | manual | Verified(code) |
| TC_M08_006 | Search theo tên/mã | BF-01 | Low | Low | consultant | ≥2 ứng viên | gõ tên | lọc đúng, không phân biệt hoa thường | — | e2e | Blocked(harness) |
| TC_M08_007 | Tạo record mảng lần đầu | BF-04 | High | High | consultant | chưa có record | Edit mảng → progress=30 → Lưu | snackbar success, progress 30% | insert training_records, CreatedBy=actor, audit create | bunit/manual | Verified(code) |
| TC_M08_008 | Cập nhật record đã có | BF-04 | High | Med | RM | record progress=30 | Edit → 80 → Lưu | 80% | update, UpdatedAt mới, audit update | bunit/manual | Verified(code) |
| TC_M08_009 | Progress clamp trên (>100) | BF-04 | High | Med | consultant | — | nhập 150 → Lưu | lưu 100 | progress_percent=100 | unit(logic)/manual | Verified(code) — `Math.Clamp` |
| TC_M08_010 | Progress clamp dưới (<0) | BF-04 | High | Med | consultant | — | nhập -5 → Lưu | lưu 0 | progress_percent=0 | unit(logic)/manual | Verified(code) |
| TC_M08_011 | Tắt "Có học" mảng | BF-04 | Med | Low | consultant | record enrolled | tắt switch → Lưu | mảng "Chưa học / đơn hàng không yêu cầu" | is_enrolled=false | bunit/manual | Verified(code) |
| TC_M08_012 | Level/note trim & null | BF-04 | Low | Low | consultant | — | nhập "  " vào level → Lưu | level = null | level_label NULL | unit(logic)/manual | Verified(code) |
| TC_M08_013 | Thêm phiếu đánh giá không đính kèm | BF-05 | High | High | consultant | ứng viên B5+ | Thêm phiếu → 4 rating → Lưu | phiếu vào timeline tuần | insert training_evaluations, AttachmentsJson NULL, audit create | bunit/manual | Verified(code) |
| TC_M08_014 | Thêm phiếu có đính kèm ảnh/PDF | BF-05 | High | Med | consultant | — | chọn 2 tệp → Lưu | link đính kèm hiển thị | AttachmentsJson=list 2 object key | e2e/manual | Blocked(harness+MinIO) |
| TC_M08_015 | Ngày đánh giá tương lai | BF-05 | Low | Low | consultant | — | ngày = +7d → Lưu | phiếu lưu, lên đầu timeline | evaluation_date tương lai | manual | **Needs Req Clarification (U-M08-2)** |
| TC_M08_016 | Phiếu gộp theo tuần Monday-based | BF-05 | Med | Low | consultant | phiếu 2 ngày trong 1 tuần | xem detail | 1 nhóm tuần, 2 báo cáo | — | unit(logic)/manual | Verified(code) — `WeekStart` |
| TC_M08_017 | Nhiều phiếu cùng ngày cho phép | BF-05 | Med | Low | consultant | — | tạo 2 phiếu cùng ngày | cả 2 hiển thị | 2 rows (không unique) | manual | Verified(code) |

## Authentication & Authorization

| TC | Tên | Flow | Priority | Role | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|
| TC_M08_018 | Chưa đăng nhập → chặn | BF-01 | High | anonymous | mở `/training` | redirect login | e2e | Blocked(harness) |
| TC_M08_019 | Read-only role không có nút sửa/thêm | BF-03 | High | agent/collaborator | mở `/training/{id}` | không có nút Edit/Thêm phiếu | e2e/manual | Verified(code) |
| TC_M08_020 | Các bộ phận liên quan có quyền xem nhưng không sửa đào tạo | BF-01 | High | recruiter/document/visa/accountant | kiểm seed + đăng nhập | có `training:read`; không có create/update/delete/approve | unit + e2e | **Pass unit 4 role; runtime UI pending — CR-M08-1** |
| TC_M08_021 | Server re-check update khi thiếu quyền | BF-04 | High | agent | (giả lập) gọi Save track | snackbar "không có quyền", không ghi DB | bunit | Blocked(harness) |
| TC_M08_022 | Server re-check create khi thiếu quyền | BF-05 | High | collaborator | (giả lập) gọi Save phiếu | snackbar "không có quyền", không ghi DB | bunit | Blocked(harness) |
| TC_M08_023 | IDOR self-scoped xem ứng viên khác | BF-02 | High | student | mở `/training/{idKhác}` | "Không tìm thấy / không có quyền" | e2e/manual | Verified(code) |
| TC_M08_024 | IDOR agent xem ứng viên agent khác | BF-03 | High | agent | mở `/training/{idNgoàiScope}` | `_found=false` | e2e/manual | Verified(code) |
| TC_M08_025 | super_admin toàn quyền training | BF-04/05 | Med | super_admin | Edit + thêm phiếu | thành công | e2e/manual | Verified(code) — seed all perms |

## Boundary & Input

| TC | Tên | Priority | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|
| TC_M08_026 | Note Unicode/tiếng Việt dấu/emoji | Low | note = "Tiến bộ 👍 N4→N3" | lưu nguyên văn | manual | Verified(code) |
| TC_M08_027 | Note dài (>vài nghìn ký tự) | Low | note dài | lưu (cột text) | manual | Blocked(harness) |
| TC_M08_028 | Đính kèm >10 tệp | Low | chọn 12 tệp | chỉ nhận 10 (`maximumFileCount:10`) | manual | Verified(code) |
| TC_M08_029 | Rating enum đủ 4 mức | Low | mở select | Weak/Average/Good/Excellent | unit | **Automatable** (xem 05) |
| TC_M08_030 | Track enum đủ 2 mảng | Low | — | Language/Vocational | unit | **Automatable** (xem 05) |

## Concurrency & Database

| TC | Tên | Priority | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|
| TC_M08_031 | 2 người cùng tạo record 1 mảng | Med | 2 phiên Save cùng (candidate,track) | 1 thành công; người 2 nên báo lỗi thân thiện | e2e/integration | **Blocked** → OBS-M08-01 (hiện DbUpdateException chưa bắt) |
| TC_M08_032 | Unique index chặn trùng mảng | Med | insert 2 record cùng (candidate,track) | vi phạm unique | integration(DB) | Blocked(harness) — verify code: index IsUnique |
| TC_M08_033 | Audit ghi cho mỗi save | Med | Save track + phiếu | 2 audit rows resource="training" | integration(DB) | Blocked(harness) — verify code: AddAudit gọi |
| TC_M08_034 | JSON đính kèm hỏng không crash | Med | AttachmentsJson = "{bad" | detail vẫn render, bỏ đính kèm | bunit | Verified(code) — try/catch |

## Coverage note

- **Verified(code):** logic đọc trực tiếp source, khẳng định đúng ở mức code (authorization gate, clamp, attribution, scope, audit, week-grouping).
- **Blocked(harness):** cần bUnit (component) + WebApplicationFactory/DB test + MinIO cho runtime — chưa dựng (blocker chung repo).
- **Automatable ngay:** TC_M08_029/030 (enum shape) — thêm ở phiên này (`M08_TrainingRulesTests`).
