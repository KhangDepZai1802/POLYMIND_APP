# Thiết Kế Database — POLYMIND OLMS

**DBMS:** PostgreSQL  
**Naming convention:** snake_case, UUID primary keys

---

## ERD Tổng Quan (theo nhóm)

```
[users] ─── [roles] ─── [permissions]
   │
   ├── [leads] ─── [lead_activities]
   │       │
   │       └── [candidates] ─── [candidate_documents]
   │                   │                └── [document_versions]
   │                   ├── [candidate_job_orders] ─── [workflow_steps]
   │                   ├── [payments]
   │                   └── [visas] ─── [flights]
   │
   ├── [job_orders]
   ├── [agents] ─── [agent_commission_configs]
   │       └── [agent_commissions]
   ├── [expenses]
   ├── [notifications]
   └── [audit_logs]
```

---

## 1. Auth & Phân Quyền

### `roles`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| name | VARCHAR(50) | UNIQUE. VD: super_admin, director, recruiter |
| description | TEXT | |
| created_at | TIMESTAMPTZ | DEFAULT NOW() |

### `permissions`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| name | VARCHAR(100) | VD: leads:create, candidates:read |
| resource | VARCHAR(50) | VD: leads, candidates, finance |
| action | VARCHAR(20) | create, read, update, delete, approve |

### `role_permissions`
| Column | Type | Note |
|---|---|---|
| role_id | UUID FK → roles | |
| permission_id | UUID FK → permissions | |
| PK | (role_id, permission_id) | |

### `users`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| role_id | UUID FK → roles | |
| full_name | VARCHAR(100) | |
| email | VARCHAR(150) | UNIQUE |
| phone | VARCHAR(20) | |
| password_hash | VARCHAR(255) | bcrypt |
| is_active | BOOLEAN | DEFAULT true |
| last_login_at | TIMESTAMPTZ | |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

---

## 2. Lead Management

### `leads`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | UNIQUE. Auto: LD-YYYYMMDD-XXXX |
| full_name | VARCHAR(100) | |
| dob | DATE | |
| gender | VARCHAR(10) | male/female/other |
| cccd | VARCHAR(20) | |
| phone | VARCHAR(20) | |
| email | VARCHAR(150) | |
| address | TEXT | |
| province | VARCHAR(100) | |
| occupation | VARCHAR(100) | |
| education_level | VARCHAR(50) | |
| work_experience | TEXT | |
| languages | VARCHAR(200) | |
| target_country | VARCHAR(100) | |
| interested_job_order_id | UUID FK → job_orders | NULL |
| source | VARCHAR(50) | facebook_ads, tiktok_ads, google_ads, website, zalo, hotline, agent, referral, event |
| agent_id | UUID FK → agents | NULL — nếu từ đại lý |
| assigned_to | UUID FK → users | Nhân viên phụ trách |
| status | VARCHAR(50) | new, not_contacted, contacted, interested, appointment, consulting, registered, converted, unsuitable, cancelled |
| notes | TEXT | |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

**Index:** phone, cccd, assigned_to, status, source, created_at

### `lead_activities`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| lead_id | UUID FK → leads | |
| activity_type | VARCHAR(50) | call, note, email, sms, zalo, appointment, status_change |
| content | TEXT | |
| old_status | VARCHAR(50) | NULL — chỉ dùng khi type = status_change |
| new_status | VARCHAR(50) | NULL |
| created_by | UUID FK → users | |
| created_at | TIMESTAMPTZ | |

---

## 3. Job Orders (Đơn Hàng)

### `job_orders`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | UNIQUE. Auto: JO-YYYYMM-XXX |
| country | VARCHAR(100) | |
| union_name | VARCHAR(200) | Tên nghiệp đoàn |
| company_name | VARCHAR(200) | Công ty tiếp nhận |
| field | VARCHAR(100) | Ngành nghề |
| quantity | INT | Số lượng tuyển |
| salary_description | TEXT | Mức lương mô tả |
| cost_amount | NUMERIC(15,2) | Chi phí tham gia |
| requirements | TEXT | Điều kiện tuyển dụng |
| recruitment_start_date | DATE | |
| expected_departure_date | DATE | |
| status | VARCHAR(50) | recruiting, full_profiles, interviewing, closed, cancelled |
| created_by | UUID FK → users | |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

