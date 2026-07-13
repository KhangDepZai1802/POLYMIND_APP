# M08 — Training · 05 Automation Report

> QA: Claude · 2026-07-10.

## Framework & dependency

- **Test project:** `tests/Polymind.Tests` (xUnit 2.9.2, .NET 10). Ref **Domain + Infrastructure** — **KHÔNG** ref `Polymind.Web` (tránh khóa DLL khi dev server `:5177` chạy + logic Web dùng test thủ công/harness sau).
- **Hệ quả:** toàn bộ logic M08 nằm trong razor (`Polymind.Web/Components/Pages/Training/*`) → **không unit-test trực tiếp** được ở đây. Cần bUnit (component) + WebApplicationFactory/DB test (runtime) — chưa dựng (blocker chung).

## Test structure phiên này

| File | Test | Test Case | Loại | Kết quả |
|---|---|---|---|---|
| `tests/Polymind.Tests/M08_TrainingRulesTests.cs` | `TrainingTrack_has_exactly_language_and_vocational` | TC_M08_030 | unit (Domain enum contract) | **Pass** |
| | `EvaluationRating_has_four_levels_in_ascending_order` | TC_M08_029 | unit | **Pass** |
| | `New_training_record_defaults_to_enrolled` | (bất biến default) | unit | **Pass** |
| | `Training_evaluation_track_is_optional_for_general_review` | (bất biến) | unit | **Pass** |
| | `Related_staff_can_read_training_but_cannot_mutate_it` (4 role cases) | TC_M08_020 / CR-M08-1 | unit (Infra seed contract) | **Pass 4/4** |

**Phạm vi 8 test:** 4 contract enum/entity + 4 case role seed cho CR-M08-1. Không phủ runtime Razor clamp/scope/week-grouping.

## Lệnh chạy

```bash
# An toàn kể cả khi dev server :5177 đang chạy (test project không ref Web).
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả tổng

- **M08: 8/8; toàn suite: 116 passed · 0 failed · 0 skipped.**
- Web build output `.qa/build/m08-cr1`: 0 error, 0 warning.

## Phân loại & trạng thái

| Nhóm | Trạng thái | Ghi chú |
|---|---|---|
| Enum/entity contract | **Automated Pass** (4) | Phiên này |
| CR-M08-1 role permission | **Automated Pass** (4 role cases) | Có read; không có create/update/delete/approve |
| Authorization gate (page + dialog re-check) | **Verified (code)** | Đọc source: `[Authorize]` + `AuthorizeAsync` re-check trong SaveAsync. Runtime bUnit pending. |
| Data scope / IDOR | **Verified (code)** | Query lọc `AgentScope` fail-closed (giống pattern `CandidateAccessScope` đã verify M02/M05). Runtime pending. |
| Progress clamp 0..100 | **Verified (code)** | `Math.Clamp` trong `TrainingTrackDialog.SaveAsync`. Blocked automation (razor). |
| Attribution `CreatedBy=actor` | **Verified (code)** | `GetRequiredUserIdAsync` — KHÔNG dính first-user anti-pattern (BUG_M04/M06). |
| Week grouping Monday-based | **Verified (code)** | `WeekStart` `((int)DayOfWeek+6)%7`. Blocked automation (private razor). |
| Audit ghi | **Verified (code)** | `AddAudit` cả 2 flow. DB assert pending harness. |
| Upload minh chứng (MinIO) | **Blocked** | Cần MinIO test → M18. |
| Concurrency unique-index race | **Blocked / OBS-M08-01** | Cần integration DB test. |

## Environment / test-data issues

- Không có DB test / MinIO test / bUnit host → runtime của module không đo được phiên này (khai báo Blocked, không suy đoán pass).

## Automation backlog (đề xuất, KHÔNG làm trong phiên QA — cần refactor/harness)

1. **bUnit** cho `TrainingTrackDialog`/`TrainingEvaluationDialog`: assert re-check quyền + clamp + snackbar khi thiếu quyền (TC_M08_009/010/021/022/034).
2. **WebApplicationFactory + DB test**: unique index `(candidate,track)`, audit row, IDOR scope, self-scoped redirect (TC_M08_023/024/031/032/033).
3. **Refactor gợi ý (cho Codex):** tách helper thuần `TrainingProgress.Overall(...)` + `WeekGrouping.WeekStart(...)` sang Domain → unit-test trực tiếp clamp/average/week (bỏ Blocked). *Không thực hiện ở phiên QA vì là sửa business-logic/cấu trúc.*
