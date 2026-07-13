# M07 — Candidate Workflow (20 bước) · Phân tích

> Nguồn đã đọc: `WorkflowStep`/`WorkflowStepStatus`/`CandidateJobOrderStatus` enum (`Enums.cs`), `CandidateJobOrder`/`WorkflowStepRecord` entity, `WorkflowStepAccess.cs`, `WorkflowSteps.cs`, và luồng inline trong `CandidateDetail.razor` (`AdvanceStep`, `FailEntranceExam`, `ReassignJobOrder`, `AddOverseasLog`, RB-2 `ChangeJobOrderAsync`). Hoa hồng → M09; tài chính (PaymentStage) → M10; visa/vé → M12.

## 1. Module Overview
- **Module ID:** M07 · **Name:** Candidate Workflow 20 bước
- **Business purpose:** State-machine tiến trình XKLĐ của cặp (ứng viên, đơn hàng) qua 20 bước chính + 1 bước phụ 7.5. Mỗi lần chuyển bước ghi `WorkflowStepRecord` (audit tiến độ), phân quyền theo nhóm phụ trách từng bước, và trigger hoa hồng theo giai đoạn đóng tiền.
- **Actor/Role:** staff theo nhóm bước (Tuyển dụng/TVV, Kế toán, Hồ sơ, Visa) + super_admin (mọi bước). Đối tác/cổng cá nhân KHÔNG có quyền chuyển bước.
- **Dependencies:** M05 (Candidate + gắn đơn), M06 (JobOrder). Trigger M09 (commission), liên quan M10 (payment stage), M12 (visa/flight).
- **Entry:** gắn đơn hàng (CandidateJobOrder, CurrentStep=Lead) → nút chuyển bước ở `/candidates/{id}`. **Exit:** `Completed` (B20) khi hết nghĩa vụ (vay tất toán).

## 2. Source Code Map
| File | Vai trò | Ghi chú |
|---|---|---|
| `Enums.cs` `WorkflowStep` | 21 giá trị B1..B20 + Completed; ReselectJobOrder=8 (B7.5) | build xanh (comment `//`) |
| `CandidateJobOrder.cs` | Nối UV↔job + `CurrentStep` + `Status`(Active/Dropped/Completed) | 1 dòng = 1 tiến trình |
| `WorkflowStepRecord.cs` | Lịch sử từng bước: Step/Status/AssignedTo/CreatedBy/Notes/thời gian | append-only |
| `WorkflowStepAccess.cs` (Web/Display) | `CanAdvance(user, step)` + `CanAssignJobOrder` + `OwnerLabel` | ma trận role/bước |
| `WorkflowSteps.cs` (Web/Display) | `Next()` (7→9 skip 7.5), `No()`, `Progress()` | luồng tuần tự |
| `CandidateDetail.razor` | `AdvanceStep`/`FailEntranceExam`/`ReassignJobOrder`/`AddOverseasLog`/RB-2 | mutation inline |

## 3. State Machine (chuẩn hóa từ source)

`Next()`: tuần tự +1, **HealthCheck(B7)→EntranceExam(B8)** bỏ qua ReselectJobOrder(7.5), cap tại Completed. 7.5 chỉ vào qua fail B8.

| Step | # | Nhóm phụ trách (CanAdvance) | Gate đặc biệt |
|---|---|---|---|
| Lead/Contacted/Consulting/Registration | 1-4 | Tuyển dụng/TVV | — |
| Deposit | 5 | Kế toán | — |
| Document/HealthCheck | 6-7 | Hồ sơ | — |
| **ReselectJobOrder** | 7.5 | Tuyển dụng | chỉ qua fail B8; `ReassignJobOrder` bắt job MỚI + còn hạn |
| EntranceExam | 8 | Hồ sơ + Tuyển dụng | chọn `_examMode`; có nhánh **FailEntranceExam**→7.5 |
| Selected | 9 | Hồ sơ + Tuyển dụng | — |
| Orientation | 10 | Hồ sơ | chọn nội dung học hoặc Skip |
| SignContract | 11 | Hồ sơ + Kế toán | — |
| CoeApplication/VisaSubmit/VisaApproved | 12-14 | Visa | — |
| FullPayment | 15 | Kế toán | `_paymentMode`; "Cam kết trả nợ" cần có hồ sơ vay |
| BookFlight/Departure/Arrived | 16-18 | Visa + Tuyển dụng | — |
| OverseasSupport | 19 | Tuyển dụng + Hồ sơ | B20 gate: **khoản vay phải tất toán** + confirm |
| Completed | 20 | — | `Status=Completed` |

## 4. Database Impact
- `candidate_job_orders`: `CurrentStep`, `Status`; đổi khi advance/fail/reselect. **Không có concurrency token (rowversion).**
- `workflow_step_records`: append mỗi lần chuyển bước (Completed/Failed/Skipped/InProgress), gắn actor `CreatedBy`/`AssignedTo`.
- Side effect: `CommissionEngine.EnsureAsync` (idempotent) mỗi advance; audit `advance_step`/`fail_step`/`reselect_job_order`.

## 5. Roles & Permissions
- Chuyển bước: `WorkflowStepAccess.CanAdvance` (super_admin any; nhóm theo bước). Re-check server-side ở cả 3 mutation (`AdvanceStep` 1723, `FailEntranceExam` 1857, `ReassignJobOrder` 1911).
- Gắn đơn: `CanAssignJobOrder` (super_admin/Tuyển dụng).
- **Attribution ĐÚNG:** mọi record dùng `AuthStateProvider.GetRequiredUserIdAsync(db)` (actor thật) — KHÔNG dính anti-pattern "first user" (khác M06/M12).

## 6. Risk Analysis
| # | Risk | Mức | Trạng thái |
|---|---|---|---|
| R1 | Chuyển bước sai quyền (vertical escalation) | High | **Chặn** (CanAdvance re-check) — không bug |
| R2 | Nhảy/bỏ bước tùy ý | High | **Chặn** (`Next()` tuần tự; 7.5 chỉ qua fail) — không bug |
| R3 | Double-advance / lost update (2 người hoặc double-click) | Med | **OBS** — không rowversion; `_busy` chỉ chặn trong 1 circuit; validate dùng `_cjo` cached vs advance dùng `cjo` fresh → stale-state race |
| R4 | Reselect gắn lại đúng đơn cũ | Med | **Chặn** (new≠old + deadline) — không bug |
| R5 | Hoàn thành khi còn nợ vay | High | **Chặn** (`_hasOpenLoan` gate B20) — không bug |
| R6 | RB-2 đổi đơn hàng reset workflow có hoàn tiền/hoa hồng? | — | **U1 ĐÃ CHỐT: KHÔNG hoàn → hành vi đúng, không bug** |
| R7 | Commission double khi advance | Med | Cross M09 — `EnsureAsync` idempotent (comment); verify ở M09 |

## 7. Requirement Clarification
- **U1 (= M05 U2) — ĐÃ CHỐT (user 2026-07-10):** RB-2 `ChangeJobOrderAsync` (super_admin đổi đơn hàng) reset tiến trình **KHÔNG** hoàn/hủy khoản thu + hoa hồng đã phát sinh (khớp WORKLOG). → Hành vi đúng, **không bug.** Verify chéo M09/M10 rằng không hoàn tiền vô tình.
- **U2 (còn mở — kỹ thuật, không phải nghiệp vụ):** `EnsureAsync` tính hoa hồng theo PaymentStage — mức idempotent thực tế cần chạy runtime ở **M09** (không cần user chốt).
