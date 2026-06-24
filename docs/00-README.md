# POLYMIND — Overseas Labor Management System

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