---

## 4. Candidates (Ứng Viên)

### `candidates`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | UNIQUE. Auto: UV-YYYYMMDD-XXXX |
| lead_id | UUID FK → leads | NULL — nếu không qua lead |
| full_name | VARCHAR(100) | |
| cccd_number | VARCHAR(20) | |
| cccd_issue_date | DATE | |
| cccd_issue_place | VARCHAR(200) | |
| passport_number | VARCHAR(20) | |
| passport_expiry | DATE | |
| dob | DATE | |
| gender | VARCHAR(10) | |
| address | TEXT | |
| province | VARCHAR(100) | |
| marital_status | VARCHAR(30) | single, married, divorced, widowed |
| phone | VARCHAR(20) | |
| emergency_contact_name | VARCHAR(100) | |
| emergency_contact_phone | VARCHAR(20) | |
| emergency_contact_relation | VARCHAR(50) | |
| bank_account_number | VARCHAR(50) | |
| bank_name | VARCHAR(100) | |
| bank_account_name | VARCHAR(100) | |
| agent_id | UUID FK → agents | NULL |
| created_by | UUID FK → users | |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

### `candidate_documents`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| candidate_id | UUID FK → candidates | |
| doc_type | VARCHAR(50) | cccd, passport, household_book, birth_cert, degree, certificate, health_check, photo, criminal_record, contract, other |
| current_version_id | UUID FK → document_versions | |
| created_at | TIMESTAMPTZ | |

### `document_versions`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| document_id | UUID FK → candidate_documents | |
| version_number | INT | AUTO INCREMENT per document |
| file_url | TEXT | S3/MinIO path |
| file_name | VARCHAR(255) | |
| file_size | BIGINT | bytes |
| mime_type | VARCHAR(100) | |
| uploaded_by | UUID FK → users | |
| notes | TEXT | |
| created_at | TIMESTAMPTZ | |

---

## 5. Candidate Workflow

### `candidate_job_orders`
Bảng nối giữa ứng viên và đơn hàng (1 ứng viên có thể thử nhiều đơn)

| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| candidate_id | UUID FK → candidates | |
| job_order_id | UUID FK → job_orders | |
| current_step | VARCHAR(50) | Bước hiện tại trong workflow |
| status | VARCHAR(50) | active, dropped, completed |
| assigned_to | UUID FK → users | Nhân viên phụ trách |
| notes | TEXT | |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

### `workflow_steps`
Lưu toàn bộ lịch sử tiến độ từng bước

| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| cjo_id | UUID FK → candidate_job_orders | |
| step_name | VARCHAR(100) | Tên bước (xem enum bên dưới) |
| step_order | SMALLINT | Thứ tự bước |
| status | VARCHAR(30) | pending, in_progress, completed, skipped, failed |
| assigned_to | UUID FK → users | |
| started_at | TIMESTAMPTZ | |
| completed_at | TIMESTAMPTZ | |
| notes | TEXT | |
| created_by | UUID FK → users | |
| created_at | TIMESTAMPTZ | |

**Enum bước workflow (theo thứ tự):**
```
01. lead            — Lead mới
02. consulting      — Tư vấn
03. registration    — Đăng ký
04. deposit         — Đặt cọc
05. document        — Hoàn thiện hồ sơ
06. health_check    — Khám sức khỏe
07. orientation     — Học định hướng
08. entrance_exam   — Thi tuyển
09. selected        — Trúng tuyển
10. sign_contract   — Ký hợp đồng
11. visa_submit     — Nộp hồ sơ Visa
12. visa_approved   — Đậu Visa
13. full_payment    — Thanh toán đủ
14. book_flight     — Đặt vé máy bay
15. departure       — Xuất cảnh
16. arrived         — Đến nơi làm việc
17. completed       — Hoàn tất hồ sơ
```

---

## 6. Tài Chính

