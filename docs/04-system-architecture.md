# Kiến Trúc Hệ Thống — POLYMIND OLMS

## 1. Tổng Quan Kiến Trúc

```
┌─────────────────────────────────────────────────────────────────────┐
│                         INTERNET                                    │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ HTTPS
                               ▼
                    ┌──────────────────┐
                    │    Nginx         │  Reverse Proxy + SSL
                    │  (Load Balancer) │  Static file serving
                    └────────┬─────────┘
                             │
                ┌────────────┴────────────┐
                ▼                         ▼
    ┌──────────────────┐      ┌──────────────────┐
    │  Next.js Frontend│      │  NestJS Backend   │
    │  (SSR / CSR)     │      │  REST API         │
    │  Port: 3000      │      │  Port: 4000       │
    └──────────────────┘      └────────┬──────────┘
                                       │
                    ┌──────────────────┼─────────────────────┐
                    ▼                  ▼                      ▼
         ┌─────────────────┐  ┌──────────────┐  ┌──────────────────┐
         │   PostgreSQL    │  │    Redis      │  │  MinIO / S3      │
         │   (Primary DB)  │  │  (Cache +     │  │  (File Storage)  │
         │   Port: 5432    │  │   Queue)      │  │  Port: 9000      │
         └─────────────────┘  └──────────────┘  └──────────────────┘
                                       │
                              ┌────────┴────────┐
                              ▼                 ▼
                    ┌─────────────────┐  ┌──────────────────┐
                    │  Bull Queue     │  │  Notification     │
                    │  (Job Scheduler)│  │  Services:        │
                    └─────────────────┘  │  - Email (SMTP)   │
                                         │  - SMS (eSMS)     │
                                         │  - Zalo OA API    │
                                         └──────────────────┘
```

---

## 2. Frontend Architecture (Next.js 14)

### Cấu trúc thư mục
```
src/
├── app/                        # App Router
│   ├── (auth)/                 # Auth pages (không cần layout chung)
│   │   ├── login/
│   │   └── agent-portal/login/
│   ├── (dashboard)/            # Main app
│   │   ├── layout.tsx          # Sidebar + Topbar layout
│   │   ├── dashboard/
│   │   ├── leads/
│   │   │   ├── page.tsx        # Danh sách leads
│   │   │   ├── [id]/page.tsx   # Chi tiết lead
│   │   │   └── create/page.tsx
│   │   ├── candidates/
│   │   ├── job-orders/
│   │   ├── finance/
│   │   │   ├── payments/
│   │   │   ├── expenses/
│   │   │   └── receipts/
│   │   ├── agents/
│   │   ├── commissions/
│   │   ├── visa/
│   │   ├── reports/
│   │   └── settings/
│   └── agent-portal/           # Portal đại lý (domain/path riêng)
│       ├── dashboard/
│       ├── candidates/
│       └── commissions/
├── components/
│   ├── ui/                     # shadcn/ui components
│   ├── layout/                 # Sidebar, Topbar, Breadcrumb
│   ├── leads/                  # Lead-specific components
│   ├── candidates/             # Candidate-specific components
│   ├── finance/
│   └── shared/                 # DataTable, FileUpload, StatusBadge...
├── lib/
│   ├── api/                    # API client (axios/fetch wrappers)
│   ├── auth/                   # NextAuth config
│   └── utils/
├── hooks/                      # Custom React hooks
├── stores/                     # Zustand stores
└── types/                      # TypeScript interfaces
```

### State Management
- **Server State:** TanStack Query (React Query) — fetching, caching, invalidation
- **Client State:** Zustand — UI state, filters, selections
- **Forms:** React Hook Form + Zod validation

---

## 3. Backend Architecture (NestJS)

### Module Structure
```
src/
├── main.ts
├── app.module.ts
├── modules/
│   ├── auth/                   # JWT, RBAC guards, decorators
│   ├── users/
│   ├── roles/
│   ├── leads/
│   ├── lead-activities/
│   ├── candidates/
│   ├── candidate-documents/
│   ├── job-orders/
│   ├── workflows/
│   ├── finance/
│   │   ├── payments/
│   │   ├── receipts/
│   │   └── expenses/
│   ├── agents/
│   ├── commissions/
│   ├── visas/
│   ├── flights/
│   ├── notifications/
│   ├── reports/
│   └── audit/
├── common/
│   ├── decorators/             # @CurrentUser, @Roles, @Permissions
│   ├── guards/                 # JwtAuthGuard, RbacGuard
│   ├── interceptors/           # AuditLogInterceptor, TransformInterceptor
│   ├── filters/                # HttpExceptionFilter
│   ├── pipes/                  # ValidationPipe, ParseUUIDPipe
│   └── dto/                    # PaginationDto, FilterDto
├── config/                     # Database, Redis, Storage configs
└── jobs/                       # Bull job processors (notifications)
```

### API Design
- **Base URL:** `/api/v1/`
- **Auth:** Bearer JWT trong Authorization header
- **Pagination:** `?page=1&limit=20`
- **Filter:** `?status=active&search=keyword&dateFrom=...&dateTo=...`
- **Sort:** `?sortBy=created_at&sortOrder=desc`

