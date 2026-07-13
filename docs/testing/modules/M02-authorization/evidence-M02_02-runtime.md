# BUG_M02_02 — Bằng chứng RUNTIME (PoC không phá hoại)

> PoC chạy thật trên môi trường **Local** (`http://localhost:5177`, dev server đang chạy) — chỉ thao tác **đọc** (login + GET). Không sửa/xóa dữ liệu. Thực hiện lúc ~15:30 2026-07-10.
> **Không** lưu token/mật khẩu ở đây (theo nguyên tắc bảo mật tài liệu). Tài khoản demo dùng mật khẩu chung theo README.

## Cách tái hiện
1. `POST /api/auth/login` (AllowAnonymous) bằng tài khoản phạm vi hẹp → nhận JWT.
2. `GET /api/candidates?page=1&pageSize=100` kèm `Authorization: Bearer <jwt>`.
3. Đếm số ứng viên trả về so với phạm vi đáng lẽ được thấy.

## Kết quả quan sát

| Tài khoản (demo) | Role (trong JWT) | `candidates:read` | `GET /api/candidates` | Ứng viên trả về | Phạm vi ĐÁNG LẼ |
|---|---|---|---|---|---|
| `admin@polymind.local` | super_admin | ✔ (100 perms) | HTTP 200 | **total=18** | tất cả (đúng) |
| `ctv-ctv0001@polymind.local` | collaborator | ✔ | HTTP 200 | **total=18, trả 18** | chỉ ứng viên CTV này giới thiệu |
| `hocvien-uv202606082001@polymind.local` | **student** | ✔ | HTTP 200 | **total=18, trả 18** | **đúng 1 hồ sơ của chính mình** |
| `phuhuynh-uv202606082001@polymind.local` | **parent** | ✔ | HTTP 200 | **total=18, trả 18** | **đúng 1 hồ sơ của con/em** |
| `agent@polymind.local` | *(không có role trong DB hiện tại)* | ✘ | HTTP 403 | — | (403 vì tài khoản này đang không có role — xem ghi chú) |

- Trường trả về gồm: `id, code, fullName, phone, province, gender, passportNumber, createdAt` → **số hộ chiếu (PII) bị lộ** cho parent/student/collaborator.
- **Kết luận:** BUG_M02_02 **XÁC NHẬN RUNTIME** — tài khoản cổng ngoài (phụ huynh/học viên) và CTV đọc được **toàn bộ 18 ứng viên** qua REST API, trong khi web UI chỉ cho thấy đúng phạm vi. `AgentScope` không được áp ở `ResourceEndpoints`.

## Ghi chú phụ (không thuộc BUG_M02_02)
- **`agent@polymind.local` hiện KHÔNG có role** (JWT `roles=[]`, `permissions=[]`) → trả 403. Đây là **bất thường dữ liệu** của DB dev hiện tại (theo `DbSeeder`, tài khoản này lẽ ra có role `agent`). KHÔNG kết luận là bug code (seeder `EnsureSeedUserAsync` vẫn `AddToRoleAsync` đúng); ghi nhận để kiểm ở M03/M20 (có thể role bị gỡ tay, hoặc seed chưa hoàn tất). Nếu agent có role `agent` (có `candidates:read`) thì cũng sẽ IDOR như collaborator.
- **Hệ quả mở rộng:** vì `candidates:read` mở cho parent/student/agent/collaborator, mọi tài khoản này (kể cả người ngoài công ty) đều khai thác được. Ưu tiên sửa cao.
