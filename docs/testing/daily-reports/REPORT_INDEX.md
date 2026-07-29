# REPORT INDEX — BÁO CÁO KIỂM THỬ HẰNG NGÀY POLYMIND

> Tài liệu đối chiếu nội bộ, không dùng để gửi cấp quản lý.  
> Các báo cáo DOCX dùng ngôn ngữ nghiệp vụ; mã kiểm tra và mã vấn đề chỉ được giữ tại bảng này để truy lại bằng chứng.

| Ngày | Tên báo cáo | Module hoặc chức năng | Test case nguồn | Bug nguồn | Trạng thái |
|---|---|---|---|---|---|
| 14/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-14.docx` | Tổng quan 20 module; Dashboard; Security & Deployment | M17 `TC_M17_001..023`; M20 `TC_M20_001..030`; traceability M01–M20 | Không gán bug mới; dùng bảng trạng thái tổng hợp | Đã tạo |
| 15/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-15.docx` | M01 — Authentication & Session | M01 `TC_M01_001..024` | `BUG_M01_01`, `BUG_M01_02`, `BUG_M01_03` | Đã tạo |
| 16/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-16.docx` | M02 — Authorization; M03 — User Management | M02 `TC_M02_001..022`; M03 `TC_M03_001..024` | `BUG_M02_01`, `BUG_M02_02`, `BUG_M03_01`; tham chiếu `BUG_M01_01` | Đã tạo |
| 17/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-17.docx` | M05 — Candidate Management | M05 `TC_M05_001..030` | Không có bug mới ở M05; tham chiếu `BUG_M02_02`, `BUG_M03_01`, `BUG_M01_01` | Đã tạo |
| 18/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-18.docx` | M03/M05 — tài khoản gia đình; M14 — phạm vi liên hệ | M03 `TC_M03_009,015..020`; M05 `TC_M05_004..008,016..028`; M14 `TC_M14_029..032,043..046` và ca bổ sung trong bug report | `BUG_M03_01`; `CR-M14-2` | Đã tạo |
| 19/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-19.docx` | M02/M05 — phạm vi và thông tin cá nhân ứng viên | M02 `TC_M02_022`; M05 `TC_M05_005..008,015..018,027..030` | `BUG_M02_02`; observations M05 | Đã tạo |
| 20/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-20.docx` | M06 — Job Orders, market/category/deadline | M06 `TC_M06_001..017` | `BUG_M06_01` | Đã tạo |
| 21/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-21.docx` | M09 — Agents, Collaborators & Commissions | M09 `TC_M09_001..035` | `BUG_M09_01`, `BUG_M09_02`, `CR-M09-1`, `CR-M09-2`, cập nhật `CR-M09-3` tại checkpoint | Đã tạo |
| 22/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-22.docx` | M07 — Workflow; M08 — Training; đối chiếu gate M11 | M07 `TC_M07_001..021`; M08 `TC_M08_001..034`; M11 `TC_M11_024..026,042..048` | `CR-M08-1`; cập nhật `CR-M08-2`; tham chiếu `BUG_M11_01`, `CR-M11-3` | Đã tạo |
| 23/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-23.docx` | M04 — Lead appointments; M12 — Visa & Flight | M04 `TC_M04_003..006,017..021`; M12 `TC_M12_001..030` | `BUG_M12_01`, `BUG_M12_02`; yêu cầu `U-M12-1`, `U-M12-2` | Đã tạo |
| 24/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-24.docx` | M10 — Finance; M11 — Loans & Debt Collection | M10 `TC_M10_001..033`; M11 `TC_M11_001..049` | `BUG_M10_01`, `CR-M10-3`, `U-M10-1`; `BUG_M11_01`, `CR-M11-1/2/3` | Đã tạo |
| 25/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-25.docx` | M18 — Documents; M19 — Audit Log | M18 `TC_M18_001..023`; M19 `TC_M19_001..044` | Không có confirmed bug; dùng observations M18/M19 | Đã tạo |
| 26/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-26.docx` | M13 — Notifications; M14 — Messaging; M15 — AI Assistant | M13 `TC_M13_001..042`; M14 `TC_M14_001..046` và ca bổ sung CR-M14-2/3; M15 `TC_M15_001..043` | `BUG_M13_01`, `CR-M13-1`; `CR-M14-1/2/3`; `BUG_M15_01` | Đã tạo |
| 27/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-27.docx` | Search/filter/list trên M03–M06, M08–M09 | M03 `TC_M03_023`; M04 `TC_M04_015,022,023`; M05 `TC_M05_001..003`; M06 `TC_M06_003,015`; M08 `TC_M08_006`; M09 `TC_M09_025..035` | Không tạo bug tổng hợp mới; tham chiếu observations hiệu năng từng module | Đã tạo |
| 28/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-28.docx` | M16 — Reports & Export; M17 — Dashboard | M16 `TC_M16_001..032`; M17 `TC_M17_001..023` | `BUG_M16_01`, `CR-M16-1`, `CR-M17-1` | Đã tạo |
| 29/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-29.docx` | Kiểm tra chéo quyền M02/M05/M08/M09/M14/M16/M20 | M02 `TC_M02_007..022`; M05 `TC_M05_002..014,016..028`; M08 `TC_M08_018..025`; M09 `TC_M09_013..027,035`; M14 permission cases; M16 `TC_M16_001..006`; M20 `TC_M20_012..019,029..030` | `BUG_M02_01/02`, `CR-M08-2`, `CR-M09-3`, `CR-M14-2/3`, `CR-M16-1`, `CR-M17-1` | Đã tạo |
| 30/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-30.docx` | Dữ liệu sai/thiếu, duplicate/concurrency, security readiness | Negative/edge/concurrency cases trong M01, M04–M11, M14, M18, M20; trọng tâm M20 `TC_M20_001..030` | Các observations concurrency; `OBS-M20-01..10`; không nâng observation thành bug | Đã tạo |
| 31/07/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-07-31.docx` | Tổng hợp bug, change request và verification M01–M20 | Toàn bộ `03-test-cases.md`, `06-bug-report.md`, `07-fix-report.md`, `08-verification-report.md` hiện có | Toàn bộ bug/CR trên `MODULE_QA_BOARD.md` và cập nhật phiên #9 trong `SESSION_CHECKPOINT.md` | Đã tạo |
| 01/08/2026 | `POLYMIND_BAO_CAO_KIEM_THU_2026-08-01.docx` | Tổng kết 20 module và đề xuất giai đoạn tiếp theo | Toàn bộ test case M01–M20 | Không tạo bug mới; tổng hợp đúng trạng thái hiện hành | Đã tạo |

