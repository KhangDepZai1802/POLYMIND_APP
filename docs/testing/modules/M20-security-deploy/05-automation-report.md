# M20 — Security & Deployment · Automation Report

## Framework & dependency
- **xUnit** (`tests/Polymind.Tests`). Test project ref `Polymind.Domain` + `Polymind.Infrastructure` — KHÔNG ref `Polymind.Web`.
- Hệ quả: security logic ở `Polymind.Web/Program.cs` (pipeline, headers, JWT/cookie wiring, Swagger gating, rate limit) **không unit-test được** từ project này → xác minh bằng phân tích tĩnh (đọc source thật), ghi ở `03-test-cases.md`.

## Test structure (M20)
- `tests/Polymind.Tests/M20_SecurityInvariantsTests.cs` — bất biến RBAC seed chống leo thang quyền dọc (nơi DUY NHẤT test project chạm được logic bảo mật).

## Automated tests
| Test | TC | Kết quả |
|---|---|---|
| `Partner_and_portal_roles_have_no_sensitive_mutation_permissions` (Theory ×4 role × 24 permission) | TC_M20_012 | Pass |
| `Non_finance_staff_cannot_read_financial_reports` (Theory ×5) | TC_M20_013 | Pass |
| `Finance_roles_keep_financial_reports_read` (Theory ×2) | TC_M20_014 | Pass |
| `User_and_role_administration_not_granted_to_non_admin_roles` (Theory ×4) | TC_M20_015 | Pass |
| `Unknown_role_has_no_permissions` | TC_M20_016 | Pass |

## Lệnh chạy
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --filter "FullyQualifiedName~M20"
# → Passed 16, Failed 0, Skipped 0 (Theory expansions)
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo --no-restore
# → Passed 138, Failed 0, Skipped 0 (toàn suite, gồm M20)
dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo -p:OutputPath=.qa/build/session8-web/
# → 0 Warning, 0 Error
```

## Kết quả
- **Pass:** 16 case M20 mới; toàn suite **138/138**.
- **Fail:** 0. **Skipped:** 0.
- **Blocked (chưa có harness):**
  - HTTP runtime probe (header thật, 401/403, swagger public prod, /health, /hangfire gate) — cần WebApplicationFactory.
  - JWT revoke E2E khi khóa user/đổi role (OBS-M20-05).
  - Deploy production thật (Docker non-root, Data Protection persist, reverse proxy TLS, secret env) — cần môi trường production.

## Automation backlog
1. WebApplicationFactory smoke: assert 4 security headers có mặt; `/swagger` 404 khi Production (sau khi gate); `/hangfire` 302→login khi anonymous; `/api/*` 401 khi thiếu token, 403 khi sai permission.
2. Integration: JWT sau khi user bị khóa → 401 (nếu chọn thêm stamp check — U-M20-2).
3. Deploy smoke: `/health` 200; container user != root; DP keys persist qua restart.

## Ghi chú trung thực (không tuyên bố 100%)
- M20 automated CHỈ phủ bất biến RBAC seed. Toàn bộ pipeline/deployment là static/config review. Không dựng runtime HTTP/deploy harness trong phiên này → residual runtime ghi rõ ở 03/04/06.
