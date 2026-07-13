# M02 — Authorization · Automation Report

## Framework & dependency

- xUnit 2.9.2 trên `tests/Polymind.Tests` (net10.0), tham chiếu `Polymind.Infrastructure` (chứa `PermissionRegistry`).
- Enforcement handler/policy provider và `MessagingPolicy` nằm ở `Polymind.Web` → chưa tự động được (không ref Web khi dev server đang chạy). Kiểm bằng manual + phân tích source.

## Cấu trúc test

- `M02_PermissionRegistryTests.cs` — kiểm hợp đồng từ vựng RBAC (`PermissionRegistry`) mà DbSeeder role map + `[Authorize(Policy)]` + `PermissionPolicyProvider` phụ thuộc.

## Automated Test IDs → Test Case

| Automated Test | Test Case | Kiểm |
|---|---|---|
| `Generates_20_resources_times_5_actions_100_permissions` | TC_M02_001 | 20×5=100 permission |
| `All_permission_names_are_wellformed_resource_colon_action` | TC_M02_002 | name == `resource:action` |
| `Permission_names_are_unique` | TC_M02_003 | không trùng name |
| `Actions_are_exactly_crud_plus_approve` | TC_M02_004 | create/read/update/delete/approve |
| `Registry_contains_all_resources_referenced_by_role_map` | TC_M02_005 | 20 resource app dùng đều có |
| `Resources_are_unique` | TC_M02_006 | không trùng resource |

## Lệnh chạy & kết quả

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
# Passed: 11, Failed: 0  (5 M01/smoke + 6 M02)
```

- **Pass:** 6 (M02) · **Fail:** 0 · **Skipped:** 0
- **Blocked:** enforcement handler (Web), API 401/403, JWT claim, IDOR scope — cần harness integration + ref Web.

## Automation backlog

1. Test đơn vị cho `PermissionAuthorizationHandler` (super_admin bypass, claim khớp OrdinalIgnoreCase) và `PermissionPolicyProvider.IsPermissionPolicyName` — khi ref được Web.
2. Integration: 12 role login → gọi trang/endpoint đại diện → assert 200/403 theo ma trận mục 3 của `01-analysis.md`.
3. Integration IDOR: agent JWT gọi `/api/candidates/{id_ngoài_scope}` → assert bị lọc/chặn (hiện nghi ngờ KHÔNG lọc).
4. Test reconcile: chạy `DbSeeder.SeedAsync` 2 lần trên DB test → assert idempotent + xóa permission thừa.
