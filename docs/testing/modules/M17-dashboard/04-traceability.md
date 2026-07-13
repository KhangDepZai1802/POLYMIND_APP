# M17 — Dashboard · Traceability

| Business Flow | Page | Role | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M17-01 Home KPI | Home.razor | staff | TC_M17_001/002/006/020/021/022 | shared permission regression | CR-M17-1 Fixed by Codex — chờ Claude | Runtime bUnit + DB |
| BF-M17-02 Portal `/me` | Overview.razor | parent/student | TC_M17_003/004/023 | — | Source-verified (scope) | Runtime bUnit |
| BF-M17-03 Phân quyền | dashboard:read + redirect | all | TC_M17_001..005 | — | Source-verified (seed + redirect) | Runtime 403/redirect probe |
| BF-M17-04 KPI tài chính | Home.razor | finance vs non-finance staff | TC_M17_010/011 | `financial_reports:read` registry/access regression | Fixed: UI + query path cùng fail-closed | Runtime role render + SQL/query probe |

## Gap analysis
- **Automation:** M17 (Home/Overview) ở `Polymind.Web` + cần DB → chưa có component test riêng. CR-M17-1 tái sử dụng policy `financial_reports:read` đã có 6 access/range tests M16; không thêm test giả chỉ kiểm constant.
- **Runtime:** cần DB + đăng nhập theo role để kiểm redirect partner, self-scope `/me`, số liệu KPI.
