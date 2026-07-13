# M16 — Reports & Export · Traceability

| Business Flow | Endpoint/Page | Role | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M16-01 Xem báo cáo | Reports.razor | Director/Accountant/SuperAdmin; RM recruitment-only | TC_M16_003/006/014 | M16 access rules | **CR-M16-1 Fixed by Codex — chờ Claude** | Runtime bUnit + DB |
| BF-M16-02 Export file | `/export/*?from&to` | split recruitment/financial | TC_M16_001/006/010/011/012/013/032 | 6 M16 rules | **BUG_M16_01 Fixed by Codex — chờ Claude** | Runtime download/content |
| BF-M16-03 Receipt PDF | `/receipts/{id}.pdf` | finance | TC_M16_002/020/021/030 | — | Source-verified; OBS-M16-01 | Runtime IDOR probe |
| BF-M16-04 Phân quyền | DbSeeder + endpoint re-check | all | TC_M16_001..006 | exact slug/permission rules | **Fixed by Codex — chờ Claude** | Runtime 403 probe |

## Gap analysis
- **Automation:** 6 Domain/registry tests cho inclusive range/query string, invalid range, backward-compatible all-time URL, RM-vs-finance slug matrix và permission registry. Endpoint/file content vẫn cần integration.
- **Runtime:** cần đăng nhập theo role + DB để tải file thật; kiểm nội dung file khớp range.
