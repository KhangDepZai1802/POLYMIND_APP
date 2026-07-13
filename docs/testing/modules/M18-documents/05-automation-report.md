# M18 — File Upload / Documents · Automation Report

## Framework & phạm vi
- xUnit (`tests/Polymind.Tests`), ref chỉ Domain + Infrastructure.
- `MinioDocumentStorage` + upload/download UI ở `Polymind.Web` + cần MinIO → **0 automated test cho M18**. Không thêm test giả.

## Automated tests
| Automated Test | Test Case | Kết quả |
|---|---|---|
| — | — | Không có (storage/UI ở Web + cần MinIO) |

## Lệnh chạy (suite chung — không đổi)
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed 88, Failed 0, Skipped 0 (M18 không thêm test).
```

## Phân loại phát hiện
- **Không có Application Defect** (không có bug khai thác được). Storage security đúng ở source.
- **Hardening (defense-in-depth):** OBS-M18-01 (re-check scope download), OBS-M18-02 (content-type), OBS-M18-03 (orphan MinIO).

## Blocked / pending harness
| Hạng mục | Cần |
|---|---|
| Upload validate (ext/size/path traversal) | Testcontainers MinIO |
| IDOR download probe (OBS-M18-01) | MinIO + DB + role seed |
| Content-type XSS (OBS-M18-02) | MinIO + browser |
| Orphan object khi xóa (OBS-M18-03) | MinIO |

## Automation backlog
- Tách logic `SanitizeFileName`/`BuildStoredFileName`/whitelist ra Domain/Application → unit-test path traversal + extension không cần MinIO.
- Harness MinIO (Testcontainers) cho E2E upload/download/version.
