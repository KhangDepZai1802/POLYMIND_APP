# M01 — Authentication & Session · Automation Report

## Framework & dependency

- **Test framework:** xUnit 2.9.2 + Microsoft.NET.Test.Sdk 17.11.1.
- **Project:** `tests/Polymind.Tests/Polymind.Tests.csproj` (net10.0). Tham chiếu `Polymind.Domain` + `Polymind.Infrastructure`.
- **KHÔNG tham chiếu `Polymind.Web`:** build test sẽ rebuild Web và bị khóa DLL khi dev server (:5177) đang chạy (`MSB3021 ... locked by .NET Host`). Logic thuần trong Web (JwtOptions, Login.razor) do đó chưa tự động được ở session này.

## Cấu trúc test

- `M01_AuthenticationConfigTests.cs` — dựng `ServiceCollection` + `AddInfrastructure` với connection string giả (không mở kết nối), resolve `IOptions<IdentityOptions>` và assert cấu hình bảo mật.

## Automated Test IDs → Test Case

| Automated Test | Test Case | Kiểm |
|---|---|---|
| `Lockout_is_5_attempts_15_minutes` | TC_M01_003 | AllowedForNewUsers=true, MaxFailedAccessAttempts=5, DefaultLockoutTimeSpan=15' |
| `Password_policy_requires_length8_digit_upper_lower` | TC_M01_009 | RequiredLength=8, RequireDigit/Upper/Lower=true, RequireNonAlphanumeric=false |
| `User_requires_unique_email` | TC_M01_010 | RequireUniqueEmail=true |
| `SignIn_does_not_require_confirmed_account` | (bổ sung) | RequireConfirmedAccount=false |

## Lệnh chạy

```bash
dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --nologo
```

## Kết quả (session đầu)

```
Passed!  - Failed: 0, Passed: 5, Skipped: 0, Total: 5, Duration: 157 ms
```

(5 = 4 test M01 + 1 smoke test chung.)

- **Pass:** 5
- **Fail:** 0
- **Skipped:** 0
- **Blocked (không viết được ở session này):** REST `/api/auth/*` (8 TC) — cần WebApplicationFactory + DB test riêng; hành vi khóa-đá-phiên (TC_M01_020) — cần integration/UI.
- **Environment issue:** dev web server đang chạy khóa DLL Web → không ref Web từ test.
- **Test data issue:** không (test config không cần data).

## Automation backlog (đề xuất session sau)

1. Dựng `WebApplicationFactory<Program>` trỏ vào **DB test riêng** (Testcontainers PostgreSQL hoặc DB `polymind_test`), seed tối thiểu → tự động TC_M01_011–018.
2. Test tích hợp cho BUG_M01_01: đăng nhập → khóa qua UserManager → assert security stamp phải đổi / phiên bị vô hiệu.
3. Tách logic thuần của Web (MessagingPolicy, JwtOptions default) sang project không-Web hoặc dừng dev server khi chạy full-solution test để ref được Web.