### Key API Endpoints
```
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh

GET    /api/v1/leads               # Danh sách leads (filter, paginate)
POST   /api/v1/leads               # Tạo lead mới
GET    /api/v1/leads/:id
PATCH  /api/v1/leads/:id
PATCH  /api/v1/leads/:id/status    # Cập nhật trạng thái
POST   /api/v1/leads/:id/convert   # Chuyển thành ứng viên
GET    /api/v1/leads/:id/activities

GET    /api/v1/candidates
POST   /api/v1/candidates
GET    /api/v1/candidates/:id
PATCH  /api/v1/candidates/:id
GET    /api/v1/candidates/:id/workflow
PATCH  /api/v1/candidates/:id/workflow/:step
POST   /api/v1/candidates/:id/documents   # Upload file
GET    /api/v1/candidates/:id/payments

GET    /api/v1/job-orders
POST   /api/v1/job-orders

GET    /api/v1/finance/payments
POST   /api/v1/finance/payments
PATCH  /api/v1/finance/payments/:id/confirm

GET    /api/v1/agents
GET    /api/v1/agents/:id/commissions
PATCH  /api/v1/commissions/:id/approve
PATCH  /api/v1/commissions/:id/pay

GET    /api/v1/dashboard/overview
GET    /api/v1/dashboard/revenue
GET    /api/v1/reports/leads
GET    /api/v1/reports/recruitment
GET    /api/v1/reports/finance
GET    /api/v1/reports/agents
```

---

## 4. Database Layer

- **ORM:** Prisma (type-safe, migrations tốt, schema.prisma rõ ràng)
- **Connection Pool:** Prisma default pool
- **Migrations:** `prisma migrate dev` / `prisma migrate deploy`
- **Seeding:** `prisma db seed`

---

## 5. Authentication & Authorization

### JWT Flow
```
Login → backend tạo:
  access_token (expires: 15m)
  refresh_token (expires: 7d, lưu trong Redis)

Request → Authorization: Bearer <access_token>
                              │
                         JwtAuthGuard verify
                              │
                         RbacGuard check permission
                              │
                         Controller/Service
```

### RBAC Implementation
```typescript
// Decorator trên controller
@Permissions('leads:create')
@Post()
async createLead() {}

// Guard kiểm tra
// user.role.permissions.includes(requiredPermission)
```

---

## 6. File Storage

- **Storage:** MinIO (self-hosted, S3-compatible) hoặc AWS S3
- **Flow:**
  1. Frontend request presigned URL từ backend
  2. Frontend upload trực tiếp lên MinIO/S3
  3. Frontend gửi file_url về backend để lưu DB
- **Folder structure:**
  ```
  /candidates/{candidate_id}/documents/{doc_type}/{version}/{filename}
  /receipts/{year}/{month}/{receipt_code}.pdf
  /agents/{agent_id}/contracts/{filename}
  ```
- **Access:** Private bucket, serve qua presigned URL (expire 1h)

---

## 7. Notification System

### Queue-based với Bull + Redis
```
Event trigger (e.g., payment due)
        │
        ▼
notification.service.schedule(job)
        │
        ▼
Bull Queue: 'notifications'
        │
        ▼
Worker picks up job
        │
   ┌────┴──────────────────┐
   ▼         ▼             ▼
Email      SMS           Zalo OA
(SMTP/     (eSMS API/    (Zalo API)
SendGrid)  Viettel SMS)
```

---

## 8. Realtime Dashboard

- **Approach:** Polling với React Query (`refetchInterval: 30000`) cho dashboard
- **Hoặc:** Server-Sent Events (SSE) cho notifications in-app
- WebSocket chỉ cần nếu có yêu cầu realtime cao (Phase 2)

---

## 9. PDF Generation

- **Backend:** `puppeteer` hoặc `@react-pdf/renderer` để generate phiếu thu chi
- Lưu vào MinIO, trả URL về frontend để download

---

## 10. Export Excel

- **Backend:** `exceljs` — generate xlsx từ query data
- Stream response về client

---

## 11. Infrastructure (Docker)

```yaml
# docker-compose.yml
services:
  nginx:
    image: nginx:alpine
    ports: ["80:80", "443:443"]

  frontend:
    build: ./frontend
    environment:
      NEXT_PUBLIC_API_URL: http://backend:4000

  backend:
    build: ./backend
    environment:
      DATABASE_URL: postgresql://...
      REDIS_URL: redis://redis:6379

  postgres:
    image: postgres:16-alpine
    volumes: [pgdata:/var/lib/postgresql/data]

  redis:
    image: redis:7-alpine

  minio:
    image: minio/minio
    command: server /data --console-address ":9001"
    volumes: [minio_data:/data]
```

---

## 12. Security Checklist

- [ ] HTTPS everywhere (Let's Encrypt)
- [ ] JWT secret key rotation
- [ ] Refresh token blacklist trong Redis khi logout
- [ ] Rate limiting (NestJS ThrottlerModule)
- [ ] Input validation tất cả endpoints (class-validator)
- [ ] SQL injection: Prisma ORM ngăn chặn hoàn toàn
- [ ] XSS: React escape HTML mặc định
- [ ] File upload: validate mime type, size limit, virus scan
- [ ] CORS: whitelist domain cụ thể
- [ ] Audit log: mọi thao tác CRUD đều được log
- [ ] Sensitive data (CCCD, bank account): encrypt at rest

---

## 13. Scalability Path

| Giai đoạn | Setup |
|---|---|
| Phase 1 (MVP) | 1 server VPS, Docker Compose, PostgreSQL single node |
| Phase 2 | PostgreSQL read replica cho báo cáo, Redis Cluster |
| Phase 3 | Tách microservices nếu cần (Notification service riêng) |
| Phase 4 | Kubernetes, CDN cho static assets |
