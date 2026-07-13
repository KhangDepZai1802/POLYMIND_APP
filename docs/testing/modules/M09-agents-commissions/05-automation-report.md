# M09 — Agents & Commissions · 05 Automation Report

> QA: Claude · 2026-07-10.

## Framework & dependency

- `tests/Polymind.Tests` (xUnit, .NET 10). Ref Domain + Infrastructure — **KHÔNG** ref Web.
- Logic engine (`CommissionEngine`) + dialog clamp/authz nằm ở `Polymind.Web` → không unit-test trực tiếp ở đây. Cần bUnit + WebApplicationFactory/DB test (chưa dựng).

## Test phiên này

| File | Test | TC | Loại | Kết quả |
|---|---|---|---|---|
| `tests/Polymind.Tests/M09_CommissionRatesTests.cs` | `Agent_commission_rate_splits_are_1_1p5_2p5_totalling_5` | TC_M09_030 | unit (Domain const) | **Pass** |
| | `Collaborator_share_bounds_are_30_to_40_default_35` | TC_M09_031 | unit | **Pass** |
| | `New_collaborator_defaults_to_35_percent_share` | TC_M09_032 | unit | **Pass** |
| | `New_commission_starts_pending` | TC_M09_033 | unit | **Pass** |

**Phạm vi:** chốt hằng số tỉ lệ hoa hồng (business-critical — đổi âm thầm sẽ sai tiền toàn hệ thống) + default entity. **KHÔNG** phủ idempotency/amount/scope (những phần đó ở razor/Web).

## Lệnh chạy

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả tổng

- **Passed: 37 · Failed: 0 · Skipped: 0** (33 trước + 4 M09). Build test 0/0.

## Phân loại & trạng thái

| Nhóm | Trạng thái | Ghi chú |
|---|---|---|
| Rate/entity contract | **Automated Pass** (4) | Phiên này |
| Idempotency tuần tự | **Verified (code)** | `exists` AnyAsync guard trước Add |
| **Idempotency concurrency** | **Application Defect → BUG_M09_01** | Không unique index; race → duplicate. Runtime-repro cần integration parallel (Blocked). |
| **State guard approve/pay** | **Application Defect → BUG_M09_02** | Set status vô điều kiện. |
| Amount = %×cost / fixed | **Verified (code)** | Blocked automation (Web). |
| Config selection khớp nhất | **Verified (code)** | OrderByDescending job>country. |
| Authz gate + dialog re-check | **Verified (code)** | approve/pay/config/ctv/agent đều re-check. |
| IDOR portal + mask SĐT | **Verified (code)** | filter agentId/collaboratorId; MaskPhone. |
| U2 no-refund reset | **Verified (code)** | exists guard giữ mốc đã hưởng. |
| CTV share clamp 30-40 | **Verified (code)** | ClampPercentage. |

## Environment / data issues

- Không DB test / bUnit / Playwright → engine + dialog + portal runtime không đo phiên này (Blocked, không suy đoán).

## Codex CR-M09-1/2 regression (session tiếp theo)

| Test/Check | Kết quả | Ghi chú |
|---|---|---|
| M09 tests sau snapshot + visibility rules | **Passed 16/16** | Chạy trước khi thêm model-contract test cuối và migration |
| Web build sau source CR-M09-1/2 | **Passed 0 warning / 0 error** | `.qa/build/m09-pre-migration` |
| Model-contract test snapshot/index | **Not rerun** | Được thêm sau đó; restore environment hỏng |
| Full suite sau M09 | **Blocked** | `project.assets` bị offline restore ghi lại; NU1101 packages, restore ngoài sandbox không được phê duyệt do usage limit |

Không sửa/xóa/skip test để né blocker. Claude cần restore dependencies rồi chạy M09 + full suite + Web build + migration apply.

## Automation backlog (đề xuất — KHÔNG làm ở QA)

1. **Integration (WebApplicationFactory + DB):** EnsureAsync — sinh đúng mốc theo stage Paid; idempotent tuần tự; **race parallel → repro BUG_M09_01**; amount %/fixed; config match; U2 no-refund.
2. **bUnit:** approve/pay re-check + **status guard repro BUG_M09_02**; config validation; CTV clamp/re-check.
3. **e2e:** portal scope + mask SĐT + leaderboard + partner redirect.
4. **Refactor gợi ý (Codex):** tách `CommissionEngine`/`Map` + clamp sang Domain → unit-test amount/idempotency trực tiếp.
