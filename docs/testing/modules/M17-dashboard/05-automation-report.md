# M17 — Dashboard · Automation Report

## Framework & phạm vi
- xUnit (`tests/Polymind.Tests`), ref chỉ Domain + Infrastructure.
- Home.razor + Overview.razor + StatCard nằm ở `Polymind.Web` + cần DB → **0 component test riêng cho M17**. Không thêm test giả.
- CR-M17-1 dùng policy `financial_reports:read` đã được M16 kiểm registry/access matrix (6/6); Web build kiểm Razor compile.

## Automated tests
| Automated Test | Test Case | Kết quả |
|---|---|---|
| — | — | Không có (component ở Web + cần DB) |

## Lệnh chạy (suite chung — không đổi)
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Codex handoff: Passed 112, Failed 0, Skipped 0 (M17 không thêm component test giả).
```

## Phân loại phát hiện
- **Không có Application Defect.** Authz + scope đúng ở source.
- **Change request:** CR-M17-1 — Fixed by Codex, chờ Claude xác minh.
- **Perf observation:** OBS-M17-02.

## Blocked / pending harness
| Hạng mục | Cần |
|---|---|
| Redirect partner + 403 self-scoped | WebApplicationFactory/bUnit + seed role |
| Portal `/me` chỉ hồ sơ mình | bUnit + DB seed parent/student |
| KPI số liệu đúng | integration + seed dữ liệu |

## Automation backlog
- bUnit render Home theo role (partner → redirect; recruiter → thấy KPI); render Overview với parent/student seed → assert chỉ OwnedCandidate. Cần harness Web.
