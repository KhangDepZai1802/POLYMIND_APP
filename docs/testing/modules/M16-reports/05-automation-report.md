# M16 — Reports & Export · Automation Report

## Framework & phạm vi
- xUnit (`tests/Polymind.Tests`), ref chỉ Domain + Infrastructure.
- Codex tách `ReportDateRange` + `ReportAccessRules` sang Domain; endpoint/builder vẫn ở Web.
- **M16 có 6 automated regression tests**; không thêm test giả.

## Automated tests
| Automated Test | Test Case | Kết quả |
|---|---|---|
| `Date_range_is_inclusive_and_serializes_for_export_links` | TC_M16_012 | Pass |
| `Invalid_reversed_range_is_rejected` | TC_M16_032 | Pass |
| `All_time_range_keeps_backward_compatible_url` | backward compatibility | Pass |
| `Recruitment_manager_can_export_recruitment_but_not_financial_slugs` | TC_M16_006/CR-M16-1 | Pass |
| `Finance_roles_can_export_all_known_slugs` | TC_M16_004 | Pass |
| `Financial_permission_is_registered_for_dynamic_policy_provider` | CR-M16-1 | Pass |

## Lệnh chạy (suite chung — không đổi)
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Codex handoff: Passed 112, Failed 0, Skipped 0.
```

## Phân loại phát hiện
- **Application Defect:** BUG_M16_01 — Fixed by Codex, chờ Claude.
- **Change:** CR-M16-1 — Fixed by Codex, chờ Claude.
- **Defense-in-depth/Perf/Security-hardening:** OBS-M16-01..05.

## Blocked / pending harness
| Hạng mục | Cần |
|---|---|
| 403 theo role cho `/export/*`, `/receipts/*` | WebApplicationFactory + seed role |
| Export honor range runtime | integration + so khớp nội dung file cho 8 slug × 3 format |
| IDOR receipt probe | integration + seed receipt nhiều chủ |

## Automation backlog
- Integration test endpoint report/export/receipt theo role (401/403/200), assert nội dung honor range.
