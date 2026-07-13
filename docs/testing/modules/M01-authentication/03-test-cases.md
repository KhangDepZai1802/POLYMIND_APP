# M01 — Authentication & Session · Test Cases

Quy ước ID: `TC_M01_<NNN>`. Automation Layer: **Unit** (xUnit trên IdentityOptions/logic thuần), **Integration** (WebApplicationFactory — chưa có harness), **Manual** (UI/DB thủ công).

| TC | Tên | BF | Nguồn | Loại | Prio | Severity nếu fail | Role | Preconditions | Test Data | Expected UI | Expected API | Expected DB | Automation | Layer | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| TC_M01_001 | Đăng nhập web đúng | BF-01 | Login.razor | Functional/Happy | P1 | Critical | any | admin active | admin@polymind.local/Admin@123 | Redirect `/` | — | last_login_at cập nhật | Manual | Manual | Not Run |
| TC_M01_002 | Sai mật khẩu báo lỗi chung | BF-02 | Login.razor:105 | Negative | P1 | High | any | tài khoản tồn tại | admin@.../sai | "Email hoặc mật khẩu không đúng" | — | access_failed_count +1 | Manual | Manual | Not Run |
| TC_M01_003 | Lockout sau 5 lần sai | BF-02 | DI.cs:35-37 | Security | P1 | High | any | tài khoản tồn tại | 5× sai | "tạm khóa ... 15 phút" | — | lockout_end set | Unit (config) + Manual | Unit/Manual | **Pass (config)** |
| TC_M01_004 | Email không tồn tại | BF-02 | Login.razor:80-84 | Negative | P2 | Medium | — | — | zzz@x/y | "Email hoặc mật khẩu không đúng" | — | không đổi | Manual | Manual | Not Run |
| TC_M01_005 | IsActive=false chặn login | BF-03 | Login.razor:86-90 | Security | P1 | High | any | tài khoản đã khóa | locked user | "Tài khoản đang bị khóa" | — | không cấp cookie | Manual | Manual | Not Run |
| TC_M01_006 | Enumeration: thông báo khác nhau | BF-02/03 | Login.razor | Security | P2 | Low | — | 1 active,1 locked,1 none | 3 tài khoản | Thông báo phải GIỐNG nhau (kỳ vọng) → hiện KHÁC | — | — | Manual | Manual | **Fail → BUG_M01_02** |
| TC_M01_007 | Validation email rỗng | BF-01 | Login.razor:112 | Boundary | P2 | Low | — | — | "" | "Vui lòng nhập email" | — | — | Manual | Manual | Not Run |
| TC_M01_008 | Validation email sai định dạng | BF-01 | Login.razor:113 | Boundary | P2 | Low | — | — | "abc" | "Email không hợp lệ" | — | — | Manual | Manual | Not Run |
| TC_M01_009 | Mật khẩu policy ≥8 digit/upper/lower | — | DI.cs:28-32 | Security | P1 | High | admin | tạo user | "weak" | tạo thất bại | — | — | Unit | Unit | **Pass** |
| TC_M01_010 | Unique email | — | DI.cs:33 | Boundary | P2 | Medium | admin | — | email trùng | tạo thất bại | — | — | Unit | Unit | **Pass** |
| TC_M01_011 | API login đúng cấp JWT | BF-04 | AuthEndpoints:15 | Functional | P1 | Critical | any | active | POST email/pass | — | 200 TokenResponse, exp≈240' | last_login_at | Integration | Integration | Blocked (no harness) |
| TC_M01_012 | API login sai → 401 | BF-04 | AuthEndpoints:35 | Negative | P1 | High | — | — | sai | — | 401 | count+1 | Integration | Integration | Blocked |
| TC_M01_013 | API login IsActive=false → 403 | BF-04 | AuthEndpoints:28 | Security | P1 | High | — | locked | — | — | 403 | — | Integration | Integration | Blocked |
| TC_M01_014 | API login lockout → 423 | BF-04 | AuthEndpoints:33 | Security | P1 | High | — | 5× sai | — | — | 423 Locked | lockout_end | Integration | Integration | Blocked |
| TC_M01_015 | API thiếu field → 400 | BF-04 | AuthEndpoints:21 | Boundary | P2 | Medium | — | — | {} | — | 400 | — | Integration | Integration | Blocked |
| TC_M01_016 | `/api/auth/me` không token → 401 | BF-04 | AuthEndpoints:49 | Security | P1 | High | — | — | no header | — | 401 | — | Integration | Integration | Blocked |
| TC_M01_017 | `/api/auth/me` token hợp lệ | BF-04 | AuthEndpoints:49 | Functional | P2 | Medium | any | có JWT | Bearer | — | 200 UserInfo (roles+perms) | — | Integration | Integration | Blocked |
| TC_M01_018 | JWT exp = 240' | BF-04 | JwtTokenService:68 | Functional | P2 | Medium | any | — | decode token | — | exp - iat ≈ 240' | claim exp | Integration (JwtOptions ở Web, không ref được) | Integration | Blocked (no harness) |
| TC_M01_019 | Logout xóa cookie + AI session | BF-05 | Program.cs:244 | Functional | P1 | High | any | đã login | POST /Account/Logout | Redirect /login | 302 | AiSessionStore.Clear | Manual | Manual | Not Run |
| TC_M01_020 | Khóa tài khoản → phiên đang mở bị chấm dứt | BF-06 | ToggleUserAsync + Revalidate | Security | P1 | High | admin | user đang đăng nhập ở tab khác | admin khóa | user bị đá ≤30' | — | security_stamp phải đổi | Manual/Integration | Integration | **Fail → BUG_M01_01** |
| TC_M01_021 | Đổi mật khẩu → phiên cũ bị revalidate | BF-06 | Revalidate | Security | P2 | Medium | self | đang login 2 tab | đổi pass | tab kia bị đá ≤30' | — | security_stamp đổi | Manual | Manual | Not Run |
| TC_M01_022 | Cookie HttpOnly + Secure(prod) | — | Program.cs:110-114 | Security | P2 | Medium | any | — | inspect cookie | HttpOnly=true; SameSite=Lax | — | — | Manual | Manual | Not Run |
| TC_M01_023 | Timezone LastLoginAt UTC | BF-01 | Login.razor:96 | Database | P3 | Low | any | login | — | — | — | last_login_at là UTC offset 0 | Manual (DB) | Manual | Not Run |
| TC_M01_024 | Double submit form login | BF-01 | Login.razor | UI | P3 | Low | any | — | 2× submit nhanh | 1 phiên, không lỗi | — | — | Manual | Manual | Not Run |

## Gap analysis

- **Đã bao phủ tự động (Unit):** cấu hình lockout (5/15'), password policy (≥8, digit/upper/lower), unique email, JWT expiry 240' — đây là các hằng số bảo mật cốt lõi có thể kiểm chứng không cần DB.
- **Chưa tự động (Integration, blocked):** toàn bộ luồng HTTP `/api/auth/*` cần `WebApplicationFactory` + test DB riêng (chưa dựng harness — dev DB đang là DB thật của app, tránh side effect). Ghi backlog.
- **Manual bắt buộc:** UI `/login`, logout, cookie flags, hành vi khóa-đá-phiên (BUG_M01_01), timezone DB.
- **Rủi ro còn lại:** không có audit đăng nhập/đăng xuất; JWT không thu hồi được trong 240'.
