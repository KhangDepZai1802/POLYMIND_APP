# M11 — Loans & Debt Collection · Traceability

| Business Flow | Page | API | Role | State | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M11-01 Tạo hồ sơ vay | /loans + LoanDialog | — | edit roles | none→Borrowing/Disbursed | TC_M11_001,002,007,008,009 | TC_M11_040 (default) | Code + partial unit | Runtime create (Blocked harness) |
| BF-M11-02 Sinh lịch trả góp | LoanDialog | — | edit roles | +repayments Pending | TC_M11_002,003,004,027,029,030,031 | TC_M11_041 | Code (formula ở Web) | Formula unit cần tách Domain (Blocked) |
| BF-M11-03 Sửa loan | LoanDialog | — | edit roles | update | TC_M11_005,006,028 | — | Code | Runtime edit (Blocked) |
| BF-M11-04 Thu nợ + tất toán | /debt-collection | — | edit roles | Pending→Paid; Loan→Settled | TC_M11_010,011,012,013,014,023,036 | TC_M11_037,039 | Code | Runtime MarkPaid (Blocked) |
| BF-M11-05 Xóa loan | /loans + CandidateDetail | — | delete roles | removed | TC_M11_018,019,035 | — | Code | Orphan repayment (Obs) |
| BF-M11-06 Gate B20 | /candidates/{id} AdvanceStep | — | workflow roles | block/allow Completed | TC_M11_024,025,026,028 | TC_M11_037 | Code | Runtime advance (Blocked) |
| AuthZ (page + mutation) | tất cả | — | mọi role | — | TC_M11_015..023 | TC_M11_038 | Code | Runtime role matrix (Blocked) |
| Scope (agent/self) | /loans, /debt-collection | — | agent/parent/student | — | TC_M11_020,021,022 | — | Code | Runtime scope (Blocked) |
| Concurrency/DB | — | — | — | — | TC_M11_034,035,036 | — | Obs | No unique/rowversion/FK (Obs) |
| Enum/entity contract | — | — | — | — | TC_M11_037..041 | **5 unit** | **Automated** | — |
| BUG_M11_01 gate Bank/Company | CandidateDetail | — | workflow roles | B20 block/allow | TC_M11_042,043 | **Unit** | Automated + source | Runtime UI chờ verify |
| CR-M11-1/2/3 thu nợ an toàn | DebtCollection + LoanDialog | — | accountant/super_admin | Pending/Partial→Paid; Loan→Settled | TC_M11_044..049 | **Unit + migration** | Automated + build | Runtime DB/role/receipt chờ verify |

## Gap Analysis

- **Đã phủ ở source (Pass code):** page authorize, mutation permission+role re-check, scope filter (agent/self read-only), attribution actor thật, Bank vs Company phân nhánh, gate B20 dùng latest loan, autocomplete lọc trùng, validation Amount/Term.
- **Automated:** enum/entity contract + gate Bank/Company + status settlement + individual/collect-all + no-schedule + migration source links. Toàn suite Codex handoff: **82/82**.
- **Blocked (cần harness):** mọi flow runtime tạo/sửa/thu/xóa/advance qua DB + UI; công thức lãi/lịch (nằm trong `LoanDialog.razor` — cần tách Domain để unit-test amount/split/rounding).
- **Observations (không phải confirmed bug):** OBS-M11-01 (2 loan/candidate — no unique), OBS-M11-02 (thu nợ over-permission — req), OBS-M11-03 (thu nợ không sinh receipt — req), OBS-M11-04 (không partial payment), OBS-M11-05 (set Settled thủ công mở gate B20 — req), OBS-M11-06 (xóa loan orphan repayments — no FK cascade).
- **Không tuyên bố 100%:** phạm vi runtime DB/UI + công thức tài chính chưa đo tự động.
