# M20 — Security & Deployment · Bug Report

## Kết luận: **No Confirmed Bugs** (10 observations hardening + 2 requirement clarification)

Không phát hiện lỗ hổng khai thác được (confirmed exploitable defect) trong bối cảnh hiện tại:
- Auth/AuthZ (M01/M02) đã Verified; IDOR web/REST/MinIO (M02/M05/M18) fail-closed đã Verified.
- Bất biến RBAC seed chống leo thang quyền dọc: **Pass** (`M20_SecurityInvariantsTests` 16/16) — partner/portal không có quyền nhạy cảm.
- Secret không nằm trong config tracked (trừ Gemini dev key — không push repo public); production không seed tài khoản mẫu; thiếu `Jwt:Key` prod → throw chặn deploy.

Các mục dưới là **hardening/observation** — phần lớn được deploy plan hoãn tới production thật (giai đoạn hiện tại: TEST qua Cloudflare Tunnel). Không có mục nào là exploit đã chứng minh ở trạng thái hiện tại.

## Observations

| ID | Mức | Vấn đề | Bối cảnh giảm nhẹ | Đề xuất |
|---|---|---|---|---|
| OBS-M20-01 | Low-Med | Thiếu `Content-Security-Policy` | Blazor Server render server-side; `X-Frame-Options=SAMEORIGIN` giảm clickjacking | Thêm CSP restrictive trước prod |
| OBS-M20-02 | Med (direct) / Low (tunnel) | `ForwardedHeaders.KnownProxies.Clear()` tin mọi proxy → có thể giả `X-Forwarded-For` | An toàn sau Cloudflare/Caddy (chỉ proxy nói chuyện với app) | Set `KnownProxies`/`KnownNetworks` theo reverse proxy prod |
| OBS-M20-03 | Low | `/swagger` public mọi env (kể cả Production) | Chỉ lộ schema; endpoint vẫn JWT-gated | Gate `if (app.Environment.IsDevelopment())` cho Swagger |
| OBS-M20-04 | Med | Không rate limit API/login/Gemini | Login có Identity lockout 5/15'; API scoped | Thêm `AddRateLimiter` cho `/api/auth/login` + Gemini trước prod |
| OBS-M20-05 | Med | JWT không kiểm security-stamp → không revoke tức thì khi khóa user/đổi role | Expiry 4h; cookie path revoke đúng | **U-M20-2:** thêm stamp check cho Bearer hoặc rút ngắn expiry |
| OBS-M20-06 | Low | `AllowedHosts="*"` | Host header abuse hạn chế khi sau proxy | Siết theo domain prod |
| OBS-M20-07 | Low-Med | Data Protection keys in-memory → cookie/antiforgery invalid sau redeploy/multi-instance | Single-instance test OK | Persist DP keys (file/DB/Redis) khi lên prod/multi-instance |
| OBS-M20-08 | Low | Container chạy root | Chấp nhận giai đoạn test | Thêm `USER` non-root trong Dockerfile |
| OBS-M20-09 | Med (nếu push public) | Gemini key trong `appsettings.Development.json` (tracked git) | Repo private; memory ghi rõ không push public | Chuyển sang user-secrets/env; không push public |
| OBS-M20-10 | Info | Không cấu hình CORS (API same-origin) | Restrictive = an toàn | Nếu mở consumer cross-origin, cấu hình CORS whitelist |

> **Đính chính analysis:** `01-analysis.md` mục 3.6 ghi "DbSeeder **throw** nếu prod thiếu super_admin" — thực tế `DbSeeder.cs:177-181` là `LogError` + **skip tạo account** (không throw). Kết quả bảo mật vẫn đúng (không lộ default credential); chỉ khác về availability (app boot không có super_admin cho tới khi đặt env). Không phải bug.

## Needs Requirement Clarification

- **U-M20-1 (Go-live hardening checklist):** Trước production thật, hardening nào **bắt buộc**? Đề xuất tối thiểu: CSP (OBS-01), gate Swagger prod (OBS-03), rate limit login/API (OBS-04), KnownProxies (OBS-02), Data Protection persist (OBS-07), non-root container (OBS-08), Gemini key ra khỏi tracked config (OBS-09), siết AllowedHosts (OBS-06). Cần user chốt scope + thứ tự.
- **U-M20-2 (JWT revoke):** Có cần revoke JWT tức thì khi khóa user/đổi role (thêm security-stamp validation cho Bearer) hay chấp nhận cửa sổ 4h expiry? Ảnh hưởng REST API consumer.

## Codex Handoff Queue (M20)

| Order | ID | Severity | Suspected Area | Required Files | Status |
|---:|---|---|---|---|---|
| — | — | — | — | — | **Không có bug cần fix.** Observations chờ U-M20-1/2 user chốt trước khi Codex thực thi hardening. |

## Trạng thái
→ **QA Status = No Confirmed Bugs**, **Codex Status = Not Required** (chờ user chốt U-M20-1/2 mới handoff hardening), **Verification Status = Verified (code/config static + unit)**.
