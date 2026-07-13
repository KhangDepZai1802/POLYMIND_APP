# M19 — Audit Log · Automation Report

## Framework & dependency
- **Test framework:** xUnit (`tests/Polymind.Tests`).
- **Ref:** `Polymind.Domain` + `Polymind.Infrastructure` (KHÔNG ref `Polymind.Web` — tránh khóa DLL khi dev server chạy).
- **Hệ quả:** `AuditLogHelpers.AddAudit` + trang `Admin.razor` (logic ghi/đọc/label) nằm ở `Polymind.Web` → **không unit-test trực tiếp**; chỉ contract-test `AuditLog` entity (Domain).

## Test structure
| File | Test | Test Case ID |
|---|---|---|
| `tests/Polymind.Tests/M19_AuditLogTests.cs` | `New_audit_log_defaults_optional_fields_to_null` | TC_M19_030 |
| " | `New_audit_log_generates_id_and_created_at` | TC_M19_031 |
| " | `Audit_log_stores_action_resource_and_json_values` | TC_M19_032 |

## Lệnh chạy
```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả
- **Passed: 101, Failed: 0, Skipped: 0** (toàn suite; gồm 3 test M19 mới + các test module khác, có 4 test do phiên Codex song song thêm cùng lúc).
- **M19 mới:** 3/3 pass.
- **Web build:** `dotnet build src/Polymind.Web/Polymind.Web.csproj --nologo` → **0 Warning, 0 Error** (đã build khi verify M15 cùng phiên).

## Phân loại
- **Application Defect:** 0 confirmed.
- **Test Code Defect:** 0.
- **Environment Defect:** build Web với `-p:OutputPath=` custom gây MSB4018 (GenerateDepsFile) — tránh, build thường OK.
- **Test Data Defect:** 0.
- **Requirement Ambiguity:** U-M19-1 (login/logout + Ip/UA), U-M19-2 (paging/range/export).

## Automation backlog (khi có harness Web + DB test)
1. E2E: thao tác nghiệp vụ → xác nhận đúng 1 audit đúng actor/action/resource/resourceId.
2. Atomicity: ép SaveChanges fail → không audit mồ côi.
3. Delete resource → audit vẫn còn (ResourceId không FK).
4. Authz: role không `audit:read` → không load được nhật ký.
5. Filter/sort/Take(200) runtime.
6. Actor-null path (OBS-M19-03) — hiện chỉ static.

## Phạm vi đã đo / chưa đo
- **Đã đo (contract/static):** entity default/nullable; Ip/UA không tự set; Id/CreatedAt auto; atomicity + authz + attribution qua đọc source.
- **Chưa đo (blocked harness):** toàn bộ runtime write/read/filter; append-only DB PoC.
- **KHÔNG tuyên bố 100%.**
