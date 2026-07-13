# M10 — Finance · 04 Traceability

> QA: Claude · 2026-07-10.

| Business Flow | Page/Logic | Role | State | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|
| BF-M10-01 Tạo lịch | Finance.CreateSchedule / PaymentSchedule.Split | accountant/SA | →Pending×4 | TC_M10_001..005 | M10_FinanceRulesTests (stage order) | split verified(code); order Pass | integration split pending |
| BF-M10-02 Đánh dấu đóng | Finance.MarkStagePaid | accountant/SA | Pending→Paid | TC_M10_006,007,008,012 | — | Verified(code) tuần tự+trigger | **BUG_M09_01** race; integration pending |
| BF-M10-03 Duyệt (tab thu) | Finance.ApprovePayment | accountant/SA | Pending→Paid | TC_M10_009,011 | — | **BUG_M10_01** | integration pending |
| BF-M10-04 Edit khoản thu | PaymentDialog | accountant/SA | any→Paid | TC_M10_010,025,026 | — | **BUG_M10_01** | bUnit pending |
| BF-M10-05 Khoản chi | ExpenseDialog | accountant/SA | — | TC_M10_013,014,015 | — | Verified(code); **OBS-M10-01** no-approve | bUnit pending |
| BF-M10-06 Phiếu + PDF | CreateReceiptFor*/endpoint | accountant/SA/director | — | TC_M10_016,017,018,023 | — | Verified(code) idempotent+gate | e2e pending |
| BF-M10-07 Self-scoped | Finance.Load/LoadProgress | parent/student | read | TC_M10_021,022 | — | Verified(code) filter | e2e pending |
| Contract | PaymentStage/Status/ReceiptType | — | — | TC_M10_030..033 | **M10_FinanceRulesTests (4)** | **Pass** | — |
| U2 no-refund | (không refund logic) | — | — | TC_M10_028 | — | Verified(code) | integration cross-check pending |

## Gap Analysis

| Gap | Loại | Xử lý |
|---|---|---|
| Nhiều đường set Paid, 1 trigger hoa hồng + hở tuần tự | **Application defect** | **BUG_M10_01 (Medium)** → Codex: thống nhất set-Paid qua 1 hàm (ép tuần tự + EnsureAsync), hoặc chặn set-Paid stage payment ngoài tab Tiến độ. |
| Idempotency hoa hồng concurrency | Application (M09) | **BUG_M09_01** — cùng handoff M09. |
| Khoản chi không có luồng duyệt | Requirement | **OBS-M10-01 / U-M10-1** — confirm RB-7. |
| Code trùng random suffix | Application (Low) | **OBS-M10-02** — dùng sequence/retry hoặc bắt DbUpdateException. |
| Runtime split/tuần tự/commission/PDF | Test infra | Integration DB + e2e — blocker chung. |
| `PaymentSchedule.Split` nằm ở Web | Testability | Đề xuất tách sang Domain → unit-test split trực tiếp (cho Codex, KHÔNG làm ở QA). |

**Coverage tuyên bố:** 7 flow + split + tuần tự + authz + IDOR + U2 phân tích đủ. Automated chỉ phủ contract enum (4 Pass). BUG_M10_01 phát hiện ở mức code; runtime cần integration. KHÔNG tuyên bố 100%.