### `payments`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | Auto: PT-YYYYMMDD-XXXX |
| candidate_id | UUID FK → candidates | |
| job_order_id | UUID FK → job_orders | NULL |
| payment_type | VARCHAR(50) | deposit, document_fee, training_fee, visa_fee, service_fee, other_income |
| amount | NUMERIC(15,2) | |
| due_date | DATE | |
| paid_date | DATE | NULL nếu chưa trả |
| status | VARCHAR(30) | pending, partial, paid, overdue, refunded |
| payment_method | VARCHAR(50) | cash, bank_transfer, other |
| receipt_id | UUID FK → receipts | NULL |
| notes | TEXT | |
| created_by | UUID FK → users | |
| approved_by | UUID FK → users | NULL |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

### `receipts`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | UNIQUE. Auto: RC-YYYYMMDD-XXXX |
| receipt_type | VARCHAR(20) | income, expense |
| candidate_id | UUID FK → candidates | NULL |
| agent_id | UUID FK → agents | NULL |
| amount | NUMERIC(15,2) | |
| description | TEXT | |
| receipt_date | DATE | |
| pdf_url | TEXT | |
| signed_by | UUID FK → users | |
| created_by | UUID FK → users | |
| created_at | TIMESTAMPTZ | |

### `expenses`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | Auto: EX-YYYYMMDD-XXXX |
| category | VARCHAR(50) | marketing, partner, document, visa, training, refund, other |
| amount | NUMERIC(15,2) | |
| description | TEXT | |
| expense_date | DATE | |
| receipt_id | UUID FK → receipts | NULL |
| created_by | UUID FK → users | |
| approved_by | UUID FK → users | NULL |
| created_at | TIMESTAMPTZ | |

---

## 7. Đại Lý & Hoa Hồng

### `agents`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| code | VARCHAR(20) | UNIQUE. Auto: AG-XXXXXX |
| name | VARCHAR(200) | |
| representative_name | VARCHAR(100) | Người đại diện |
| phone | VARCHAR(20) | |
| email | VARCHAR(150) | |
| address | TEXT | |
| bank_account_number | VARCHAR(50) | |
| bank_name | VARCHAR(100) | |
| bank_account_name | VARCHAR(100) | |
| contract_url | TEXT | |
| is_active | BOOLEAN | DEFAULT true |
| user_id | UUID FK → users | NULL — tài khoản portal |
| created_at | TIMESTAMPTZ | |

### `agent_commission_configs`
Cấu hình % hoa hồng theo mốc

| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| agent_id | UUID FK → agents | |
| job_order_id | UUID FK → job_orders | NULL = áp dụng tất cả |
| country | VARCHAR(100) | NULL = áp dụng tất cả |
| milestone | VARCHAR(50) | deposit, selected, departure |
| percentage | NUMERIC(5,2) | VD: 20.00 |
| fixed_amount | NUMERIC(15,2) | NULL — nếu dùng % |
| created_at | TIMESTAMPTZ | |

### `agent_commissions`
Hoa hồng thực tế phát sinh theo từng mốc

| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| agent_id | UUID FK → agents | |
| candidate_id | UUID FK → candidates | |
| job_order_id | UUID FK → job_orders | |
| config_id | UUID FK → agent_commission_configs | |
| milestone | VARCHAR(50) | |
| base_amount | NUMERIC(15,2) | Số tiền căn cứ tính hoa hồng |
| commission_amount | NUMERIC(15,2) | Hoa hồng thực tế |
| status | VARCHAR(30) | pending, approved, paid, cancelled |
| paid_date | DATE | NULL |
| receipt_id | UUID FK → receipts | NULL |
| approved_by | UUID FK → users | NULL |
| created_at | TIMESTAMPTZ | |

---

## 8. Visa & Xuất Cảnh

### `visas`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| candidate_id | UUID FK → candidates | |
| job_order_id | UUID FK → job_orders | |
| visa_type | VARCHAR(50) | |
| country | VARCHAR(100) | |
| submitted_date | DATE | NULL |
| interview_date | DATE | NULL |
| result_date | DATE | NULL |
| status | VARCHAR(50) | not_submitted, preparing, submitted, additional_required, approved, rejected |
| rejection_reason | TEXT | NULL |
| notes | TEXT | |
| handled_by | UUID FK → users | |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

