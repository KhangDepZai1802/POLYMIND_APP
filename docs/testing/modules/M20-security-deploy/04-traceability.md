# M20 — Security & Deployment · Traceability

| Business Flow | Surface | Control | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M20-01 Login + lockout | Cookie sign-in | Identity password/lockout, message chung | TC_M20_001/002/011 | — (config) | Static + M01 Verified | Rate-limit login (OBS-M20-04) |
| BF-M20-02 Revalidate stamp | Cookie session | `IdentityRevalidatingAuthenticationStateProvider` | (M01/M02) | M01/M02 suite | Verified (M01/M02) | JWT stamp (OBS-M20-05) |
| BF-M20-03 JWT lifecycle | REST `/api/*` | Validate + config throw | TC_M20_003/004/005/019 | — (config) + M02 | Static + M02 Verified | Runtime HTTP probe (Blocked) |
| BF-M20-04 AuthZ deny | tất cả | Permission handler + seed invariants | TC_M20_012..017/030 | **M20_SecurityInvariantsTests (16)** | Unit + Static + M02/M05/M18 | Runtime 403 probe (Blocked) |
| BF-M20-05 Pipeline/headers | HTTP response | Security headers, HSTS, CSRF, exception handler | TC_M20_006..010/025/026 | — (config) | Static | CSP (OBS-01), header runtime thật (Blocked) |
| BF-M20-06 Seed & env | Startup | Prod no-demo, super_admin env, secret env | TC_M20_004/021/022/023/024 | — (config) | Static | Prod deploy thật (Blocked) |
| BF-M20-07 Container | Deploy | Docker, reverse proxy, DP keys | TC_M20_027/028 | — (config) | Static | Non-root, DP persist (OBS-07/08) |

## Coverage summary
- **Automated (Unit):** 16 case bất biến RBAC seed (chống vertical escalation) — TC_M20_012..016.
- **Static (đọc source/config thật):** cookie/JWT/headers/HSTS/CSRF/exception/seed/env/docker — TC_M20_001..011, 017, 018, 020..028, 030.
- **Blocked (Runtime/Deploy harness):** header runtime, 403/401 HTTP probe, JWT revoke E2E, prod deploy verify — TC_M20_003(runtime), 018(runtime), 029.

## Gap analysis
- Không thể unit-test Program.cs pipeline (không ref Web + không HTTP harness) → mọi kiểm pipeline là **static**. Đề xuất backlog: WebApplicationFactory smoke test (header assertion, 401/403, swagger gate) — cùng harness đã đề xuất cho M01–M19 runtime.
- Deployment (Docker non-root, DP keys, reverse proxy TLS) chỉ verify được khi dựng production thật → gộp vào U-M20-1 go-live checklist.
