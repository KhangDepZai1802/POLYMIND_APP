# M09 — Agents & Commissions · 03 Test Cases

> QA: Claude · 2026-07-10. `TC_M09_<n>`. Layer: unit / bunit / e2e / integration / manual. Nhiều TC **Blocked(harness)** vì logic ở razor/Web.

## Commission engine (idempotency & tính tiền) — CỐT LÕI

| TC | Tên | Flow | Priority | Sev | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|
| TC_M09_001 | Sinh hoa hồng khi stage Paid | BF-01 | High | High | payment Deposit=Paid → EnsureAsync | 1 commission Deposit Pending, amount=%×cost | integration | Blocked(harness) — verify(code) |
| TC_M09_002 | Idempotent gọi lặp (tuần tự) | BF-01 | High | High | EnsureAsync 2 lần cùng stage | chỉ 1 commission | integration | **Verified(code)** — `exists` AnyAsync |
| TC_M09_003 | **Idempotency dưới concurrency** | BF-01 | High | High | 2 EnsureAsync đồng thời (advance+pay) cùng ứng viên | phải chỉ 1 commission/mốc | integration(parallel) | **FAIL(code-level) → BUG_M09_01** (không unique index) |
| TC_M09_004 | Đổi đơn KHÔNG regenerate mốc đã hưởng (U2) | BF-01 | High | High | commission Deposit tồn tại → đổi job → pay lại | không tạo trùng, không hoàn | integration | **Verified(code)** — exists guard |
| TC_M09_005 | Amount theo % config | BF-01 | High | Med | config 1%, cost=100tr | amount=1tr | integration | Blocked — verify(code) |
| TC_M09_006 | Amount fixed khi không % | BF-01 | Med | Med | config fixed=500k | amount=500k | integration | Blocked — verify(code) |
| TC_M09_007 | Config chọn khớp nhất (đơn>quốc gia>chung) | BF-01 | Med | Med | 2 config | chọn config đơn khớp | integration | **Verified(code)** — OrderByDescending |
| TC_M09_008 | Không config → không commission | BF-01 | Med | Low | agent không config | tạo 0 | integration | Verified(code) |
| TC_M09_009 | CostAmount null → amount 0 | BF-01 | Low | Low | cost null, % config | amount 0 | integration | Verified(code) |
| TC_M09_010 | Rate constant 1/1.5/2.5=5% | — | High | Med | — | hằng số đúng | **unit** | **Pass (M09_CommissionRatesTests)** |

## Approve / Pay (state machine)

| TC | Tên | Flow | Priority | Role | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|
| TC_M09_011 | Duyệt hoa hồng Pending | BF-02 | High | accountant | nút Duyệt | Approved, ApprovedBy=actor, audit | bunit/e2e | Verified(code) |
| TC_M09_012 | Chi hoa hồng Approved | BF-03 | High | accountant | nút Đã chi | Paid, PaidDate, audit | bunit/e2e | Verified(code) |
| TC_M09_013 | Director duyệt nhưng KHÔNG chi | BF-02/03 | High | director | duyệt OK; không thấy nút chi | approve OK, pay ẩn | e2e | Verified(code) — perms |
| TC_M09_014 | Thiếu quyền duyệt bị chặn server | BF-02 | High | recruiter | (giả lập) ApproveCommission | snackbar, không đổi | bunit | Blocked(harness) |
| TC_M09_015 | **Stale-UI revert Paid→Approved** | BF-02 | Med | 2 admin | admin B duyệt commission đã Paid (UI cũ) | KHÔNG được revert | integration(concurrent) | **FAIL(code-level) → BUG_M09_02** (không guard status) |
| TC_M09_016 | **Chi khi chưa duyệt (UI stale)** | BF-03 | Med | accountant | MarkPaid trên Pending | KHÔNG được Paid nếu chưa Approved | integration | **FAIL(code-level) → BUG_M09_02** |

## Config & Collaborator

| TC | Tên | Priority | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|
| TC_M09_017 | Config yêu cầu % hoặc fixed | Med | Save trống cả 2 | snackbar cảnh báo | bunit | Verified(code) |
| TC_M09_018 | % ưu tiên hơn fixed | Med | nhập cả 2 | fixed=null lưu % | unit(logic)/manual | Verified(code) — ApplyTo |
| TC_M09_019 | Config re-check agents:update | High | thiếu quyền Save | snackbar, không lưu | bunit | Blocked(harness) |
| TC_M09_020 | CTV share clamp 30-40 | High | nhập 50 → Save | lưu 40 | unit(logic)/manual | Verified(code) — ClampPercentage |
| TC_M09_021 | CTV share clamp dưới (20) | Med | nhập 20 | lưu 30 | manual | Verified(code) |
| TC_M09_022 | CTV FullName bắt buộc | Med | trống tên | snackbar | bunit | Verified(code) |
| TC_M09_023 | CTV re-check collaborators perm | High | agent sửa share | OK (agent có update) | e2e | Verified(code) |

## Authorization / IDOR / Portal

| TC | Tên | Priority | Role | Steps | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|
| TC_M09_024 | Partner-only bị chặn AgentDetail | High | agent | mở `/agents/{id}` | redirect `/agents` | e2e | Verified(code) |
| TC_M09_025 | CTV portal chỉ ứng viên mình | High | collaborator | `/my-commissions` | chỉ ứng viên CollaboratorId mình | e2e/integration | Verified(code) — filter |
| TC_M09_026 | CTV mask SĐT ứng viên | High | collaborator | xem bảng ứng viên | SĐT `••• ••• 123` | manual | Verified(code) — MaskPhone |
| TC_M09_027 | CTV không thấy bảng thi đua CTV | Med | collaborator | `/agents` | bảng CTV ẩn | e2e | Verified(code) |
| TC_M09_028 | Agent net = gross − CTV share | Med | agent | `/my-commissions` | agent còn lại đúng | unit(logic)/manual | Verified(code) |
| TC_M09_029 | Chưa gắn agent → cảnh báo scope | Low | agent chưa link | `/my-commissions` | thông báo liên hệ admin | e2e | Verified(code) |
| TC_M09_034 | % chia CTV lịch sử không đổi | High | agent/CTV | phát sinh 35%, đổi cấu hình CTV thành 40% | dòng cũ vẫn 35%; dòng mới 40% | unit + integration | **Pass rule/source; migration/runtime pending** |
| TC_M09_035 | Partner không thấy doanh số đối thủ | High | agent/CTV | mở `/agents` | chỉ thấy hàng đại lý mình với rank toàn cục; CTV chỉ trong đại lý mình; staff vẫn top đầy đủ | unit + e2e | **Pass visibility rule/source; runtime pending** |

## Contract (Domain) — Automated Pass

| TC | Tên | Automation | Status |
|---|---|---|---|
| TC_M09_030 | Rate 1/1.5/2.5 total 5 | unit | **Pass** |
| TC_M09_031 | CTV 30-40 default 35 | unit | **Pass** |
| TC_M09_032 | CTV mới default 35% | unit | **Pass** |
| TC_M09_033 | Commission mới = Pending | unit | **Pass** |

## Coverage note
- **Automated Pass:** 4 (contract hằng số hoa hồng).
- **Verified(code):** authz gate/re-check, IDOR portal, mask SĐT, clamp, config selection, U2 no-refund, idempotency tuần tự.
- **FAIL(code-level) → bug:** TC_M09_003 (BUG_M09_01), TC_M09_015/016 (BUG_M09_02) — cần integration parallel để runtime-repro (Blocked harness).
- **Blocked(harness):** integration engine + bUnit dialog + e2e portal.
