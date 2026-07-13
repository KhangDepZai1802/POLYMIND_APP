# M01 — Authentication & Session · Traceability

| BF ID | Page | API | Role | State | Test Case IDs | Automated Test IDs | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M01-01 Login web OK | `/login` | — | any | Anon→Auth | TC_M01_001, 007, 008, 023, 024 | — | Manual | Chưa tự động (UI SSR) |
| BF-M01-02 Sai pass / lockout | `/login` | — | any | Anon→Locked | TC_M01_002, 003, 004, 006 | `M01_...Lockout_is_5_attempts_15_minutes` | Config tự động + manual UI | Hành vi runtime lockout cần manual/integration |
| BF-M01-03 IsActive=false | `/login` | — | any | Anon | TC_M01_005, 006 | — | Manual | — |
| BF-M01-04 API login JWT | — | `/api/auth/login`, `/me` | any | Anon→JWT | TC_M01_011–018 | — | Integration blocked | Cần WebApplicationFactory + test DB |
| BF-M01-05 Logout | — | `/Account/Logout` | any | Auth→Anon | TC_M01_019 | — | Manual | — |
| BF-M01-06 Revalidate stamp | — | — | any | Auth→Revoked | TC_M01_020, 021 | — | Manual/Integration | BUG_M01_01 |
| (config) Password policy | — | — | admin | — | TC_M01_009, 010 | `Password_policy_requires_length8...`, `User_requires_unique_email`, `SignIn_does_not_require_confirmed_account` | Tự động | — |

## Độ phủ tổng hợp

- **Test case tạo:** 24 (TC_M01_001 → 024).
- **Automated (Unit, chạy PASS):** 4 test method / phủ TC_M01_003, 009, 010 + 1 bổ sung (RequireConfirmedAccount).
- **Manual cần chạy:** 11 (UI, cookie, logout, DB timezone, hành vi khóa-đá-phiên).
- **Integration blocked:** 8 (toàn bộ REST `/api/auth/*`) — thiếu harness.
- **Bug phát hiện:** 2 (BUG_M01_01 High, BUG_M01_02 Low).
- **Phạm vi CHƯA kiểm:** thu hồi JWT, audit đăng nhập, đa tab/đa thiết bị, hết hạn cookie 8h thực tế.
