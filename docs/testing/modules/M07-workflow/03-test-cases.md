# M07 — Candidate Workflow · Test Cases

> `TC_M07_<n>`. Workflow logic ở `.razor` + `Web/Display` → UI/integration → phần lớn `Blocked (no harness)`; điểm có bằng chứng dòng code = `Pass (code)`.

| TC | Tên | Flow | Type | Priority | Sev if fail | Role | Expected | Automation | Status |
|---|---|---|---|---|---|---|---|---|---|
| TC_M07_001 | Advance đúng nhóm quyền | BF-01 | Functional | High | High | Kế toán @Deposit | Chuyển bước, record Completed, actor đúng | UI/manual | Blocked (harness) |
| TC_M07_002 | Advance sai quyền (vertical escalation) | BF-01 | AuthZ | High | High | Visa @Deposit | `CanAdvance` chặn (1723) | UI/manual (code) | Pass (code) |
| TC_M07_003 | Advance của đối tác/cổng | BF-01 | AuthZ | High | High | collaborator/parent | Không có nút + CanAdvance=false | UI/manual (code) | Pass (code) |
| TC_M07_004 | Next() B7→B8 bỏ qua 7.5 | BF-01 | State | High | High | Hồ sơ @HealthCheck | Next=EntranceExam | code review (`WorkflowSteps.Next`) | Pass (code) |
| TC_M07_005 | Không nhảy/bỏ bước tùy ý | BF-01 | State | High | High | any | Chỉ +1 (hoặc 7→9) | code review | Pass (code) |
| TC_M07_006 | EntranceExam thiếu `_examMode` | BF-01 | Validation | Med | Med | Hồ sơ | Chặn "chọn tình huống" (1735) | UI/manual (code) | Pass (code) |
| TC_M07_007 | Orientation không chọn nội dung/không skip | BF-01 | Validation | Med | Med | Hồ sơ | Chặn (1754) | UI/manual (code) | Pass (code) |
| TC_M07_008 | FullPayment "Cam kết trả nợ" chưa có vay | BF-01 | Business rule | High | Med | Kế toán | Chặn "chưa có hồ sơ vay" (1768) | UI/manual (code) | Pass (code) |
| TC_M07_009 | Fail B8 → lùi 7.5 | BF-02 | Functional | High | High | Hồ sơ | record Failed, CurrentStep=7.5 | UI/manual (code) | Pass (code) |
| TC_M07_010 | Fail B8 thiếu `_examMode` | BF-02 | Validation | Med | Med | Hồ sơ | Chặn (1851) | UI/manual (code) | Pass (code) |
| TC_M07_011 | Reassign job mới ≠ đơn cũ | BF-03 | Business rule | High | High | Tuyển dụng | Chặn nếu chọn đơn cũ (1924) | UI/manual (code) | Pass (code) |
| TC_M07_012 | Reassign job hết hạn ứng tuyển | BF-03 | Validation | Med | Med | Tuyển dụng | Chặn "hết hạn" (1931) | UI/manual (code) | Pass (code) |
| TC_M07_013 | Reassign 7.5 → B8 giữ hồ sơ/lịch sử | BF-03 | Functional | High | High | Tuyển dụng | JobOrderId đổi, Step=B8, lịch sử giữ | UI/manual | Blocked (harness) |
| TC_M07_014 | B20 hoàn thành khi còn nợ vay | BF-05 | Business rule | High | High | Tuyển dụng | Chặn `_hasOpenLoan` (1777) | UI/manual (code) | Pass (code) |
| TC_M07_015 | B20 hoàn thành khi nợ tất toán | BF-05 | Functional | High | High | Tuyển dụng | confirm → Completed, Status=Completed | UI/manual | Blocked (harness) |
| TC_M07_016 | B19 thêm nhật ký (nhiều record) | BF-04 | Functional | Med | Low | Tuyển dụng | record InProgress, không đổi CurrentStep | UI/manual | Blocked (harness) |
| TC_M07_017 | Advance ghi actor thật | BF-01 | Data | High | Med | non-first user | `CreatedBy`/`AssignedTo`=actor (GetRequiredUserIdAsync) | UI/manual (code) | Pass (code) |
| TC_M07_018 | Commission phát sinh idempotent khi advance | BF-01 | Business rule | High | High | Kế toán | Không double hoa hồng | Integration (M09) | Blocked (harness) |
| TC_M07_019 | Double-click advance | BF-01 | Concurrency | Med | Med | staff | `_busy` chặn trong circuit | UI/manual (code 1792) | Partial (code) |
| TC_M07_020 | 2 người advance cùng lúc (stale) | BF-01 | Concurrency | Med | Med | 2 staff | Không double-advance/skip | Integration | **Blocked → OBS-M07-01** |
| TC_M07_021 | RB-2 đổi đơn hàng reset workflow | BF-06 | Functional | High | High | super_admin | reset 20 bước (hoàn tiền? U1) | UI/manual | Blocked (harness + spec) |

## Gap
- Workflow mutation ở `.razor`, access ở `Web/Display` → không unit-test được từ test project (không ref Web). State-machine `Next()`/`CanAdvance` verify qua **source review** (Pass code).
- Concurrency (TC_020) cần integration harness → chưa đo (OBS-M07-01).
- Backlog: tách `WorkflowStepAccess`/`WorkflowSteps` sang `Polymind.Domain` → unit-test ma trận role/bước + Next() trực tiếp.