## Nguồn Markdown đã đối chiếu

- `docs/testing/MODULE_QA_BOARD.md`.
- `docs/testing/SESSION_CHECKPOINT.md`.
- Toàn bộ thư mục `docs/testing/modules/M01-*` đến `docs/testing/modules/M20-*`.
- Trong từng module: `01-analysis.md`, `02-business-flows.md`, `03-test-cases.md`, `04-traceability.md`, `05-automation-report.md`, `06-bug-report.md`.
- Các module có hồ sơ sửa và xác nhận: `07-fix-report.md`, `08-verification-report.md`.
- `docs/testing/modules/M02-authorization/evidence-M02_02-runtime.md`.
- `docs/messaging-tiers.md` cho quy tắc liên hệ mới của M14.

## Lưu ý diễn giải

- “Đã tạo” trong bảng này chỉ xác nhận file báo cáo đã được sinh, không đồng nghĩa toàn bộ chức năng đã kiểm tra hoàn tất trên giao diện.
- Những nội dung `Fixed — chờ xác minh`, `Waiting for Fix`, `runtime pending`, `Blocked` hoặc observation trong nguồn được chuyển sang ngôn ngữ quản lý là “đã điều chỉnh, chờ xác nhận”, “chưa đủ dữ liệu để kết luận” hoặc “điểm cần theo dõi”.
- Không có tên đối thủ cụ thể hoặc khẳng định thiếu nguồn trong phần so sánh của các báo cáo.
