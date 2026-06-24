# POLYMIND — Overseas Labor Management System

## 🔑 Tài khoản demo theo vai trò

> Tất cả tài khoản dùng chung mật khẩu: **`Admin@123`**. Đăng nhập tại `http://localhost:5177/login`.
> (Nguồn: `src/Polymind.Infrastructure/Persistence/DbSeeder.cs` — tự seed khi khởi động.)

| Vai trò (role) | Email | Quyền chính |
|---|---|---|
| **Super Admin** (`super_admin`) | `admin@polymind.local` | Toàn quyền mọi module |
| **Giám đốc** (`director`) | `director@polymind.local` | Xem tất cả + duyệt thu/chi/hoa hồng + báo cáo |
| **Trưởng phòng tuyển dụng** (`recruitment_manager`) | `recruitment.manager@polymind.local` | CRUD Lead, quản lý ứng viên, xem đơn hàng/đại lý/báo cáo |
| **Nhân viên tuyển dụng** (`recruiter`) | `recruiter@polymind.local` | Tạo/sửa Lead & ứng viên, xem đơn hàng/đại lý |
| **Bộ phận hồ sơ** (`document_staff`) | `document.staff@polymind.local` | Xem/sửa ứng viên, xem Lead/đơn hàng/visa |
| **Bộ phận visa** (`visa_staff`) | `visa.staff@polymind.local` | Toàn quyền Visa & Vé máy bay, xem/sửa ứng viên |
| **Kế toán** (`accountant`) | `accountant@polymind.local` | Toàn quyền Thu/Chi/Phiếu/Hoa hồng, xem ứng viên/đại lý/báo cáo |
| **Đại lý / CTV** (`agent`) | `agent@polymind.local` | Portal đại lý: chỉ xem ứng viên mình giới thiệu + hoa hồng của mình |

> **Lưu ý Portal đại lý:** tài khoản `agent@` được seed gắn vào đại lý **"Đại lý Miền Bắc" (AG-000001)**; khi đăng nhập chỉ thấy ứng viên/hoa hồng thuộc đại lý này (không thấy dashboard/báo cáo toàn công ty).

## Tài Liệu Thiết Kế Hệ Thống

| File | Nội dung |
|---|---|
| [01-business-analysis.md](01-business-analysis.md) | Phân tích nghiệp vụ, actor, module, phase triển khai |
| [02-database-design.md](02-database-design.md) | Schema PostgreSQL đầy đủ, ERD, index strategy |
| [03-workflow.md](03-workflow.md) | Workflow Lead → Ứng viên → Xuất cảnh, thông báo tự động, KPI |
| [04-system-architecture.md](04-system-architecture.md) | Kiến trúc hệ thống, API design, tech stack chi tiết |

## Tech Stack Đã Chọn

| Layer | Công nghệ |
|---|---|
| Frontend | Next.js 14 (App Router) + TypeScript + Tailwind CSS + shadcn/ui |
| State | TanStack Query + Zustand |
| Forms | React Hook Form + Zod |
| Backend | NestJS (Node.js) + TypeScript |
| ORM | Prisma |
| Database | PostgreSQL 16 |
| Cache/Queue | Redis + Bull |
| File Storage | MinIO (S3-compatible) |
| Auth | JWT (access + refresh token) |
| PDF | Puppeteer |
| Excel | ExcelJS |
| Notifications | SMTP + eSMS + Zalo OA API |
| Deploy | Docker Compose + Nginx |

## Phát Triển Theo Phase

- **Phase 1 (MVP):** Auth, Lead CRM, Ứng viên, Đơn hàng, Workflow, Dashboard cơ bản
- **Phase 2:** Tài chính, Hoa hồng đại lý, Portal đại lý
- **Phase 3:** Visa, Xuất cảnh, Báo cáo đầy đủ, Thông báo tự động
- **Phase 4:** Tích hợp Facebook/TikTok/Zalo API, OCR, AI, Mobile App