### `flights`
| Column | Type | Type |
|---|---|---|
| id | UUID PK | |
| candidate_id | UUID FK → candidates | |
| job_order_id | UUID FK → job_orders | |
| airline | VARCHAR(100) | |
| ticket_code | VARCHAR(50) | |
| departure_date | DATE | |
| departure_time | TIME | |
| departure_airport | VARCHAR(100) | |
| destination_country | VARCHAR(100) | |
| destination_airport | VARCHAR(100) | |
| actual_departure_at | TIMESTAMPTZ | NULL — xác nhận xuất cảnh thực tế |
| assigned_to | UUID FK → users | |
| notes | TEXT | |
| created_at | TIMESTAMPTZ | |

---

## 9. Thông Báo & Audit

### `notifications`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| user_id | UUID FK → users | Người nhận |
| type | VARCHAR(50) | reminder_document, reminder_payment, reminder_interview, reminder_visa, reminder_departure, commission_payment |
| title | VARCHAR(255) | |
| body | TEXT | |
| channel | VARCHAR(20) | email, sms, zalo, in_app |
| is_read | BOOLEAN | DEFAULT false |
| reference_type | VARCHAR(50) | candidate, lead, agent |
| reference_id | UUID | |
| sent_at | TIMESTAMPTZ | NULL — chưa gửi |
| read_at | TIMESTAMPTZ | NULL |
| created_at | TIMESTAMPTZ | |

### `audit_logs`
| Column | Type | Note |
|---|---|---|
| id | UUID PK | |
| user_id | UUID FK → users | |
| action | VARCHAR(50) | create, update, delete, approve, login, logout |
| resource | VARCHAR(50) | VD: leads, candidates, payments |
| resource_id | UUID | |
| old_value | JSONB | NULL |
| new_value | JSONB | NULL |
| ip_address | INET | |
| user_agent | TEXT | |
| created_at | TIMESTAMPTZ | |

**Index:** (resource, resource_id), (user_id, created_at)

---

## 10. Index Strategy

```sql
-- Leads
CREATE INDEX idx_leads_phone ON leads(phone);
CREATE INDEX idx_leads_cccd ON leads(cccd);
CREATE INDEX idx_leads_status ON leads(status);
CREATE INDEX idx_leads_assigned_to ON leads(assigned_to);
CREATE INDEX idx_leads_source ON leads(source);
CREATE INDEX idx_leads_created_at ON leads(created_at DESC);

-- Candidates
CREATE INDEX idx_candidates_phone ON candidates(phone);
CREATE INDEX idx_candidates_cccd ON candidates(cccd_number);
CREATE INDEX idx_candidates_passport ON candidates(passport_number);

-- Workflow
CREATE INDEX idx_cjo_candidate ON candidate_job_orders(candidate_id);
CREATE INDEX idx_cjo_job_order ON candidate_job_orders(job_order_id);
CREATE INDEX idx_workflow_steps_cjo ON workflow_steps(cjo_id);

-- Finance
CREATE INDEX idx_payments_candidate ON payments(candidate_id);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_commissions_agent ON agent_commissions(agent_id);
CREATE INDEX idx_commissions_status ON agent_commissions(status);

-- Audit
CREATE INDEX idx_audit_resource ON audit_logs(resource, resource_id);
CREATE INDEX idx_audit_user_time ON audit_logs(user_id, created_at DESC);
```

---

## 11. Seed Data (Roles & Permissions)

```sql
INSERT INTO roles (id, name, description) VALUES
  (gen_random_uuid(), 'super_admin', 'Quyền cao nhất'),
  (gen_random_uuid(), 'director', 'Giám đốc'),
  (gen_random_uuid(), 'recruitment_manager', 'Trưởng phòng tuyển dụng'),
  (gen_random_uuid(), 'recruiter', 'Nhân viên tuyển dụng'),
  (gen_random_uuid(), 'document_staff', 'Bộ phận hồ sơ'),
  (gen_random_uuid(), 'visa_staff', 'Bộ phận visa'),
  (gen_random_uuid(), 'accountant', 'Kế toán'),
  (gen_random_uuid(), 'agent', 'Đại lý / CTV');
```
