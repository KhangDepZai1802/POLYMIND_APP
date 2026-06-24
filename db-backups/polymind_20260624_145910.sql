--
-- PostgreSQL database dump
--

\restrict bMYYKo0WspwcUr4paWlkPYRkOLGMH2khNnqNOXtIle94Ih83bPKHlgCQeRGi2Ij

-- Dumped from database version 16.14
-- Dumped by pg_dump version 16.14

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.workflow_step_records DROP CONSTRAINT IF EXISTS fk_workflow_step_records_candidate_job_orders_candidate_job_or;
ALTER TABLE IF EXISTS ONLY public.role_permissions DROP CONSTRAINT IF EXISTS fk_role_permissions_permissions_permission_id;
ALTER TABLE IF EXISTS ONLY public.lead_activities DROP CONSTRAINT IF EXISTS fk_lead_activities_leads_lead_id;
ALTER TABLE IF EXISTS ONLY public.document_versions DROP CONSTRAINT IF EXISTS fk_document_versions_candidate_documents_document_id;
ALTER TABLE IF EXISTS ONLY public.candidate_job_orders DROP CONSTRAINT IF EXISTS fk_candidate_job_orders_job_orders_job_order_id;
ALTER TABLE IF EXISTS ONLY public.candidate_job_orders DROP CONSTRAINT IF EXISTS fk_candidate_job_orders_candidates_candidate_id;
ALTER TABLE IF EXISTS ONLY public.candidate_documents DROP CONSTRAINT IF EXISTS fk_candidate_documents_candidates_candidate_id;
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS fk_asp_net_user_tokens_asp_net_users_user_id;
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS fk_asp_net_user_roles_asp_net_users_user_id;
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS fk_asp_net_user_roles_asp_net_roles_role_id;
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS fk_asp_net_user_logins_asp_net_users_user_id;
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS fk_asp_net_user_claims_asp_net_users_user_id;
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS fk_asp_net_role_claims_asp_net_roles_role_id;
ALTER TABLE IF EXISTS ONLY public.agent_commissions DROP CONSTRAINT IF EXISTS fk_agent_commissions_agents_agent_id;
ALTER TABLE IF EXISTS ONLY public.agent_commission_configs DROP CONSTRAINT IF EXISTS fk_agent_commission_configs_agents_agent_id;
DROP INDEX IF EXISTS public.ix_workflow_step_records_candidate_job_order_id;
DROP INDEX IF EXISTS public.ix_role_permissions_permission_id;
DROP INDEX IF EXISTS public.ix_receipts_code;
DROP INDEX IF EXISTS public.ix_permissions_name;
DROP INDEX IF EXISTS public.ix_payments_status;
DROP INDEX IF EXISTS public.ix_payments_code;
DROP INDEX IF EXISTS public.ix_payments_candidate_id;
DROP INDEX IF EXISTS public.ix_leads_status;
DROP INDEX IF EXISTS public.ix_leads_source;
DROP INDEX IF EXISTS public.ix_leads_phone;
DROP INDEX IF EXISTS public.ix_leads_created_at;
DROP INDEX IF EXISTS public.ix_leads_code;
DROP INDEX IF EXISTS public.ix_leads_cccd;
DROP INDEX IF EXISTS public.ix_leads_assigned_to;
DROP INDEX IF EXISTS public.ix_lead_activities_lead_id;
DROP INDEX IF EXISTS public.ix_job_orders_code;
DROP INDEX IF EXISTS public.ix_expenses_code;
DROP INDEX IF EXISTS public.ix_document_versions_document_id;
DROP INDEX IF EXISTS public.ix_candidates_phone;
DROP INDEX IF EXISTS public.ix_candidates_passport_number;
DROP INDEX IF EXISTS public.ix_candidates_code;
DROP INDEX IF EXISTS public.ix_candidates_cccd_number;
DROP INDEX IF EXISTS public.ix_candidate_job_orders_job_order_id;
DROP INDEX IF EXISTS public.ix_candidate_job_orders_candidate_id;
DROP INDEX IF EXISTS public.ix_candidate_documents_candidate_id;
DROP INDEX IF EXISTS public.ix_audit_logs_user_id_created_at;
DROP INDEX IF EXISTS public.ix_audit_logs_resource_resource_id;
DROP INDEX IF EXISTS public.ix_asp_net_user_roles_role_id;
DROP INDEX IF EXISTS public.ix_asp_net_user_logins_user_id;
DROP INDEX IF EXISTS public.ix_asp_net_user_claims_user_id;
DROP INDEX IF EXISTS public.ix_asp_net_role_claims_role_id;
DROP INDEX IF EXISTS public.ix_agents_code;
DROP INDEX IF EXISTS public.ix_agent_commissions_status;
DROP INDEX IF EXISTS public.ix_agent_commissions_agent_id;
DROP INDEX IF EXISTS public.ix_agent_commission_configs_agent_id;
DROP INDEX IF EXISTS public."UserNameIndex";
DROP INDEX IF EXISTS public."RoleNameIndex";
DROP INDEX IF EXISTS public."EmailIndex";
ALTER TABLE IF EXISTS ONLY public.workflow_step_records DROP CONSTRAINT IF EXISTS pk_workflow_step_records;
ALTER TABLE IF EXISTS ONLY public.visas DROP CONSTRAINT IF EXISTS pk_visas;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS pk_users;
ALTER TABLE IF EXISTS ONLY public.roles DROP CONSTRAINT IF EXISTS pk_roles;
ALTER TABLE IF EXISTS ONLY public.role_permissions DROP CONSTRAINT IF EXISTS pk_role_permissions;
ALTER TABLE IF EXISTS ONLY public.receipts DROP CONSTRAINT IF EXISTS pk_receipts;
ALTER TABLE IF EXISTS ONLY public.permissions DROP CONSTRAINT IF EXISTS pk_permissions;
ALTER TABLE IF EXISTS ONLY public.payments DROP CONSTRAINT IF EXISTS pk_payments;
ALTER TABLE IF EXISTS ONLY public.notifications DROP CONSTRAINT IF EXISTS pk_notifications;
ALTER TABLE IF EXISTS ONLY public.leads DROP CONSTRAINT IF EXISTS pk_leads;
ALTER TABLE IF EXISTS ONLY public.lead_activities DROP CONSTRAINT IF EXISTS pk_lead_activities;
ALTER TABLE IF EXISTS ONLY public.job_orders DROP CONSTRAINT IF EXISTS pk_job_orders;
ALTER TABLE IF EXISTS ONLY public.flights DROP CONSTRAINT IF EXISTS pk_flights;
ALTER TABLE IF EXISTS ONLY public.expenses DROP CONSTRAINT IF EXISTS pk_expenses;
ALTER TABLE IF EXISTS ONLY public.document_versions DROP CONSTRAINT IF EXISTS pk_document_versions;
ALTER TABLE IF EXISTS ONLY public.candidates DROP CONSTRAINT IF EXISTS pk_candidates;
ALTER TABLE IF EXISTS ONLY public.candidate_job_orders DROP CONSTRAINT IF EXISTS pk_candidate_job_orders;
ALTER TABLE IF EXISTS ONLY public.candidate_documents DROP CONSTRAINT IF EXISTS pk_candidate_documents;
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS pk_audit_logs;
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS pk_asp_net_user_tokens;
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS pk_asp_net_user_roles;
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS pk_asp_net_user_logins;
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS pk_asp_net_user_claims;
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS pk_asp_net_role_claims;
ALTER TABLE IF EXISTS ONLY public.agents DROP CONSTRAINT IF EXISTS pk_agents;
ALTER TABLE IF EXISTS ONLY public.agent_commissions DROP CONSTRAINT IF EXISTS pk_agent_commissions;
ALTER TABLE IF EXISTS ONLY public.agent_commission_configs DROP CONSTRAINT IF EXISTS pk_agent_commission_configs;
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS pk___ef_migrations_history;
DROP TABLE IF EXISTS public.workflow_step_records;
DROP TABLE IF EXISTS public.visas;
DROP TABLE IF EXISTS public.users;
DROP TABLE IF EXISTS public.roles;
DROP TABLE IF EXISTS public.role_permissions;
DROP TABLE IF EXISTS public.receipts;
DROP TABLE IF EXISTS public.permissions;
DROP TABLE IF EXISTS public.payments;
DROP TABLE IF EXISTS public.notifications;
DROP TABLE IF EXISTS public.leads;
DROP TABLE IF EXISTS public.lead_activities;
DROP TABLE IF EXISTS public.job_orders;
DROP TABLE IF EXISTS public.flights;
DROP TABLE IF EXISTS public.expenses;
DROP TABLE IF EXISTS public.document_versions;
DROP TABLE IF EXISTS public.candidates;
DROP TABLE IF EXISTS public.candidate_job_orders;
DROP TABLE IF EXISTS public.candidate_documents;
DROP TABLE IF EXISTS public.audit_logs;
DROP TABLE IF EXISTS public.agents;
DROP TABLE IF EXISTS public.agent_commissions;
DROP TABLE IF EXISTS public.agent_commission_configs;
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."AspNetUserTokens";
DROP TABLE IF EXISTS public."AspNetUserRoles";
DROP TABLE IF EXISTS public."AspNetUserLogins";
DROP TABLE IF EXISTS public."AspNetUserClaims";
DROP TABLE IF EXISTS public."AspNetRoleClaims";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public."AspNetRoleClaims" (
    id integer NOT NULL,
    role_id uuid NOT NULL,
    claim_type text,
    claim_value text
);


ALTER TABLE public."AspNetRoleClaims" OWNER TO polymind;

--
-- Name: AspNetRoleClaims_id_seq; Type: SEQUENCE; Schema: public; Owner: polymind
--

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public."AspNetUserClaims" (
    id integer NOT NULL,
    user_id uuid NOT NULL,
    claim_type text,
    claim_value text
);


ALTER TABLE public."AspNetUserClaims" OWNER TO polymind;

--
-- Name: AspNetUserClaims_id_seq; Type: SEQUENCE; Schema: public; Owner: polymind
--

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public."AspNetUserLogins" (
    login_provider text NOT NULL,
    provider_key text NOT NULL,
    provider_display_name text,
    user_id uuid NOT NULL
);


ALTER TABLE public."AspNetUserLogins" OWNER TO polymind;

--
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public."AspNetUserRoles" (
    user_id uuid NOT NULL,
    role_id uuid NOT NULL
);


ALTER TABLE public."AspNetUserRoles" OWNER TO polymind;

--
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public."AspNetUserTokens" (
    user_id uuid NOT NULL,
    login_provider text NOT NULL,
    name text NOT NULL,
    value text
);


ALTER TABLE public."AspNetUserTokens" OWNER TO polymind;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO polymind;

--
-- Name: agent_commission_configs; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.agent_commission_configs (
    id uuid NOT NULL,
    agent_id uuid NOT NULL,
    job_order_id uuid,
    country text,
    milestone text NOT NULL,
    percentage numeric(5,2),
    fixed_amount numeric(15,2),
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.agent_commission_configs OWNER TO polymind;

--
-- Name: agent_commissions; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.agent_commissions (
    id uuid NOT NULL,
    agent_id uuid NOT NULL,
    candidate_id uuid NOT NULL,
    job_order_id uuid NOT NULL,
    config_id uuid,
    milestone text NOT NULL,
    base_amount numeric(15,2) NOT NULL,
    commission_amount numeric(15,2) NOT NULL,
    status text NOT NULL,
    paid_date date,
    receipt_id uuid,
    approved_by uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.agent_commissions OWNER TO polymind;

--
-- Name: agents; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.agents (
    id uuid NOT NULL,
    code text NOT NULL,
    name text NOT NULL,
    representative_name text,
    phone text,
    email text,
    address text,
    bank_account_number text,
    bank_name text,
    bank_account_name text,
    contract_url text,
    is_active boolean NOT NULL,
    user_id uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.agents OWNER TO polymind;

--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.audit_logs (
    id uuid NOT NULL,
    user_id uuid,
    action text NOT NULL,
    resource text NOT NULL,
    resource_id uuid,
    old_value jsonb,
    new_value jsonb,
    ip_address text,
    user_agent text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.audit_logs OWNER TO polymind;

--
-- Name: candidate_documents; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.candidate_documents (
    id uuid NOT NULL,
    candidate_id uuid NOT NULL,
    doc_type text NOT NULL,
    current_version_id uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.candidate_documents OWNER TO polymind;

--
-- Name: candidate_job_orders; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.candidate_job_orders (
    id uuid NOT NULL,
    candidate_id uuid NOT NULL,
    job_order_id uuid NOT NULL,
    current_step text NOT NULL,
    status text NOT NULL,
    assigned_to uuid,
    notes text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.candidate_job_orders OWNER TO polymind;

--
-- Name: candidates; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.candidates (
    id uuid NOT NULL,
    code text NOT NULL,
    lead_id uuid,
    full_name text NOT NULL,
    cccd_number text,
    cccd_issue_date date,
    cccd_issue_place text,
    passport_number text,
    passport_expiry date,
    dob date,
    gender text,
    address text,
    province text,
    marital_status text,
    phone text,
    emergency_contact_name text,
    emergency_contact_phone text,
    emergency_contact_relation text,
    bank_account_number text,
    bank_name text,
    bank_account_name text,
    agent_id uuid,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.candidates OWNER TO polymind;

--
-- Name: document_versions; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.document_versions (
    id uuid NOT NULL,
    document_id uuid NOT NULL,
    version_number integer NOT NULL,
    file_url text NOT NULL,
    file_name text NOT NULL,
    file_size bigint NOT NULL,
    mime_type text,
    uploaded_by uuid NOT NULL,
    notes text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.document_versions OWNER TO polymind;

--
-- Name: expenses; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.expenses (
    id uuid NOT NULL,
    code text NOT NULL,
    category text NOT NULL,
    amount numeric(15,2) NOT NULL,
    description text,
    expense_date date NOT NULL,
    receipt_id uuid,
    created_by uuid NOT NULL,
    approved_by uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.expenses OWNER TO polymind;

--
-- Name: flights; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.flights (
    id uuid NOT NULL,
    candidate_id uuid NOT NULL,
    job_order_id uuid NOT NULL,
    airline text,
    ticket_code text,
    departure_date date,
    departure_time time without time zone,
    departure_airport text,
    destination_country text,
    destination_airport text,
    actual_departure_at timestamp with time zone,
    assigned_to uuid,
    notes text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.flights OWNER TO polymind;

--
-- Name: job_orders; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.job_orders (
    id uuid NOT NULL,
    code text NOT NULL,
    country text NOT NULL,
    union_name text,
    company_name text,
    field text,
    quantity integer NOT NULL,
    salary_description text,
    cost_amount numeric(15,2),
    requirements text,
    recruitment_start_date date,
    expected_departure_date date,
    status text NOT NULL,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.job_orders OWNER TO polymind;

--
-- Name: lead_activities; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.lead_activities (
    id uuid NOT NULL,
    lead_id uuid NOT NULL,
    activity_type text NOT NULL,
    content text,
    old_status text,
    new_status text,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.lead_activities OWNER TO polymind;

--
-- Name: leads; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.leads (
    id uuid NOT NULL,
    code text NOT NULL,
    full_name text NOT NULL,
    dob date,
    gender text,
    cccd text,
    phone text,
    email text,
    address text,
    province text,
    occupation text,
    education_level text,
    work_experience text,
    languages text,
    target_country text,
    interested_job_order_id uuid,
    source text NOT NULL,
    agent_id uuid,
    assigned_to uuid,
    status text NOT NULL,
    notes text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.leads OWNER TO polymind;

--
-- Name: notifications; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.notifications (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    type text NOT NULL,
    title text NOT NULL,
    body text,
    channel text NOT NULL,
    is_read boolean NOT NULL,
    reference_type text,
    reference_id uuid,
    sent_at timestamp with time zone,
    read_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.notifications OWNER TO polymind;

--
-- Name: payments; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.payments (
    id uuid NOT NULL,
    code text NOT NULL,
    candidate_id uuid NOT NULL,
    job_order_id uuid,
    payment_type text NOT NULL,
    amount numeric(15,2) NOT NULL,
    due_date date,
    paid_date date,
    status text NOT NULL,
    payment_method text,
    receipt_id uuid,
    notes text,
    created_by uuid NOT NULL,
    approved_by uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.payments OWNER TO polymind;

--
-- Name: permissions; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.permissions (
    id uuid NOT NULL,
    name text NOT NULL,
    resource text NOT NULL,
    action text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.permissions OWNER TO polymind;

--
-- Name: receipts; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.receipts (
    id uuid NOT NULL,
    code text NOT NULL,
    receipt_type text NOT NULL,
    candidate_id uuid,
    agent_id uuid,
    amount numeric(15,2) NOT NULL,
    description text,
    receipt_date date NOT NULL,
    pdf_url text,
    signed_by uuid,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.receipts OWNER TO polymind;

--
-- Name: role_permissions; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.role_permissions (
    role_id uuid NOT NULL,
    permission_id uuid NOT NULL
);


ALTER TABLE public.role_permissions OWNER TO polymind;

--
-- Name: roles; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.roles (
    id uuid NOT NULL,
    description text,
    name character varying(256),
    normalized_name character varying(256),
    concurrency_stamp text
);


ALTER TABLE public.roles OWNER TO polymind;

--
-- Name: users; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.users (
    id uuid NOT NULL,
    full_name text NOT NULL,
    is_active boolean NOT NULL,
    last_login_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    user_name character varying(256),
    normalized_user_name character varying(256),
    email character varying(256),
    normalized_email character varying(256),
    email_confirmed boolean NOT NULL,
    password_hash text,
    security_stamp text,
    concurrency_stamp text,
    phone_number text,
    phone_number_confirmed boolean NOT NULL,
    two_factor_enabled boolean NOT NULL,
    lockout_end timestamp with time zone,
    lockout_enabled boolean NOT NULL,
    access_failed_count integer NOT NULL
);


ALTER TABLE public.users OWNER TO polymind;

--
-- Name: visas; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.visas (
    id uuid NOT NULL,
    candidate_id uuid NOT NULL,
    job_order_id uuid NOT NULL,
    visa_type text,
    country text NOT NULL,
    submitted_date date,
    interview_date date,
    result_date date,
    status text NOT NULL,
    rejection_reason text,
    notes text,
    handled_by uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.visas OWNER TO polymind;

--
-- Name: workflow_step_records; Type: TABLE; Schema: public; Owner: polymind
--

CREATE TABLE public.workflow_step_records (
    id uuid NOT NULL,
    candidate_job_order_id uuid NOT NULL,
    step text NOT NULL,
    status text NOT NULL,
    assigned_to uuid,
    started_at timestamp with time zone,
    completed_at timestamp with time zone,
    notes text,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


ALTER TABLE public.workflow_step_records OWNER TO polymind;

--
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public."AspNetRoleClaims" (id, role_id, claim_type, claim_value) FROM stdin;
\.


--
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public."AspNetUserClaims" (id, user_id, claim_type, claim_value) FROM stdin;
\.


--
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public."AspNetUserLogins" (login_provider, provider_key, provider_display_name, user_id) FROM stdin;
\.


--
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public."AspNetUserRoles" (user_id, role_id) FROM stdin;
019ef7b8-c87b-705f-aecb-05cfc3c92827	019ef7b8-c6c8-71df-bec4-242eb846f44a
019ef84b-078a-7465-b909-89a6a20c9113	019ef7b8-c762-782b-95ea-5666f661464b
019ef84b-0801-7735-af48-49dc9483fa27	019ef7b8-c769-7d13-bc34-4b39d2e0acb9
019ef84b-0849-773f-a462-195a31e9bb9b	019ef7b8-c76f-7e7d-ac60-2919cff7c62c
019ef84b-088f-724c-9a87-09edcc5c5cb1	019ef7b8-c774-7a82-946b-f1ef65b242a9
019ef84b-08dd-7102-9c9a-b824901ae4e3	019ef7b8-c778-7182-b4f2-49e3170b89b0
019ef84b-0928-7cf4-bf25-9c6a8ce31402	019ef7b8-c77e-736f-ad6e-1930f7e1bf10
019ef84b-0973-773d-97ac-a1ea7ca8328b	019ef7b8-c783-7fd4-aae7-a47cea63b5f3
\.


--
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public."AspNetUserTokens" (user_id, login_provider, name, value) FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public."__EFMigrationsHistory" (migration_id, product_version) FROM stdin;
20260624034033_InitialCreate	10.0.9
\.


--
-- Data for Name: agent_commission_configs; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.agent_commission_configs (id, agent_id, job_order_id, country, milestone, percentage, fixed_amount, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: agent_commissions; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.agent_commissions (id, agent_id, candidate_id, job_order_id, config_id, milestone, base_amount, commission_amount, status, paid_date, receipt_id, approved_by, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: agents; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.agents (id, code, name, representative_name, phone, email, address, bank_account_number, bank_name, bank_account_name, contract_url, is_active, user_id, created_at, updated_at) FROM stdin;
2b3d004a-8492-430a-8e64-df20ff1aed44	AG-000001	Đại lý Miền Bắc	Nguyễn Văn An	0988104784	ag-000001@agent.local	\N	\N	\N	\N	\N	t	\N	2026-06-24 04:00:54.615428+00	2026-06-24 04:00:54.615429+00
415a3c4c-9fe9-4e58-b4e2-78f11adf1553	AG-000002	CTV Nghệ An	Trần Thị Bình	0942720621	ag-000002@agent.local	\N	\N	\N	\N	\N	t	\N	2026-06-24 04:00:54.615593+00	2026-06-24 04:00:54.615593+00
829c2745-8331-4965-a2d3-8695d0ad829d	AG-000003	Đại lý Hải Phòng	Lê Hoàng Cường	0947289188	ag-000003@agent.local	\N	\N	\N	\N	\N	t	\N	2026-06-24 04:00:54.615595+00	2026-06-24 04:00:54.615595+00
\.


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.audit_logs (id, user_id, action, resource, resource_id, old_value, new_value, ip_address, user_agent, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: candidate_documents; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.candidate_documents (id, candidate_id, doc_type, current_version_id, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: candidate_job_orders; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.candidate_job_orders (id, candidate_id, job_order_id, current_step, status, assigned_to, notes, created_at, updated_at) FROM stdin;
18114c8e-7e29-4df4-a188-f10c7a0f2075	e2afc2af-ec08-4bf4-a808-a0d991c43dbb	31406569-076f-4e8b-8d10-ab07247b1b18	Arrived	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-03 00:00:54.724766+00	2026-06-03 00:00:54.724766+00
210aad29-1ee0-428e-9fac-2c02bbe4008c	ae6e5fce-26b0-4a4d-bd61-a79db574ff44	d7a64bb2-923e-41ad-a0a6-ce7e1bfcb9fd	EntranceExam	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-05-23 01:00:54.725336+00	2026-05-23 01:00:54.725336+00
4aeb662c-30f8-4997-a005-0fcb7f25ada7	38e9601b-13bc-4dc8-ac46-319cc113af00	31406569-076f-4e8b-8d10-ab07247b1b18	Document	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-14 10:00:54.725319+00	2026-06-14 10:00:54.725319+00
552937da-6395-429d-acc0-0f27f93f5825	45307427-0bc6-4243-a04c-d23440eb3661	d31c12ab-586a-49e1-be28-4c96f4d7cd03	Document	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-08 02:00:54.725343+00	2026-06-08 02:00:54.725343+00
5b0d3934-7391-406b-aefc-a5e295d7dcbc	0644df67-36ad-4595-ae77-39926cd2e8e6	dec6d6a8-6cb7-4afd-ab00-87faa77f2ba0	SignContract	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-05 19:00:54.72533+00	2026-06-05 19:00:54.72533+00
70c57d40-2774-4c21-8b93-470282c8e678	9e47bf90-0bdf-440c-a3f7-37814b6654d3	6b94f36a-8d95-420c-b84a-9ce633ee4408	Orientation	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-01 05:00:54.725339+00	2026-06-01 05:00:54.725339+00
77a0ef1f-9ceb-43ae-a009-c5ce19a851da	79579ce6-1b65-4d6e-937a-478dcf7faf84	6b94f36a-8d95-420c-b84a-9ce633ee4408	Selected	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-06 16:00:54.725326+00	2026-06-06 16:00:54.725326+00
89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	15b08e0d-179e-4762-acd9-84f0752ebe26	d31c12ab-586a-49e1-be28-4c96f4d7cd03	FullPayment	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-05-16 14:00:54.725333+00	2026-05-16 14:00:54.725333+00
8af81188-5b54-4a11-9e79-7f4beba87710	fcb085f0-c3f2-4eb4-8468-a103d53bdf9c	6b94f36a-8d95-420c-b84a-9ce633ee4408	Orientation	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-08 22:00:54.725279+00	2026-06-08 22:00:54.725279+00
e73dd440-22d0-406f-93c9-993c2682ebc0	f04745f2-66e4-4181-b1cb-d4ead5b90a5d	d31c12ab-586a-49e1-be28-4c96f4d7cd03	BookFlight	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-05-28 23:00:54.725323+00	2026-05-28 23:00:54.725323+00
f8c53e18-2f5a-456d-944c-e52e65275e60	e6dc06d8-ea69-425e-ae27-f6c7926246d2	31406569-076f-4e8b-8d10-ab07247b1b18	Deposit	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-02 03:00:54.725287+00	2026-06-02 03:00:54.725287+00
f9959a07-0d0e-4440-8d38-baec9cfcaa57	d070bbce-35bd-4a25-ba0b-5377374537e4	31406569-076f-4e8b-8d10-ab07247b1b18	BookFlight	Active	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-05-21 04:00:54.725291+00	2026-05-21 04:00:54.725291+00
\.


--
-- Data for Name: candidates; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.candidates (id, code, lead_id, full_name, cccd_number, cccd_issue_date, cccd_issue_place, passport_number, passport_expiry, dob, gender, address, province, marital_status, phone, emergency_contact_name, emergency_contact_phone, emergency_contact_relation, bank_account_number, bank_name, bank_account_name, agent_id, created_by, created_at, updated_at) FROM stdin;
0644df67-36ad-4595-ae77-39926cd2e8e6	UV-20260605-2007	15e3c29d-9f15-4d40-af0b-ffd36aa900c5	Nguyễn Tuấn Phong	022625451202	\N	\N	\N	\N	\N	Female	\N	Hà Nội	\N	0965494546	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-05 19:00:54.72533+00	2026-06-05 19:00:54.72533+00
15b08e0d-179e-4762-acd9-84f0752ebe26	UV-20260516-2008	7a19a478-4901-469f-a004-a25d3e3743b0	Vũ Thị Hà	056165840416	\N	\N	\N	\N	\N	Male	\N	Hà Nội	\N	0976159462	\N	\N	\N	\N	\N	\N	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-16 14:00:54.725333+00	2026-05-16 14:00:54.725333+00
38e9601b-13bc-4dc8-ac46-319cc113af00	UV-20260614-2004	56e040ee-33e0-4ab8-9802-fd1273876a0a	Bùi Thị Lan	090083949324	\N	\N	\N	\N	\N	Female	\N	Nam Định	\N	0936064600	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-14 10:00:54.725319+00	2026-06-14 10:00:54.725319+00
45307427-0bc6-4243-a04c-d23440eb3661	UV-20260608-2011	eae11b57-90b0-452a-b4e2-12aa55032eef	Nguyễn Minh Châu	035283628468	\N	\N	\N	\N	\N	Female	\N	Hà Nội	\N	0999413618	\N	\N	\N	\N	\N	\N	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 02:00:54.725343+00	2026-06-08 02:00:54.725343+00
79579ce6-1b65-4d6e-937a-478dcf7faf84	UV-20260606-2006	69cf9616-3bf2-45d4-a264-df37ad46e456	Hoàng Văn An	050840984709	\N	\N	\N	\N	\N	Female	\N	Hà Nội	\N	0954963711	\N	\N	\N	\N	\N	\N	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-06 16:00:54.725326+00	2026-06-06 16:00:54.725326+00
9e47bf90-0bdf-440c-a3f7-37814b6654d3	UV-20260601-2010	aaf83077-a0ed-44c4-956c-cc4ed8e36068	Đặng Đức Mạnh	083300801963	\N	\N	\N	\N	\N	Male	\N	Hà Nội	\N	0968574909	\N	\N	\N	\N	\N	\N	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-01 05:00:54.725339+00	2026-06-01 05:00:54.725339+00
ae6e5fce-26b0-4a4d-bd61-a79db574ff44	UV-20260523-2009	2b19856a-79b6-42e4-932e-0a872cddc7ee	Trần Thị Hà	023852931663	\N	\N	\N	\N	\N	Female	\N	Nghệ An	\N	0922122421	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-23 01:00:54.725336+00	2026-05-23 01:00:54.725336+00
d070bbce-35bd-4a25-ba0b-5377374537e4	UV-20260521-2003	99baa6c3-1eaa-4a24-9b85-623fc2101b60	Nguyễn Quốc Dũng	084576995508	\N	\N	\N	\N	\N	Male	\N	Hà Tĩnh	\N	0958214981	\N	\N	\N	\N	\N	\N	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-21 04:00:54.725291+00	2026-05-21 04:00:54.725291+00
e2afc2af-ec08-4bf4-a808-a0d991c43dbb	UV-20260603-2000	b3f4e745-dbb0-4fde-a359-2c0f70cd9c5b	Đặng Thị Bình	013949244354	\N	\N	\N	\N	\N	Male	\N	Nam Định	\N	0933633340	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-03 00:00:54.724766+00	2026-06-03 00:00:54.724766+00
e6dc06d8-ea69-425e-ae27-f6c7926246d2	UV-20260602-2002	a4de1cf1-bfe6-4d80-889f-b0bbe02c8bab	Nguyễn Đức Mạnh	046734907846	\N	\N	\N	\N	\N	Male	\N	Nghệ An	\N	0974003912	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-02 03:00:54.725287+00	2026-06-02 03:00:54.725287+00
f04745f2-66e4-4181-b1cb-d4ead5b90a5d	UV-20260528-2005	8f1ecda8-bdca-4f2c-a105-ea9073cd0002	Nguyễn Thị Hà	065925630651	\N	\N	\N	\N	\N	Male	\N	Thanh Hóa	\N	0911192839	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-28 23:00:54.725323+00	2026-05-28 23:00:54.725323+00
fcb085f0-c3f2-4eb4-8468-a103d53bdf9c	UV-20260608-2001	50903674-44db-414b-ad62-f9590f74091e	Lê Hữu Khang	089350534967	\N	\N	\N	\N	\N	Male	\N	Hà Nội	\N	0956570598	\N	\N	\N	\N	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 22:00:54.725279+00	2026-06-08 22:00:54.725279+00
\.


--
-- Data for Name: document_versions; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.document_versions (id, document_id, version_number, file_url, file_name, file_size, mime_type, uploaded_by, notes, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: expenses; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.expenses (id, code, category, amount, description, expense_date, receipt_id, created_by, approved_by, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: flights; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.flights (id, candidate_id, job_order_id, airline, ticket_code, departure_date, departure_time, departure_airport, destination_country, destination_airport, actual_departure_at, assigned_to, notes, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: job_orders; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.job_orders (id, code, country, union_name, company_name, field, quantity, salary_description, cost_amount, requirements, recruitment_start_date, expected_departure_date, status, created_by, created_at, updated_at) FROM stdin;
31406569-076f-4e8b-8d10-ab07247b1b18	JO-202606-930	Đức	ZAV Germany	Bosch GmbH	Điều dưỡng	10	30-40 triệu/tháng	120000000.00	\N	2026-06-24	2026-10-24	Recruiting	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.57465+00	2026-06-24 04:00:54.57465+00
6b94f36a-8d95-420c-b84a-9ce633ee4408	JO-202606-388	Nhật Bản	Nghiệp đoàn Osaka	Panasonic	Điện tử	20	30-40 triệu/tháng	120000000.00	\N	2026-06-24	2026-10-24	Recruiting	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.574605+00	2026-06-24 04:00:54.574605+00
d31c12ab-586a-49e1-be28-4c96f4d7cd03	JO-202606-144	Đài Loan	Formosa Group	Formosa Plastics	Nhựa - Hóa chất	50	30-40 triệu/tháng	120000000.00	\N	2026-06-24	2026-10-24	Recruiting	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.574648+00	2026-06-24 04:00:54.574648+00
d7a64bb2-923e-41ad-a0a6-ce7e1bfcb9fd	JO-202606-794	Hàn Quốc	EPS Korea	Hyundai Steel	Luyện kim	15	30-40 triệu/tháng	120000000.00	\N	2026-06-24	2026-10-24	Recruiting	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.57465+00	2026-06-24 04:00:54.57465+00
dec6d6a8-6cb7-4afd-ab00-87faa77f2ba0	JO-202606-475	Nhật Bản	Nghiệp đoàn Kanto	Toyota Kyushu	Cơ khí chế tạo	30	30-40 triệu/tháng	120000000.00	\N	2026-06-24	2026-10-24	Recruiting	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.569398+00	2026-06-24 04:00:54.569398+00
\.


--
-- Data for Name: lead_activities; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.lead_activities (id, lead_id, activity_type, content, old_status, new_status, created_by, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: leads; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.leads (id, code, full_name, dob, gender, cccd, phone, email, address, province, occupation, education_level, work_experience, languages, target_country, interested_job_order_id, source, agent_id, assigned_to, status, notes, created_at, updated_at) FROM stdin;
08ee6832-ef94-4a0c-baa6-d1e8bc393762	LD-20260622-1038	Đỗ Hữu Khang	\N	Male	\N	0969104496	lead38@example.com	\N	Thanh Hóa	\N	\N	\N	\N	Nhật Bản	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Cancelled	\N	2026-06-22 15:00:54.725427+00	2026-06-22 15:00:54.725427+00
18a019cd-57a7-44b6-833c-a59ec2bbbd18	LD-20260613-1033	Nguyễn Thị Lan	\N	Male	\N	0984441306	lead33@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Nhật Bản	\N	Zalo	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	New	\N	2026-06-13 07:00:54.725411+00	2026-06-13 07:00:54.725411+00
1f03ec65-9dd0-48e0-a8ae-fc5a52fe0099	LD-20260514-1032	Phạm Văn An	\N	Male	\N	0992753209	lead32@example.com	\N	Bắc Giang	\N	\N	\N	\N	Nhật Bản	\N	Website	415a3c4c-9fe9-4e58-b4e2-78f11adf1553	019ef7b8-c87b-705f-aecb-05cfc3c92827	NotContacted	\N	2026-05-14 13:00:54.725408+00	2026-05-14 13:00:54.725408+00
21d62dea-7b5e-4ba0-b9ca-2bc5e2642493	LD-20260605-1018	Đỗ Thị Lan	\N	Female	\N	0989208359	lead18@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Đức	\N	Event	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	New	\N	2026-06-05 09:00:54.725365+00	2026-06-05 09:00:54.725365+00
25812455-09c2-40e4-80be-ea98b6b1caa1	LD-20260516-1035	Đỗ Quốc Dũng	\N	Female	\N	0986042729	lead35@example.com	\N	Quảng Bình	\N	\N	\N	\N	Đức	\N	Hotline	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	Appointment	\N	2026-05-16 09:00:54.725418+00	2026-05-16 09:00:54.725418+00
34250d15-765f-4ea9-a333-19f5ecb41e34	LD-20260623-1023	Hồ Thị Nga	\N	Female	\N	0962221851	lead23@example.com	\N	Hà Nội	\N	\N	\N	\N	Đức	\N	LandingPage	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Unsuitable	\N	2026-06-23 21:00:54.72538+00	2026-06-23 21:00:54.72538+00
34257641-42b3-472a-979f-5b8288b2da43	LD-20260622-1013	Trần Minh Châu	\N	Male	\N	0994011548	lead13@example.com	\N	Quảng Bình	\N	\N	\N	\N	Hàn Quốc	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	NotContacted	\N	2026-06-22 23:00:54.72535+00	2026-06-22 23:00:54.72535+00
3b610fcd-5f57-4988-aac9-31644578e7c9	LD-20260512-1030	Vũ Thị Hà	\N	Female	\N	0937807065	lead30@example.com	\N	Hải Dương	\N	\N	\N	\N	Hàn Quốc	\N	LandingPage	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	New	\N	2026-05-12 17:00:54.725402+00	2026-05-12 17:00:54.725402+00
41018f8b-0dfb-4789-9f7f-ca1fb6f6c7aa	LD-20260523-1014	Lê Hữu Khang	\N	Female	\N	0963836001	lead14@example.com	\N	Hải Dương	\N	\N	\N	\N	Đức	\N	LandingPage	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Consulting	\N	2026-05-23 02:00:54.725353+00	2026-05-23 02:00:54.725353+00
4c3d1627-89d3-41f7-af70-f7f893a88b26	LD-20260519-1020	Bùi Minh Châu	\N	Male	\N	0927869604	lead20@example.com	\N	Quảng Bình	\N	\N	\N	\N	Nhật Bản	\N	FacebookAds	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	Consulting	\N	2026-05-19 15:00:54.725371+00	2026-05-19 15:00:54.725371+00
519ebcd4-9794-4ddc-b518-d0012f6c9569	LD-20260603-1029	Phạm Hữu Khang	\N	Male	\N	0955162604	lead29@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Nhật Bản	\N	LandingPage	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-03 05:00:54.725399+00	2026-06-03 05:00:54.725399+00
52f401d4-1c1f-4bc2-b06f-5d39f5a8d5dc	LD-20260528-1022	Phạm Thị Bình	\N	Male	\N	0944838986	lead22@example.com	\N	Quảng Bình	\N	\N	\N	\N	Nhật Bản	\N	LandingPage	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Cancelled	\N	2026-05-28 14:00:54.725377+00	2026-05-28 14:00:54.725377+00
5f7f83a3-8265-4c36-b7ab-27c1935f39cc	LD-20260608-1017	Đỗ Đức Mạnh	\N	Male	\N	0982386280	lead17@example.com	\N	Nghệ An	\N	\N	\N	\N	Đức	\N	Hotline	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	Contacted	\N	2026-06-08 16:00:54.725362+00	2026-06-08 16:00:54.725362+00
61baa13f-42f2-4868-af7f-c82f0140f9d5	LD-20260526-1034	Vũ Văn An	\N	Female	\N	0957735192	lead34@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Đài Loan	\N	Agent	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	New	\N	2026-05-26 02:00:54.725414+00	2026-05-26 02:00:54.725414+00
69cf9616-3bf2-45d4-a264-df37ad46e456	LD-20260604-1006	Hoàng Văn An	\N	Female	\N	0954963711	lead6@example.com	\N	Hà Nội	\N	\N	\N	\N	Đức	\N	FacebookAds	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-04 16:00:54.725326+00	2026-06-04 16:00:54.725326+00
759a961a-bb71-4211-9b63-488c86245928	LD-20260602-1021	Lê Minh Châu	\N	Female	\N	0950841029	lead21@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Hàn Quốc	\N	Event	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Unsuitable	\N	2026-06-02 20:00:54.725374+00	2026-06-02 20:00:54.725374+00
7abcc5bc-55b5-4d2a-8b27-4c46e6623b50	LD-20260621-1037	Vũ Tuấn Phong	\N	Male	\N	0941276148	lead37@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Nhật Bản	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Cancelled	\N	2026-06-21 08:00:54.725424+00	2026-06-21 08:00:54.725424+00
7ee882a6-2ef8-482e-bf61-52e1f4777e6a	LD-20260514-1016	Vũ Thị Hà	\N	Female	\N	0990149050	lead16@example.com	\N	Nghệ An	\N	\N	\N	\N	Nhật Bản	\N	Zalo	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	NotContacted	\N	2026-05-14 05:00:54.725359+00	2026-05-14 05:00:54.725359+00
84fec9f1-902f-4480-9142-398ef9ff100d	LD-20260525-1027	Đặng Thị Bình	\N	Female	\N	0942060899	lead27@example.com	\N	Bắc Giang	\N	\N	\N	\N	Đài Loan	\N	LandingPage	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	Registered	\N	2026-05-25 08:00:54.725392+00	2026-05-25 08:00:54.725392+00
8f1ecda8-bdca-4f2c-a105-ea9073cd0002	LD-20260526-1005	Nguyễn Thị Hà	\N	Male	\N	0911192839	lead5@example.com	\N	Thanh Hóa	\N	\N	\N	\N	Nhật Bản	\N	FacebookAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-26 23:00:54.725323+00	2026-05-26 23:00:54.725323+00
9b587b90-c9b0-427f-9397-fd5414811cf5	LD-20260519-1015	Hoàng Hữu Khang	\N	Male	\N	0919660047	lead15@example.com	\N	Hà Nội	\N	\N	\N	\N	Nhật Bản	\N	Event	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Cancelled	\N	2026-05-19 01:00:54.725356+00	2026-05-19 01:00:54.725356+00
aa57e637-7d3f-4a3b-bd01-aee11313d91f	LD-20260522-1024	Hồ Minh Châu	\N	Male	\N	0940672089	lead24@example.com	\N	Nam Định	\N	\N	\N	\N	Nhật Bản	\N	Hotline	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Cancelled	\N	2026-05-22 04:00:54.725383+00	2026-05-22 04:00:54.725383+00
b3eaca7a-5dbc-4cd8-8e97-27cbbb9a3447	LD-20260514-1012	Nguyễn Văn An	\N	Female	\N	0993359769	lead12@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Nhật Bản	\N	Zalo	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Unsuitable	\N	2026-05-14 23:00:54.725346+00	2026-05-14 23:00:54.725346+00
c0587162-6b90-4cfc-b7dd-3d74f58ffa6c	LD-20260529-1039	Trần Thị Hà	\N	Female	\N	0973880366	lead39@example.com	\N	Thanh Hóa	\N	\N	\N	\N	Đức	\N	LandingPage	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Interested	\N	2026-05-29 05:00:54.72543+00	2026-05-29 05:00:54.72543+00
c18c772a-692e-4f42-971b-0856c609f40c	LD-20260525-1028	Vũ Thị Lan	\N	Female	\N	0972703934	lead28@example.com	\N	Nghệ An	\N	\N	\N	\N	Nhật Bản	\N	Zalo	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Appointment	\N	2026-05-25 05:00:54.725396+00	2026-05-25 05:00:54.725396+00
d7887d45-f6c7-4c3b-bf01-4d6586afbde3	LD-20260528-1026	Hồ Quốc Dũng	\N	Male	\N	0923989251	lead26@example.com	\N	Nghệ An	\N	\N	\N	\N	Hàn Quốc	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	New	\N	2026-05-28 20:00:54.725389+00	2026-05-28 20:00:54.725389+00
df287542-0f74-42b0-9668-0effbd9a2938	LD-20260526-1025	Phạm Minh Châu	\N	Male	\N	0938971951	lead25@example.com	\N	Nam Định	\N	\N	\N	\N	Nhật Bản	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Registered	\N	2026-05-26 14:00:54.725386+00	2026-05-26 14:00:54.725386+00
e363d40a-108d-45cb-8e3c-19780f1b0ed0	LD-20260601-1031	Hoàng Tuấn Phong	\N	Female	\N	0995251137	lead31@example.com	\N	Quảng Bình	\N	\N	\N	\N	Hàn Quốc	\N	Hotline	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Cancelled	\N	2026-06-01 23:00:54.725405+00	2026-06-01 23:00:54.725405+00
f3603a8c-29ab-4c86-b537-13714974db4f	LD-20260511-1036	Đỗ Thị Bình	\N	Male	\N	0983263703	lead36@example.com	\N	Hải Dương	\N	\N	\N	\N	Đức	\N	TiktokAds	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-11 09:00:54.725421+00	2026-05-11 09:00:54.725421+00
f6e0b28e-5c36-49c3-aac5-c13847e85cd1	LD-20260614-1019	Hồ Thị Bình	\N	Female	\N	0955081090	lead19@example.com	\N	Nam Định	\N	\N	\N	\N	Nhật Bản	\N	Referral	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Consulting	\N	2026-06-14 13:00:54.725368+00	2026-06-14 13:00:54.725368+00
15e3c29d-9f15-4d40-af0b-ffd36aa900c5	LD-20260603-1007	Nguyễn Tuấn Phong	\N	Female	\N	0965494546	lead7@example.com	\N	Hà Nội	\N	\N	\N	\N	Nhật Bản	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-03 19:00:54.72533+00	2026-06-03 19:00:54.72533+00
2b19856a-79b6-42e4-932e-0a872cddc7ee	LD-20260521-1009	Trần Thị Hà	\N	Female	\N	0922122421	lead9@example.com	\N	Nghệ An	\N	\N	\N	\N	Đài Loan	\N	Zalo	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-21 01:00:54.725336+00	2026-05-21 01:00:54.725336+00
50903674-44db-414b-ad62-f9590f74091e	LD-20260606-1001	Lê Hữu Khang	\N	Male	\N	0956570598	lead1@example.com	\N	Hà Nội	\N	\N	\N	\N	Đức	\N	Zalo	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-06 22:00:54.725279+00	2026-06-06 22:00:54.725279+00
56e040ee-33e0-4ab8-9802-fd1273876a0a	LD-20260612-1004	Bùi Thị Lan	\N	Female	\N	0936064600	lead4@example.com	\N	Nam Định	\N	\N	\N	\N	Nhật Bản	\N	FacebookAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-12 10:00:54.725319+00	2026-06-12 10:00:54.725319+00
7a19a478-4901-469f-a004-a25d3e3743b0	LD-20260514-1008	Vũ Thị Hà	\N	Male	\N	0976159462	lead8@example.com	\N	Hà Nội	\N	\N	\N	\N	Đức	\N	Event	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-14 14:00:54.725333+00	2026-05-14 14:00:54.725333+00
99baa6c3-1eaa-4a24-9b85-623fc2101b60	LD-20260519-1003	Nguyễn Quốc Dũng	\N	Male	\N	0958214981	lead3@example.com	\N	Hà Tĩnh	\N	\N	\N	\N	Hàn Quốc	\N	Website	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-19 04:00:54.725291+00	2026-05-19 04:00:54.725291+00
a4de1cf1-bfe6-4d80-889f-b0bbe02c8bab	LD-20260531-1002	Nguyễn Đức Mạnh	\N	Male	\N	0974003912	lead2@example.com	\N	Nghệ An	\N	\N	\N	\N	Đức	\N	Hotline	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-31 03:00:54.725287+00	2026-05-31 03:00:54.725287+00
aaf83077-a0ed-44c4-956c-cc4ed8e36068	LD-20260530-1010	Đặng Đức Mạnh	\N	Male	\N	0968574909	lead10@example.com	\N	Hà Nội	\N	\N	\N	\N	Nhật Bản	\N	Event	2b3d004a-8492-430a-8e64-df20ff1aed44	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-05-30 05:00:54.725339+00	2026-05-30 05:00:54.725339+00
b3f4e745-dbb0-4fde-a359-2c0f70cd9c5b	LD-20260601-1000	Đặng Thị Bình	\N	Male	\N	0933633340	lead0@example.com	\N	Nam Định	\N	\N	\N	\N	Đài Loan	\N	TiktokAds	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-01 00:00:54.724766+00	2026-06-01 00:00:54.724766+00
eae11b57-90b0-452a-b4e2-12aa55032eef	LD-20260606-1011	Nguyễn Minh Châu	\N	Female	\N	0999413618	lead11@example.com	\N	Hà Nội	\N	\N	\N	\N	Nhật Bản	\N	Website	829c2745-8331-4965-a2d3-8695d0ad829d	019ef7b8-c87b-705f-aecb-05cfc3c92827	Converted	\N	2026-06-06 02:00:54.725343+00	2026-06-06 02:00:54.725343+00
\.


--
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.notifications (id, user_id, type, title, body, channel, is_read, reference_type, reference_id, sent_at, read_at, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: payments; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.payments (id, code, candidate_id, job_order_id, payment_type, amount, due_date, paid_date, status, payment_method, receipt_id, notes, created_by, approved_by, created_at, updated_at) FROM stdin;
0d5d305a-9b56-45c9-9892-4c73035ac794	PT-20260521-3003	d070bbce-35bd-4a25-ba0b-5377374537e4	31406569-076f-4e8b-8d10-ab07247b1b18	Deposit	20000000.00	\N	2026-05-26	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.933454+00	2026-06-24 04:00:54.933454+00
36858618-1892-43d7-8de8-fa262f17c3ab	PT-20260608-3011	45307427-0bc6-4243-a04c-d23440eb3661	d31c12ab-586a-49e1-be28-4c96f4d7cd03	Deposit	20000000.00	\N	2026-06-13	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.935361+00	2026-06-24 04:00:54.935361+00
7b842cdf-aa8e-4a88-9404-e5effbe3f216	PT-20260605-3007	0644df67-36ad-4595-ae77-39926cd2e8e6	dec6d6a8-6cb7-4afd-ab00-87faa77f2ba0	Deposit	20000000.00	\N	2026-06-10	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.934553+00	2026-06-24 04:00:54.934553+00
8947f2d0-8083-4c5d-8bd0-1374a5180fb9	PT-20260601-3010	9e47bf90-0bdf-440c-a3f7-37814b6654d3	6b94f36a-8d95-420c-b84a-9ce633ee4408	Deposit	20000000.00	\N	2026-06-06	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.93523+00	2026-06-24 04:00:54.935231+00
9c76ff57-dc31-4c2f-a7b4-f665cbcc84b7	PT-20260614-3004	38e9601b-13bc-4dc8-ac46-319cc113af00	31406569-076f-4e8b-8d10-ab07247b1b18	Deposit	20000000.00	\N	2026-06-19	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.933673+00	2026-06-24 04:00:54.933673+00
ab51e4ff-3761-4711-acff-8ecd563ac1ac	PT-20260608-3001	fcb085f0-c3f2-4eb4-8468-a103d53bdf9c	6b94f36a-8d95-420c-b84a-9ce633ee4408	Deposit	20000000.00	\N	2026-06-13	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.932387+00	2026-06-24 04:00:54.932388+00
ad65ec17-90a2-46bd-a749-5112eb4a091c	PT-20260603-3000	e2afc2af-ec08-4bf4-a808-a0d991c43dbb	31406569-076f-4e8b-8d10-ab07247b1b18	Deposit	20000000.00	\N	2026-06-08	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.90604+00	2026-06-24 04:00:54.90604+00
b529a343-ac55-4a03-8fa1-20d187242963	PT-20260602-3002	e6dc06d8-ea69-425e-ae27-f6c7926246d2	31406569-076f-4e8b-8d10-ab07247b1b18	Deposit	20000000.00	\N	2026-06-07	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.932564+00	2026-06-24 04:00:54.932564+00
cb94357c-d1e4-4c3a-9779-57e9ce18afd1	PT-20260516-3008	15b08e0d-179e-4762-acd9-84f0752ebe26	d31c12ab-586a-49e1-be28-4c96f4d7cd03	Deposit	20000000.00	\N	2026-05-21	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.934853+00	2026-06-24 04:00:54.934853+00
d57f805c-fe52-488e-af76-c10ea49ce83d	PT-20260606-3006	79579ce6-1b65-4d6e-937a-478dcf7faf84	6b94f36a-8d95-420c-b84a-9ce633ee4408	Deposit	20000000.00	\N	2026-06-11	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.93426+00	2026-06-24 04:00:54.934261+00
d5aba1b0-bdb0-4877-99e6-e8fce5e09156	PT-20260523-3009	ae6e5fce-26b0-4a4d-bd61-a79db574ff44	d7a64bb2-923e-41ad-a0a6-ce7e1bfcb9fd	Deposit	20000000.00	\N	2026-05-28	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.935066+00	2026-06-24 04:00:54.935066+00
e8d00c14-b85d-444f-ad6e-cda903f11b98	PT-20260528-3005	f04745f2-66e4-4181-b1cb-d4ead5b90a5d	d31c12ab-586a-49e1-be28-4c96f4d7cd03	Deposit	20000000.00	\N	2026-06-02	Paid	\N	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	\N	2026-06-24 04:00:54.934037+00	2026-06-24 04:00:54.934037+00
\.


--
-- Data for Name: permissions; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.permissions (id, name, resource, action, created_at, updated_at) FROM stdin;
0176a45f-971f-4461-8043-61c908000491	agents:create	agents	create	2026-06-24 03:42:22.619443+00	2026-06-24 03:42:22.619443+00
017cdffe-4f60-42ef-84dd-2bb90c0e54f0	expenses:create	expenses	create	2026-06-24 03:42:22.619434+00	2026-06-24 03:42:22.619434+00
01a30cf0-1182-47a8-89ac-0d89004f369a	dashboard:delete	dashboard	delete	2026-06-24 03:42:22.619453+00	2026-06-24 03:42:22.619453+00
02fa450d-c709-4b73-aaca-f0237391e968	agents:delete	agents	delete	2026-06-24 03:42:22.619444+00	2026-06-24 03:42:22.619444+00
03e1a9be-c0fc-4e11-8298-fef8c579e0f1	payments:read	payments	read	2026-06-24 03:42:22.619433+00	2026-06-24 03:42:22.619433+00
053b2fbf-ddeb-4cef-be4e-5475348845ab	job_orders:read	job_orders	read	2026-06-24 03:42:22.619431+00	2026-06-24 03:42:22.619431+00
077925b9-fec8-4e45-a855-cb2e8d357869	roles:create	roles	create	2026-06-24 03:42:22.619463+00	2026-06-24 03:42:22.619463+00
0820ec83-655f-484d-9b3c-2b69384e0686	leads:create	leads	create	2026-06-24 03:42:22.619255+00	2026-06-24 03:42:22.619256+00
08341eea-79d6-45ba-9e24-4ba26e171196	flights:create	flights	create	2026-06-24 03:42:22.619448+00	2026-06-24 03:42:22.619448+00
0845ef1f-32e2-46c7-ab1f-647d60cdd601	reports:update	reports	update	2026-06-24 03:42:22.619451+00	2026-06-24 03:42:22.619451+00
09e82520-ddd6-4e25-863e-d41db995f7e4	audit:update	audit	update	2026-06-24 03:42:22.619467+00	2026-06-24 03:42:22.619467+00
0ab1d653-68f0-4567-8dc2-2bda80984323	payments:approve	payments	approve	2026-06-24 03:42:22.619433+00	2026-06-24 03:42:22.619433+00
0c33d345-4bbf-4255-a11d-57c4bcf8dda2	job_orders:delete	job_orders	delete	2026-06-24 03:42:22.619432+00	2026-06-24 03:42:22.619432+00
0f8b3146-2c7a-4e7d-85ca-c59dcca62dd2	job_orders:update	job_orders	update	2026-06-24 03:42:22.619431+00	2026-06-24 03:42:22.619431+00
127045e2-4707-4eb3-895a-96b60c9a4395	notifications:update	notifications	update	2026-06-24 03:42:22.619465+00	2026-06-24 03:42:22.619465+00
14d013b1-5969-44b4-aebb-2572ff31b153	leads:approve	leads	approve	2026-06-24 03:42:22.619374+00	2026-06-24 03:42:22.619374+00
1a3e0595-18d4-4fc5-8256-e51c44395a32	notifications:read	notifications	read	2026-06-24 03:42:22.619465+00	2026-06-24 03:42:22.619465+00
1afc8382-a489-40e0-a55a-cf11e73cdcfe	commissions:create	commissions	create	2026-06-24 03:42:22.619445+00	2026-06-24 03:42:22.619445+00
1d102910-f63a-4122-a4e4-49740267c746	dashboard:update	dashboard	update	2026-06-24 03:42:22.619452+00	2026-06-24 03:42:22.619453+00
1ea772f1-d336-4bf1-9845-417035e9dca8	flights:approve	flights	approve	2026-06-24 03:42:22.61945+00	2026-06-24 03:42:22.61945+00
1ef45055-f8f7-48cc-a16d-40f045ce7197	receipts:create	receipts	create	2026-06-24 03:42:22.619441+00	2026-06-24 03:42:22.619442+00
1f15b305-abff-4bc9-9bb6-a36d91ea2b69	visas:update	visas	update	2026-06-24 03:42:22.619447+00	2026-06-24 03:42:22.619447+00
2241e52a-5da3-488a-8ec5-3348ce6ffcca	notifications:approve	notifications	approve	2026-06-24 03:42:22.619466+00	2026-06-24 03:42:22.619466+00
23808083-8fcc-4069-b657-e8b15239b467	audit:approve	audit	approve	2026-06-24 03:42:22.619468+00	2026-06-24 03:42:22.619468+00
2685cc7f-9462-4f95-a0ca-c7aec7d33c6f	payments:delete	payments	delete	2026-06-24 03:42:22.619433+00	2026-06-24 03:42:22.619433+00
270db067-64c5-4ea2-85ea-fb6eeea7fa8f	leads:read	leads	read	2026-06-24 03:42:22.619371+00	2026-06-24 03:42:22.619371+00
29ba4da2-837d-4eb9-848f-0b9632078d3b	expenses:read	expenses	read	2026-06-24 03:42:22.619434+00	2026-06-24 03:42:22.619434+00
2d16d083-fe94-4962-b8c4-a4f813bb3bd7	users:delete	users	delete	2026-06-24 03:42:22.619455+00	2026-06-24 03:42:22.619455+00
2f18fcd0-9527-437c-9503-b655ce307c58	visas:create	visas	create	2026-06-24 03:42:22.619447+00	2026-06-24 03:42:22.619447+00
33345c87-167a-4980-aa24-779dcc09571c	payments:update	payments	update	2026-06-24 03:42:22.619433+00	2026-06-24 03:42:22.619433+00
346e4c9e-98bd-4d8a-83a4-c611f0ce5029	receipts:read	receipts	read	2026-06-24 03:42:22.619442+00	2026-06-24 03:42:22.619442+00
34741e2d-a00b-43f3-8a69-730ec5d24ff3	commissions:delete	commissions	delete	2026-06-24 03:42:22.619446+00	2026-06-24 03:42:22.619446+00
391cfe53-1829-4548-8f2b-311b84a16d84	users:read	users	read	2026-06-24 03:42:22.619454+00	2026-06-24 03:42:22.619454+00
3a8abd40-2206-41bb-b29d-48e344140524	payments:create	payments	create	2026-06-24 03:42:22.619432+00	2026-06-24 03:42:22.619432+00
3b9d8bc9-16c9-4e4f-af25-0157fbe8aac4	receipts:delete	receipts	delete	2026-06-24 03:42:22.619442+00	2026-06-24 03:42:22.619443+00
405b0ff7-79fc-46a1-90eb-dbb26aa0b5d1	expenses:update	expenses	update	2026-06-24 03:42:22.619434+00	2026-06-24 03:42:22.619434+00
40825090-9883-41f8-8abc-897e4d6f6a93	agents:approve	agents	approve	2026-06-24 03:42:22.619445+00	2026-06-24 03:42:22.619445+00
410c6be4-dcea-44ee-9555-4ffa1e064edd	agents:read	agents	read	2026-06-24 03:42:22.619443+00	2026-06-24 03:42:22.619443+00
42da3476-d256-4e50-8fbd-1457671d84a5	reports:approve	reports	approve	2026-06-24 03:42:22.619451+00	2026-06-24 03:42:22.619451+00
4764d05e-748d-42d4-8e6a-296992a93337	flights:update	flights	update	2026-06-24 03:42:22.619449+00	2026-06-24 03:42:22.619449+00
4c6b23af-2fff-49a7-aed7-356d9e6d33cb	flights:delete	flights	delete	2026-06-24 03:42:22.619449+00	2026-06-24 03:42:22.619449+00
4e81fcf3-b24f-4de9-a607-fa0061221cd7	dashboard:read	dashboard	read	2026-06-24 03:42:22.619452+00	2026-06-24 03:42:22.619452+00
4f485406-c4f1-4681-9031-3983876f337e	commissions:update	commissions	update	2026-06-24 03:42:22.619446+00	2026-06-24 03:42:22.619446+00
5e6b98a3-cf64-41a1-ba3a-bc7f7883cba6	audit:create	audit	create	2026-06-24 03:42:22.619466+00	2026-06-24 03:42:22.619466+00
69225076-973a-4484-930c-371744a28408	notifications:create	notifications	create	2026-06-24 03:42:22.619465+00	2026-06-24 03:42:22.619465+00
6cfffeec-2e43-4d70-871f-808b20ff7559	candidates:delete	candidates	delete	2026-06-24 03:42:22.619375+00	2026-06-24 03:42:22.619375+00
6eea62a6-1afa-4973-a202-cd306fb45fe1	job_orders:approve	job_orders	approve	2026-06-24 03:42:22.619432+00	2026-06-24 03:42:22.619432+00
7a99b9a4-2036-4254-978d-5bf5997785d0	commissions:read	commissions	read	2026-06-24 03:42:22.619446+00	2026-06-24 03:42:22.619446+00
7aba870a-f10b-47dc-bb37-2229bf3ae86d	users:create	users	create	2026-06-24 03:42:22.619454+00	2026-06-24 03:42:22.619454+00
7ba9984e-0260-4c2d-b6f7-04698d440287	visas:read	visas	read	2026-06-24 03:42:22.619447+00	2026-06-24 03:42:22.619447+00
80fa960b-a808-4e95-b5c1-9890bf93299a	dashboard:create	dashboard	create	2026-06-24 03:42:22.619452+00	2026-06-24 03:42:22.619452+00
823f95e4-a44f-48aa-a57d-7a54544bcb30	audit:read	audit	read	2026-06-24 03:42:22.619467+00	2026-06-24 03:42:22.619467+00
8262e0a9-5656-4c79-9d9e-ede309cfc7ad	commissions:approve	commissions	approve	2026-06-24 03:42:22.619446+00	2026-06-24 03:42:22.619446+00
870685ab-4ecd-4e07-a7a5-d7ee25e66c66	receipts:approve	receipts	approve	2026-06-24 03:42:22.619443+00	2026-06-24 03:42:22.619443+00
8832d1e8-1f38-482d-8b8b-1775cf283548	audit:delete	audit	delete	2026-06-24 03:42:22.619467+00	2026-06-24 03:42:22.619467+00
8845c3ea-c0e2-4642-b788-8979d1eacdc5	reports:read	reports	read	2026-06-24 03:42:22.61945+00	2026-06-24 03:42:22.61945+00
8f4300ac-e34b-4254-8cff-2efe7003a64e	candidates:update	candidates	update	2026-06-24 03:42:22.619375+00	2026-06-24 03:42:22.619375+00
95d9e47e-1cca-421c-b8fb-d30929b428aa	agents:update	agents	update	2026-06-24 03:42:22.619444+00	2026-06-24 03:42:22.619444+00
9f90c265-9bf7-4d76-bce7-7de7579fab50	visas:delete	visas	delete	2026-06-24 03:42:22.619448+00	2026-06-24 03:42:22.619448+00
a451f0df-2510-4906-92bb-1e96baa40b67	leads:delete	leads	delete	2026-06-24 03:42:22.619372+00	2026-06-24 03:42:22.619372+00
aebb0c50-1422-4c0b-b288-2d2007ddfecf	reports:create	reports	create	2026-06-24 03:42:22.61945+00	2026-06-24 03:42:22.61945+00
af5acded-3994-48ae-a08e-2401756ba776	dashboard:approve	dashboard	approve	2026-06-24 03:42:22.619453+00	2026-06-24 03:42:22.619453+00
b15222f7-a5dd-4c05-bdc9-ce402291a923	flights:read	flights	read	2026-06-24 03:42:22.619449+00	2026-06-24 03:42:22.619449+00
b1cf925a-d943-4480-b9f4-36ad3b3ca899	reports:delete	reports	delete	2026-06-24 03:42:22.619451+00	2026-06-24 03:42:22.619451+00
c1964da9-4a7c-4980-affc-ce4ee83402b0	users:update	users	update	2026-06-24 03:42:22.619455+00	2026-06-24 03:42:22.619455+00
c69fd33b-4096-45d3-b395-9b5bb830ae35	users:approve	users	approve	2026-06-24 03:42:22.619463+00	2026-06-24 03:42:22.619463+00
c6dd7b95-a1fe-430d-a9ec-7e7dc39d8b16	roles:delete	roles	delete	2026-06-24 03:42:22.619464+00	2026-06-24 03:42:22.619464+00
c7c4526c-ac29-4f60-9b84-1e4d5c97c010	candidates:create	candidates	create	2026-06-24 03:42:22.619374+00	2026-06-24 03:42:22.619374+00
c881a60c-e7b6-4b1d-9eb7-16c589eb5cb8	roles:read	roles	read	2026-06-24 03:42:22.619463+00	2026-06-24 03:42:22.619463+00
ccc0619d-3574-48b3-83ac-82f5647a6c8a	receipts:update	receipts	update	2026-06-24 03:42:22.619442+00	2026-06-24 03:42:22.619442+00
d8f8a087-a306-46ac-95e4-1a41d05e84ad	job_orders:create	job_orders	create	2026-06-24 03:42:22.61943+00	2026-06-24 03:42:22.61943+00
da7be304-af3a-4e5a-8ae1-6132328231f3	expenses:approve	expenses	approve	2026-06-24 03:42:22.619435+00	2026-06-24 03:42:22.619435+00
dfd76275-205a-4b96-aed3-f50d9ee221e9	notifications:delete	notifications	delete	2026-06-24 03:42:22.619466+00	2026-06-24 03:42:22.619466+00
e1e9e2f5-b1a1-4f69-98d0-f3a418c73cd7	leads:update	leads	update	2026-06-24 03:42:22.619372+00	2026-06-24 03:42:22.619372+00
e4cf553f-379b-43fe-8fa3-f15d831e14b2	visas:approve	visas	approve	2026-06-24 03:42:22.619448+00	2026-06-24 03:42:22.619448+00
e5e5d3ab-c5ba-419e-857c-b098a600b3af	expenses:delete	expenses	delete	2026-06-24 03:42:22.619435+00	2026-06-24 03:42:22.619435+00
ebdbb745-f8f4-4428-86da-84252db49164	candidates:read	candidates	read	2026-06-24 03:42:22.619375+00	2026-06-24 03:42:22.619375+00
f141cac9-a22b-4ea4-8f39-165a7464a058	roles:approve	roles	approve	2026-06-24 03:42:22.619464+00	2026-06-24 03:42:22.619464+00
f36d0002-744f-4d7b-a308-ed623d30a645	roles:update	roles	update	2026-06-24 03:42:22.619464+00	2026-06-24 03:42:22.619464+00
f5338144-b3d0-4ff2-9c12-cb50b950820d	candidates:approve	candidates	approve	2026-06-24 03:42:22.61943+00	2026-06-24 03:42:22.61943+00
\.


--
-- Data for Name: receipts; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.receipts (id, code, receipt_type, candidate_id, agent_id, amount, description, receipt_date, pdf_url, signed_by, created_by, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: role_permissions; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.role_permissions (role_id, permission_id) FROM stdin;
019ef7b8-c6c8-71df-bec4-242eb846f44a	0176a45f-971f-4461-8043-61c908000491
019ef7b8-c6c8-71df-bec4-242eb846f44a	017cdffe-4f60-42ef-84dd-2bb90c0e54f0
019ef7b8-c6c8-71df-bec4-242eb846f44a	01a30cf0-1182-47a8-89ac-0d89004f369a
019ef7b8-c6c8-71df-bec4-242eb846f44a	02fa450d-c709-4b73-aaca-f0237391e968
019ef7b8-c6c8-71df-bec4-242eb846f44a	03e1a9be-c0fc-4e11-8298-fef8c579e0f1
019ef7b8-c6c8-71df-bec4-242eb846f44a	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c6c8-71df-bec4-242eb846f44a	077925b9-fec8-4e45-a855-cb2e8d357869
019ef7b8-c6c8-71df-bec4-242eb846f44a	0820ec83-655f-484d-9b3c-2b69384e0686
019ef7b8-c6c8-71df-bec4-242eb846f44a	08341eea-79d6-45ba-9e24-4ba26e171196
019ef7b8-c6c8-71df-bec4-242eb846f44a	0845ef1f-32e2-46c7-ab1f-647d60cdd601
019ef7b8-c6c8-71df-bec4-242eb846f44a	09e82520-ddd6-4e25-863e-d41db995f7e4
019ef7b8-c6c8-71df-bec4-242eb846f44a	0ab1d653-68f0-4567-8dc2-2bda80984323
019ef7b8-c6c8-71df-bec4-242eb846f44a	0c33d345-4bbf-4255-a11d-57c4bcf8dda2
019ef7b8-c6c8-71df-bec4-242eb846f44a	0f8b3146-2c7a-4e7d-85ca-c59dcca62dd2
019ef7b8-c6c8-71df-bec4-242eb846f44a	127045e2-4707-4eb3-895a-96b60c9a4395
019ef7b8-c6c8-71df-bec4-242eb846f44a	14d013b1-5969-44b4-aebb-2572ff31b153
019ef7b8-c6c8-71df-bec4-242eb846f44a	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c6c8-71df-bec4-242eb846f44a	1afc8382-a489-40e0-a55a-cf11e73cdcfe
019ef7b8-c6c8-71df-bec4-242eb846f44a	1d102910-f63a-4122-a4e4-49740267c746
019ef7b8-c6c8-71df-bec4-242eb846f44a	1ea772f1-d336-4bf1-9845-417035e9dca8
019ef7b8-c6c8-71df-bec4-242eb846f44a	1ef45055-f8f7-48cc-a16d-40f045ce7197
019ef7b8-c6c8-71df-bec4-242eb846f44a	1f15b305-abff-4bc9-9bb6-a36d91ea2b69
019ef7b8-c6c8-71df-bec4-242eb846f44a	2241e52a-5da3-488a-8ec5-3348ce6ffcca
019ef7b8-c6c8-71df-bec4-242eb846f44a	23808083-8fcc-4069-b657-e8b15239b467
019ef7b8-c6c8-71df-bec4-242eb846f44a	2685cc7f-9462-4f95-a0ca-c7aec7d33c6f
019ef7b8-c6c8-71df-bec4-242eb846f44a	270db067-64c5-4ea2-85ea-fb6eeea7fa8f
019ef7b8-c6c8-71df-bec4-242eb846f44a	29ba4da2-837d-4eb9-848f-0b9632078d3b
019ef7b8-c6c8-71df-bec4-242eb846f44a	2d16d083-fe94-4962-b8c4-a4f813bb3bd7
019ef7b8-c6c8-71df-bec4-242eb846f44a	2f18fcd0-9527-437c-9503-b655ce307c58
019ef7b8-c6c8-71df-bec4-242eb846f44a	33345c87-167a-4980-aa24-779dcc09571c
019ef7b8-c6c8-71df-bec4-242eb846f44a	346e4c9e-98bd-4d8a-83a4-c611f0ce5029
019ef7b8-c6c8-71df-bec4-242eb846f44a	34741e2d-a00b-43f3-8a69-730ec5d24ff3
019ef7b8-c6c8-71df-bec4-242eb846f44a	391cfe53-1829-4548-8f2b-311b84a16d84
019ef7b8-c6c8-71df-bec4-242eb846f44a	3a8abd40-2206-41bb-b29d-48e344140524
019ef7b8-c6c8-71df-bec4-242eb846f44a	3b9d8bc9-16c9-4e4f-af25-0157fbe8aac4
019ef7b8-c6c8-71df-bec4-242eb846f44a	405b0ff7-79fc-46a1-90eb-dbb26aa0b5d1
019ef7b8-c6c8-71df-bec4-242eb846f44a	40825090-9883-41f8-8abc-897e4d6f6a93
019ef7b8-c6c8-71df-bec4-242eb846f44a	410c6be4-dcea-44ee-9555-4ffa1e064edd
019ef7b8-c6c8-71df-bec4-242eb846f44a	42da3476-d256-4e50-8fbd-1457671d84a5
019ef7b8-c6c8-71df-bec4-242eb846f44a	4764d05e-748d-42d4-8e6a-296992a93337
019ef7b8-c6c8-71df-bec4-242eb846f44a	4c6b23af-2fff-49a7-aed7-356d9e6d33cb
019ef7b8-c6c8-71df-bec4-242eb846f44a	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c6c8-71df-bec4-242eb846f44a	4f485406-c4f1-4681-9031-3983876f337e
019ef7b8-c6c8-71df-bec4-242eb846f44a	5e6b98a3-cf64-41a1-ba3a-bc7f7883cba6
019ef7b8-c6c8-71df-bec4-242eb846f44a	69225076-973a-4484-930c-371744a28408
019ef7b8-c6c8-71df-bec4-242eb846f44a	6cfffeec-2e43-4d70-871f-808b20ff7559
019ef7b8-c6c8-71df-bec4-242eb846f44a	6eea62a6-1afa-4973-a202-cd306fb45fe1
019ef7b8-c6c8-71df-bec4-242eb846f44a	7a99b9a4-2036-4254-978d-5bf5997785d0
019ef7b8-c6c8-71df-bec4-242eb846f44a	7aba870a-f10b-47dc-bb37-2229bf3ae86d
019ef7b8-c6c8-71df-bec4-242eb846f44a	7ba9984e-0260-4c2d-b6f7-04698d440287
019ef7b8-c6c8-71df-bec4-242eb846f44a	80fa960b-a808-4e95-b5c1-9890bf93299a
019ef7b8-c6c8-71df-bec4-242eb846f44a	823f95e4-a44f-48aa-a57d-7a54544bcb30
019ef7b8-c6c8-71df-bec4-242eb846f44a	8262e0a9-5656-4c79-9d9e-ede309cfc7ad
019ef7b8-c6c8-71df-bec4-242eb846f44a	870685ab-4ecd-4e07-a7a5-d7ee25e66c66
019ef7b8-c6c8-71df-bec4-242eb846f44a	8832d1e8-1f38-482d-8b8b-1775cf283548
019ef7b8-c6c8-71df-bec4-242eb846f44a	8845c3ea-c0e2-4642-b788-8979d1eacdc5
019ef7b8-c6c8-71df-bec4-242eb846f44a	8f4300ac-e34b-4254-8cff-2efe7003a64e
019ef7b8-c6c8-71df-bec4-242eb846f44a	95d9e47e-1cca-421c-b8fb-d30929b428aa
019ef7b8-c6c8-71df-bec4-242eb846f44a	9f90c265-9bf7-4d76-bce7-7de7579fab50
019ef7b8-c6c8-71df-bec4-242eb846f44a	a451f0df-2510-4906-92bb-1e96baa40b67
019ef7b8-c6c8-71df-bec4-242eb846f44a	aebb0c50-1422-4c0b-b288-2d2007ddfecf
019ef7b8-c6c8-71df-bec4-242eb846f44a	af5acded-3994-48ae-a08e-2401756ba776
019ef7b8-c6c8-71df-bec4-242eb846f44a	b15222f7-a5dd-4c05-bdc9-ce402291a923
019ef7b8-c6c8-71df-bec4-242eb846f44a	b1cf925a-d943-4480-b9f4-36ad3b3ca899
019ef7b8-c6c8-71df-bec4-242eb846f44a	c1964da9-4a7c-4980-affc-ce4ee83402b0
019ef7b8-c6c8-71df-bec4-242eb846f44a	c69fd33b-4096-45d3-b395-9b5bb830ae35
019ef7b8-c6c8-71df-bec4-242eb846f44a	c6dd7b95-a1fe-430d-a9ec-7e7dc39d8b16
019ef7b8-c6c8-71df-bec4-242eb846f44a	c7c4526c-ac29-4f60-9b84-1e4d5c97c010
019ef7b8-c6c8-71df-bec4-242eb846f44a	c881a60c-e7b6-4b1d-9eb7-16c589eb5cb8
019ef7b8-c6c8-71df-bec4-242eb846f44a	ccc0619d-3574-48b3-83ac-82f5647a6c8a
019ef7b8-c6c8-71df-bec4-242eb846f44a	d8f8a087-a306-46ac-95e4-1a41d05e84ad
019ef7b8-c6c8-71df-bec4-242eb846f44a	da7be304-af3a-4e5a-8ae1-6132328231f3
019ef7b8-c6c8-71df-bec4-242eb846f44a	dfd76275-205a-4b96-aed3-f50d9ee221e9
019ef7b8-c6c8-71df-bec4-242eb846f44a	e1e9e2f5-b1a1-4f69-98d0-f3a418c73cd7
019ef7b8-c6c8-71df-bec4-242eb846f44a	e4cf553f-379b-43fe-8fa3-f15d831e14b2
019ef7b8-c6c8-71df-bec4-242eb846f44a	e5e5d3ab-c5ba-419e-857c-b098a600b3af
019ef7b8-c6c8-71df-bec4-242eb846f44a	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c6c8-71df-bec4-242eb846f44a	f141cac9-a22b-4ea4-8f39-165a7464a058
019ef7b8-c6c8-71df-bec4-242eb846f44a	f36d0002-744f-4d7b-a308-ed623d30a645
019ef7b8-c6c8-71df-bec4-242eb846f44a	f5338144-b3d0-4ff2-9c12-cb50b950820d
019ef7b8-c762-782b-95ea-5666f661464b	03e1a9be-c0fc-4e11-8298-fef8c579e0f1
019ef7b8-c762-782b-95ea-5666f661464b	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c762-782b-95ea-5666f661464b	0ab1d653-68f0-4567-8dc2-2bda80984323
019ef7b8-c762-782b-95ea-5666f661464b	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c762-782b-95ea-5666f661464b	270db067-64c5-4ea2-85ea-fb6eeea7fa8f
019ef7b8-c762-782b-95ea-5666f661464b	29ba4da2-837d-4eb9-848f-0b9632078d3b
019ef7b8-c762-782b-95ea-5666f661464b	346e4c9e-98bd-4d8a-83a4-c611f0ce5029
019ef7b8-c762-782b-95ea-5666f661464b	391cfe53-1829-4548-8f2b-311b84a16d84
019ef7b8-c762-782b-95ea-5666f661464b	410c6be4-dcea-44ee-9555-4ffa1e064edd
019ef7b8-c762-782b-95ea-5666f661464b	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c762-782b-95ea-5666f661464b	7a99b9a4-2036-4254-978d-5bf5997785d0
019ef7b8-c762-782b-95ea-5666f661464b	7ba9984e-0260-4c2d-b6f7-04698d440287
019ef7b8-c762-782b-95ea-5666f661464b	823f95e4-a44f-48aa-a57d-7a54544bcb30
019ef7b8-c762-782b-95ea-5666f661464b	8262e0a9-5656-4c79-9d9e-ede309cfc7ad
019ef7b8-c762-782b-95ea-5666f661464b	8845c3ea-c0e2-4642-b788-8979d1eacdc5
019ef7b8-c762-782b-95ea-5666f661464b	aebb0c50-1422-4c0b-b288-2d2007ddfecf
019ef7b8-c762-782b-95ea-5666f661464b	b15222f7-a5dd-4c05-bdc9-ce402291a923
019ef7b8-c762-782b-95ea-5666f661464b	c881a60c-e7b6-4b1d-9eb7-16c589eb5cb8
019ef7b8-c762-782b-95ea-5666f661464b	da7be304-af3a-4e5a-8ae1-6132328231f3
019ef7b8-c762-782b-95ea-5666f661464b	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	0820ec83-655f-484d-9b3c-2b69384e0686
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	270db067-64c5-4ea2-85ea-fb6eeea7fa8f
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	410c6be4-dcea-44ee-9555-4ffa1e064edd
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	8845c3ea-c0e2-4642-b788-8979d1eacdc5
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	8f4300ac-e34b-4254-8cff-2efe7003a64e
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	a451f0df-2510-4906-92bb-1e96baa40b67
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	c7c4526c-ac29-4f60-9b84-1e4d5c97c010
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	e1e9e2f5-b1a1-4f69-98d0-f3a418c73cd7
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	0820ec83-655f-484d-9b3c-2b69384e0686
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	270db067-64c5-4ea2-85ea-fb6eeea7fa8f
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	410c6be4-dcea-44ee-9555-4ffa1e064edd
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	8f4300ac-e34b-4254-8cff-2efe7003a64e
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	c7c4526c-ac29-4f60-9b84-1e4d5c97c010
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	e1e9e2f5-b1a1-4f69-98d0-f3a418c73cd7
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c774-7a82-946b-f1ef65b242a9	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c774-7a82-946b-f1ef65b242a9	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c774-7a82-946b-f1ef65b242a9	270db067-64c5-4ea2-85ea-fb6eeea7fa8f
019ef7b8-c774-7a82-946b-f1ef65b242a9	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c774-7a82-946b-f1ef65b242a9	7ba9984e-0260-4c2d-b6f7-04698d440287
019ef7b8-c774-7a82-946b-f1ef65b242a9	8f4300ac-e34b-4254-8cff-2efe7003a64e
019ef7b8-c774-7a82-946b-f1ef65b242a9	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c778-7182-b4f2-49e3170b89b0	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c778-7182-b4f2-49e3170b89b0	08341eea-79d6-45ba-9e24-4ba26e171196
019ef7b8-c778-7182-b4f2-49e3170b89b0	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c778-7182-b4f2-49e3170b89b0	1ea772f1-d336-4bf1-9845-417035e9dca8
019ef7b8-c778-7182-b4f2-49e3170b89b0	1f15b305-abff-4bc9-9bb6-a36d91ea2b69
019ef7b8-c778-7182-b4f2-49e3170b89b0	2f18fcd0-9527-437c-9503-b655ce307c58
019ef7b8-c778-7182-b4f2-49e3170b89b0	4764d05e-748d-42d4-8e6a-296992a93337
019ef7b8-c778-7182-b4f2-49e3170b89b0	4c6b23af-2fff-49a7-aed7-356d9e6d33cb
019ef7b8-c778-7182-b4f2-49e3170b89b0	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c778-7182-b4f2-49e3170b89b0	7ba9984e-0260-4c2d-b6f7-04698d440287
019ef7b8-c778-7182-b4f2-49e3170b89b0	8f4300ac-e34b-4254-8cff-2efe7003a64e
019ef7b8-c778-7182-b4f2-49e3170b89b0	9f90c265-9bf7-4d76-bce7-7de7579fab50
019ef7b8-c778-7182-b4f2-49e3170b89b0	b15222f7-a5dd-4c05-bdc9-ce402291a923
019ef7b8-c778-7182-b4f2-49e3170b89b0	e4cf553f-379b-43fe-8fa3-f15d831e14b2
019ef7b8-c778-7182-b4f2-49e3170b89b0	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	017cdffe-4f60-42ef-84dd-2bb90c0e54f0
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	03e1a9be-c0fc-4e11-8298-fef8c579e0f1
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	053b2fbf-ddeb-4cef-be4e-5475348845ab
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	0ab1d653-68f0-4567-8dc2-2bda80984323
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	1afc8382-a489-40e0-a55a-cf11e73cdcfe
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	1ef45055-f8f7-48cc-a16d-40f045ce7197
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	2685cc7f-9462-4f95-a0ca-c7aec7d33c6f
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	29ba4da2-837d-4eb9-848f-0b9632078d3b
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	33345c87-167a-4980-aa24-779dcc09571c
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	346e4c9e-98bd-4d8a-83a4-c611f0ce5029
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	34741e2d-a00b-43f3-8a69-730ec5d24ff3
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	3a8abd40-2206-41bb-b29d-48e344140524
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	3b9d8bc9-16c9-4e4f-af25-0157fbe8aac4
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	405b0ff7-79fc-46a1-90eb-dbb26aa0b5d1
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	410c6be4-dcea-44ee-9555-4ffa1e064edd
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	4f485406-c4f1-4681-9031-3983876f337e
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	7a99b9a4-2036-4254-978d-5bf5997785d0
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	8262e0a9-5656-4c79-9d9e-ede309cfc7ad
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	870685ab-4ecd-4e07-a7a5-d7ee25e66c66
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	8845c3ea-c0e2-4642-b788-8979d1eacdc5
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	ccc0619d-3574-48b3-83ac-82f5647a6c8a
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	da7be304-af3a-4e5a-8ae1-6132328231f3
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	e5e5d3ab-c5ba-419e-857c-b098a600b3af
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	ebdbb745-f8f4-4428-86da-84252db49164
019ef7b8-c783-7fd4-aae7-a47cea63b5f3	1a3e0595-18d4-4fc5-8256-e51c44395a32
019ef7b8-c783-7fd4-aae7-a47cea63b5f3	4e81fcf3-b24f-4de9-a607-fa0061221cd7
019ef7b8-c783-7fd4-aae7-a47cea63b5f3	7a99b9a4-2036-4254-978d-5bf5997785d0
019ef7b8-c783-7fd4-aae7-a47cea63b5f3	8845c3ea-c0e2-4642-b788-8979d1eacdc5
019ef7b8-c783-7fd4-aae7-a47cea63b5f3	ebdbb745-f8f4-4428-86da-84252db49164
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.roles (id, description, name, normalized_name, concurrency_stamp) FROM stdin;
019ef7b8-c6c8-71df-bec4-242eb846f44a	Quyền cao nhất	super_admin	SUPER_ADMIN	c1a85a9b-5644-48ca-ac5e-2dd7d079e8e5
019ef7b8-c762-782b-95ea-5666f661464b	Giám đốc	director	DIRECTOR	08a54982-1429-4f92-b684-15df979c9a43
019ef7b8-c769-7d13-bc34-4b39d2e0acb9	Trưởng phòng tuyển dụng	recruitment_manager	RECRUITMENT_MANAGER	2fb11cff-5b02-4555-a14f-d2e104908169
019ef7b8-c76f-7e7d-ac60-2919cff7c62c	Nhân viên tuyển dụng	recruiter	RECRUITER	de5a195a-d188-43c9-9a14-642c83bf4a31
019ef7b8-c774-7a82-946b-f1ef65b242a9	Bộ phận hồ sơ	document_staff	DOCUMENT_STAFF	c54d8032-cb4d-4635-b85e-23a8bcc3dec4
019ef7b8-c778-7182-b4f2-49e3170b89b0	Bộ phận visa	visa_staff	VISA_STAFF	0cf9efc6-8ecc-4145-bb6a-3ffc06e9b712
019ef7b8-c77e-736f-ad6e-1930f7e1bf10	Kế toán	accountant	ACCOUNTANT	54331852-7d9a-4567-ae39-8d502ed42276
019ef7b8-c783-7fd4-aae7-a47cea63b5f3	Đại lý / CTV	agent	AGENT	9e53f012-ee77-4795-907f-dbf012f8784a
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.users (id, full_name, is_active, last_login_at, created_at, user_name, normalized_user_name, email, normalized_email, email_confirmed, password_hash, security_stamp, concurrency_stamp, phone_number, phone_number_confirmed, two_factor_enabled, lockout_end, lockout_enabled, access_failed_count) FROM stdin;
019ef7b8-c87b-705f-aecb-05cfc3c92827	Super Admin	t	\N	2026-06-24 03:42:22.757504+00	admin@polymind.local	ADMIN@POLYMIND.LOCAL	admin@polymind.local	ADMIN@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEHtkZucm4ArNlMQFHpRqF5Qtc9GYff7LGdNlYepvjsP0EkA8wEJuaCLCydjVRAzjgA==	JQVP5FGEUR7IEC3ZCNNDUJOAHFZEPQKF	3e35e6e8-92b6-4fe8-be3d-4053cc5c1484	\N	f	f	\N	t	0
019ef84b-078a-7465-b909-89a6a20c9113	Giám đốc	t	\N	2026-06-24 06:22:07.163687+00	director@polymind.local	DIRECTOR@POLYMIND.LOCAL	director@polymind.local	DIRECTOR@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEMOl++Skqk6Se4RskcjisvNE3AtwGqA9lXPpY9VQ1MI7pj62vvPmXjHXtf1q4bIrjQ==	7PZQPNZWGAB7SF33RYFJD37FRTZXQT66	4f0deaa0-4830-4853-89d6-e521c6137848	\N	f	f	\N	t	0
019ef84b-0801-7735-af48-49dc9483fa27	Trưởng phòng tuyển dụng	t	\N	2026-06-24 06:22:07.305494+00	recruitment.manager@polymind.local	RECRUITMENT.MANAGER@POLYMIND.LOCAL	recruitment.manager@polymind.local	RECRUITMENT.MANAGER@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEDXozicA4AGd+/+iPsiaAAYz5qDLz3p7Vydv2nI33FiDonoqAp4/3+Ludac1+NJukw==	WYAZAZHWDP3WNDXOOJ4TGHS6XUF5FIDJ	508ec221-7bfa-4285-bc93-b97716a580f4	\N	f	f	\N	t	0
019ef84b-0849-773f-a462-195a31e9bb9b	Nhân viên tuyển dụng	t	\N	2026-06-24 06:22:07.378061+00	recruiter@polymind.local	RECRUITER@POLYMIND.LOCAL	recruiter@polymind.local	RECRUITER@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEGpVFWVgeNWxvM8hoZ2i/ZtmBsUvt4phnI7ZQ+eGpc2bK/VBAmLlIrvvx8GyGJxiOw==	I6PEANPOTG64IDOB2BECVBSBLVSIDBSM	2285287e-785b-4938-8250-1a7e2943f385	\N	f	f	\N	t	0
019ef84b-088f-724c-9a87-09edcc5c5cb1	Bộ phận hồ sơ	t	\N	2026-06-24 06:22:07.449108+00	document.staff@polymind.local	DOCUMENT.STAFF@POLYMIND.LOCAL	document.staff@polymind.local	DOCUMENT.STAFF@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEAVg72Mdr8BYiFAyj97d5BVjeLeNod/+s9xrQnDIvB4hlqUFBz90w+1kf+OA8KkxCA==	YNYJUXXEOXIJ5V3U4EHFCK6EVQ7F3OJB	069affb0-701b-4160-9966-60df5164b17f	\N	f	f	\N	t	0
019ef84b-08dd-7102-9c9a-b824901ae4e3	Bộ phận visa	t	\N	2026-06-24 06:22:07.518452+00	visa.staff@polymind.local	VISA.STAFF@POLYMIND.LOCAL	visa.staff@polymind.local	VISA.STAFF@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEFBwdobIxvfS6uiuChqp9XFxTdJu26qyCHNJ1dH0b+EV+KMfXe4BKFMmPVuWQYtJBA==	7FH7JN2LCAFXPUA4QYOPAACSDOFHQJ4Q	3452feba-3bf2-425b-a604-c2eac75f8da6	\N	f	f	\N	t	0
019ef84b-0928-7cf4-bf25-9c6a8ce31402	Kế toán	t	\N	2026-06-24 06:22:07.598913+00	accountant@polymind.local	ACCOUNTANT@POLYMIND.LOCAL	accountant@polymind.local	ACCOUNTANT@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAELe7l9DNTdlywmvEVfPF/VLOtdchTsvgW/BMld23gJo76fIRZf4Amdnwj+JTrMeE2Q==	DZG4M3775MO7JTSQWEQC5EPW6J645VOC	2a3cfc86-191d-44d9-8f5c-b1cc2b18c0ef	\N	f	f	\N	t	0
019ef84b-0973-773d-97ac-a1ea7ca8328b	Đại lý / CTV	t	\N	2026-06-24 06:22:07.674078+00	agent@polymind.local	AGENT@POLYMIND.LOCAL	agent@polymind.local	AGENT@POLYMIND.LOCAL	t	AQAAAAIAAYagAAAAEISlLxLB6/XrKcypHGsjwn0kyjk58czB7rzkcUppu++kj08L2xFPlE3Fa73SeNcEvw==	H4AP7ECLMEOGKCKSV5MCSJFIBRZKM7NR	307849b0-31b9-484b-96ec-7dccf1099455	\N	f	f	\N	t	0
\.


--
-- Data for Name: visas; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.visas (id, candidate_id, job_order_id, visa_type, country, submitted_date, interview_date, result_date, status, rejection_reason, notes, handled_by, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: workflow_step_records; Type: TABLE DATA; Schema: public; Owner: polymind
--

COPY public.workflow_step_records (id, candidate_job_order_id, step, status, assigned_to, started_at, completed_at, notes, created_by, created_at, updated_at) FROM stdin;
012821dc-6bd3-42b2-87fe-1a667b98a2f0	552937da-6395-429d-acc0-0f27f93f5825	Document	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-13 02:00:54.725343+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935246+00	2026-06-24 04:00:54.935246+00
03348921-631e-4858-8b0c-0fecbb732030	f9959a07-0d0e-4440-8d38-baec9cfcaa57	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-27 04:00:54.725291+00	2026-05-28 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932585+00	2026-06-24 04:00:54.932585+00
0343359d-5dd9-44bd-9a26-2820337e075b	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Selected	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-14 19:00:54.72533+00	2026-06-15 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934281+00	2026-06-24 04:00:54.934281+00
04026308-0226-4c67-ab29-5a051e93aec9	8af81188-5b54-4a11-9e79-7f4beba87710	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-10 22:00:54.725279+00	2026-06-11 22:00:54.725279+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930336+00	2026-06-24 04:00:54.930336+00
04160636-f51f-4374-889d-a66418e11499	e73dd440-22d0-406f-93c9-993c2682ebc0	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-30 23:00:54.725323+00	2026-05-31 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933696+00	2026-06-24 04:00:54.933696+00
0451ca91-e524-49db-9ff0-0e2666e2514a	e73dd440-22d0-406f-93c9-993c2682ebc0	Selected	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-06 23:00:54.725323+00	2026-06-07 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933699+00	2026-06-24 04:00:54.933699+00
04c20d90-763b-42c6-a63a-53d7a71e3b63	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-20 14:00:54.725333+00	2026-05-21 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93458+00	2026-06-24 04:00:54.93458+00
05532649-1ecd-47a5-ae18-3fd087b75d7d	e73dd440-22d0-406f-93c9-993c2682ebc0	SignContract	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-07 23:00:54.725323+00	2026-06-08 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933699+00	2026-06-24 04:00:54.933699+00
095da141-b39e-4b03-b5f7-d9db1ba3fd6e	f9959a07-0d0e-4440-8d38-baec9cfcaa57	VisaApproved	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-02 04:00:54.725291+00	2026-06-03 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932586+00	2026-06-24 04:00:54.932586+00
0b7b3db7-7ed9-4080-87c9-034df7a26d5e	5b0d3934-7391-406b-aefc-a5e295d7dcbc	SignContract	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-15 19:00:54.72533+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934281+00	2026-06-24 04:00:54.934282+00
0c136e8c-2a37-4997-8fe7-f177baad02f6	18114c8e-7e29-4df4-a188-f10c7a0f2075	BookFlight	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-17 00:00:54.724766+00	2026-06-18 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809378+00	2026-06-24 04:00:54.809378+00
0e68f682-4704-4ac7-8c8e-0b4b9525b9f1	18114c8e-7e29-4df4-a188-f10c7a0f2075	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-05 00:00:54.724766+00	2026-06-06 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809365+00	2026-06-24 04:00:54.809365+00
0e9c0bfc-57d7-4945-afd5-ce7da6157a5b	70c57d40-2774-4c21-8b93-470282c8e678	Orientation	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 05:00:54.725339+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935081+00	2026-06-24 04:00:54.935081+00
0fe40995-4d01-4b86-ada8-14fab56f7392	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 19:00:54.72533+00	2026-06-09 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93428+00	2026-06-24 04:00:54.93428+00
134863a5-950b-4901-9227-71e2d1c92ad2	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	VisaApproved	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-28 14:00:54.725333+00	2026-05-29 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934582+00	2026-06-24 04:00:54.934582+00
13b6400c-4a3f-48a0-a471-d065719adc26	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-25 04:00:54.725291+00	2026-05-26 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932584+00	2026-06-24 04:00:54.932584+00
18c72c52-285a-4dfe-81b0-74577f77404b	f8c53e18-2f5a-456d-944c-e52e65275e60	Deposit	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-06 03:00:54.725287+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932429+00	2026-06-24 04:00:54.932429+00
1e863f3c-c39d-4f53-90de-055412cc37be	18114c8e-7e29-4df4-a188-f10c7a0f2075	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-04 00:00:54.724766+00	2026-06-05 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809163+00	2026-06-24 04:00:54.809163+00
2053fb79-d480-41cb-8e8b-c12546ee68eb	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	EntranceExam	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-24 14:00:54.725333+00	2026-05-25 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934581+00	2026-06-24 04:00:54.934581+00
249fcce7-8805-4326-adea-e2f79bdbb40c	18114c8e-7e29-4df4-a188-f10c7a0f2075	Selected	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-12 00:00:54.724766+00	2026-06-13 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809369+00	2026-06-24 04:00:54.809369+00
25005099-1746-4b02-b533-b74495a5a280	e73dd440-22d0-406f-93c9-993c2682ebc0	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-01 23:00:54.725323+00	2026-06-02 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933697+00	2026-06-24 04:00:54.933697+00
26f8a92b-c064-41ff-ac96-5ad0c7c388d6	18114c8e-7e29-4df4-a188-f10c7a0f2075	Departure	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-18 00:00:54.724766+00	2026-06-19 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809379+00	2026-06-24 04:00:54.809379+00
2eef170b-2380-41d4-9b98-739ed7bf704e	18114c8e-7e29-4df4-a188-f10c7a0f2075	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-06 00:00:54.724766+00	2026-06-07 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809367+00	2026-06-24 04:00:54.809367+00
35998082-c1d8-4c2d-a0f7-8685ae96e0a0	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-10 16:00:54.725326+00	2026-06-11 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934057+00	2026-06-24 04:00:54.934057+00
381b23a2-d2ab-4a78-a191-f08ac3788a51	4aeb662c-30f8-4997-a005-0fcb7f25ada7	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-18 10:00:54.725319+00	2026-06-19 10:00:54.725319+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933491+00	2026-06-24 04:00:54.933491+00
3a7baa3f-2829-4879-98cf-e179aa7537dc	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-23 14:00:54.725333+00	2026-05-24 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93458+00	2026-06-24 04:00:54.93458+00
3c0909c2-57fc-4724-9b89-f8037ce67bcc	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-07 19:00:54.72533+00	2026-06-08 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93428+00	2026-06-24 04:00:54.93428+00
3ce1ff3b-de76-436a-939a-11036f509abd	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-11 16:00:54.725326+00	2026-06-12 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934057+00	2026-06-24 04:00:54.934057+00
3f34f96b-de87-4f71-9d6a-7cf8c2878f27	e73dd440-22d0-406f-93c9-993c2682ebc0	VisaSubmit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 23:00:54.725323+00	2026-06-09 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933699+00	2026-06-24 04:00:54.933699+00
3f49ff6c-717c-44fb-904c-a49af5b800a5	e73dd440-22d0-406f-93c9-993c2682ebc0	VisaApproved	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-09 23:00:54.725323+00	2026-06-10 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.9337+00	2026-06-24 04:00:54.9337+00
3feaf659-18b6-4195-913b-9327de5a3f93	210aad29-1ee0-428e-9fac-2c02bbe4008c	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-30 01:00:54.725336+00	2026-05-31 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934872+00	2026-06-24 04:00:54.934872+00
416fb892-e7ee-4fe2-b0fc-1d508dd7ccf7	210aad29-1ee0-428e-9fac-2c02bbe4008c	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-28 01:00:54.725336+00	2026-05-29 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934872+00	2026-06-24 04:00:54.934872+00
41d3c73a-ec58-4096-a46c-1013752c1e43	70c57d40-2774-4c21-8b93-470282c8e678	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-03 05:00:54.725339+00	2026-06-04 05:00:54.725339+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93508+00	2026-06-24 04:00:54.93508+00
426a1af2-fae6-41ed-81d1-c37bbcba5711	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-07 16:00:54.725326+00	2026-06-08 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934054+00	2026-06-24 04:00:54.934054+00
47116ead-c111-4d61-87b4-3c6085fcb108	f9959a07-0d0e-4440-8d38-baec9cfcaa57	VisaSubmit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-01 04:00:54.725291+00	2026-06-02 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932586+00	2026-06-24 04:00:54.932586+00
476a4e68-16d3-4278-917b-65f5fdaa9866	f9959a07-0d0e-4440-8d38-baec9cfcaa57	FullPayment	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-03 04:00:54.725291+00	2026-06-04 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932586+00	2026-06-24 04:00:54.932586+00
4accca1b-1671-4f12-b650-5ff201a21373	18114c8e-7e29-4df4-a188-f10c7a0f2075	VisaSubmit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-14 00:00:54.724766+00	2026-06-15 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.80937+00	2026-06-24 04:00:54.80937+00
4d08b7ec-ff7c-473a-aee6-afe7dfb623b1	8af81188-5b54-4a11-9e79-7f4beba87710	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-11 22:00:54.725279+00	2026-06-12 22:00:54.725279+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930336+00	2026-06-24 04:00:54.930336+00
4e64938e-6143-40c7-a663-e442128288a9	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-21 14:00:54.725333+00	2026-05-22 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93458+00	2026-06-24 04:00:54.93458+00
500c5d1a-972b-464c-b2f5-70d1df67ad03	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-22 04:00:54.725291+00	2026-05-23 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932583+00	2026-06-24 04:00:54.932583+00
5087aa0b-0b3e-4c6a-9f2e-16847d2eb514	70c57d40-2774-4c21-8b93-470282c8e678	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-07 05:00:54.725339+00	2026-06-08 05:00:54.725339+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935081+00	2026-06-24 04:00:54.935081+00
50a77118-82a1-4ea0-ac06-a972727740ce	18114c8e-7e29-4df4-a188-f10c7a0f2075	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-07 00:00:54.724766+00	2026-06-08 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809367+00	2026-06-24 04:00:54.809367+00
5397f040-98bd-460e-9e2e-e9b57ed0ffe9	210aad29-1ee0-428e-9fac-2c02bbe4008c	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-25 01:00:54.725336+00	2026-05-26 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934871+00	2026-06-24 04:00:54.934871+00
582dcade-8bcc-4616-9e7f-5c280972e864	552937da-6395-429d-acc0-0f27f93f5825	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-09 02:00:54.725343+00	2026-06-10 02:00:54.725343+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935243+00	2026-06-24 04:00:54.935243+00
58d1d1a2-016d-45de-b71f-faa1f77fa058	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	EntranceExam	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-14 16:00:54.725326+00	2026-06-15 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934058+00	2026-06-24 04:00:54.934058+00
58f68534-84dd-4225-826d-91c156320287	e73dd440-22d0-406f-93c9-993c2682ebc0	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-29 23:00:54.725323+00	2026-05-30 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933695+00	2026-06-24 04:00:54.933695+00
590f9232-2b4f-4e79-92a6-943033606000	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-12 19:00:54.72533+00	2026-06-13 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934281+00	2026-06-24 04:00:54.934281+00
5e37251c-a68e-4dfe-a664-f156c85fbff1	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-13 16:00:54.725326+00	2026-06-14 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934058+00	2026-06-24 04:00:54.934058+00
606d6d8b-6454-4b28-873c-7fedde09b81e	18114c8e-7e29-4df4-a188-f10c7a0f2075	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-10 00:00:54.724766+00	2026-06-11 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809369+00	2026-06-24 04:00:54.809369+00
65c6d9a3-2e85-4c8c-ad1a-0e3d735994a0	8af81188-5b54-4a11-9e79-7f4beba87710	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-13 22:00:54.725279+00	2026-06-14 22:00:54.725279+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930336+00	2026-06-24 04:00:54.930336+00
67157c92-c328-4f33-8013-8e82949125a8	18114c8e-7e29-4df4-a188-f10c7a0f2075	SignContract	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-13 00:00:54.724766+00	2026-06-14 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809369+00	2026-06-24 04:00:54.809369+00
67d60805-39b9-4335-b3f4-12f6d5e6c897	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Selected	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-15 16:00:54.725326+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934058+00	2026-06-24 04:00:54.934058+00
68d1be92-2796-4f45-b7e6-97f1d3f07a70	210aad29-1ee0-428e-9fac-2c02bbe4008c	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-29 01:00:54.725336+00	2026-05-30 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934872+00	2026-06-24 04:00:54.934872+00
6b220a38-91ed-490b-bd6f-bb1e2e3c9b82	8af81188-5b54-4a11-9e79-7f4beba87710	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-14 22:00:54.725279+00	2026-06-15 22:00:54.725279+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930337+00	2026-06-24 04:00:54.930337+00
6d448a39-e132-4391-9e88-04e5ef83124a	5b0d3934-7391-406b-aefc-a5e295d7dcbc	EntranceExam	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-13 19:00:54.72533+00	2026-06-14 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934281+00	2026-06-24 04:00:54.934281+00
710d2577-5cdd-481d-814f-23c7c778ff50	f9959a07-0d0e-4440-8d38-baec9cfcaa57	BookFlight	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-04 04:00:54.725291+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932586+00	2026-06-24 04:00:54.932586+00
7484fb18-3904-43b4-893d-9a29f59b254d	8af81188-5b54-4a11-9e79-7f4beba87710	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-12 22:00:54.725279+00	2026-06-13 22:00:54.725279+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930336+00	2026-06-24 04:00:54.930336+00
7529c1d3-0745-42c3-aa48-d0050215dab6	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	VisaSubmit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-27 14:00:54.725333+00	2026-05-28 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934581+00	2026-06-24 04:00:54.934581+00
77adb2ee-b69a-4c9f-9042-96e5dc670bf6	e73dd440-22d0-406f-93c9-993c2682ebc0	EntranceExam	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-05 23:00:54.725323+00	2026-06-06 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933699+00	2026-06-24 04:00:54.933699+00
85fa574b-86ba-42f7-91cb-b2892b42935e	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-12 16:00:54.725326+00	2026-06-13 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934058+00	2026-06-24 04:00:54.934058+00
8933fb1a-2d0d-4c63-a230-1419caf1e75f	210aad29-1ee0-428e-9fac-2c02bbe4008c	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-26 01:00:54.725336+00	2026-05-27 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934871+00	2026-06-24 04:00:54.934871+00
8ab45757-75cb-4967-a0b1-4334521d7570	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-24 04:00:54.725291+00	2026-05-25 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932584+00	2026-06-24 04:00:54.932584+00
8ad1e676-787c-492d-a5cf-e3069f4d6849	4aeb662c-30f8-4997-a005-0fcb7f25ada7	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-15 10:00:54.725319+00	2026-06-16 10:00:54.725319+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933489+00	2026-06-24 04:00:54.933489+00
8d5eec6c-4395-4a31-a5a5-ca0a8d5ab6db	5b0d3934-7391-406b-aefc-a5e295d7dcbc	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-11 19:00:54.72533+00	2026-06-12 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93428+00	2026-06-24 04:00:54.934281+00
8f7dcd94-7f9b-44f6-85f1-631f13524266	18114c8e-7e29-4df4-a188-f10c7a0f2075	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-09 00:00:54.724766+00	2026-06-10 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809369+00	2026-06-24 04:00:54.809369+00
8ff95a6e-648d-4371-a3eb-f5f89dc6ce9a	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Selected	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-25 14:00:54.725333+00	2026-05-26 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934581+00	2026-06-24 04:00:54.934581+00
91b453c1-869c-4a90-b8d8-c4bba13c6873	e73dd440-22d0-406f-93c9-993c2682ebc0	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-31 23:00:54.725323+00	2026-06-01 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933696+00	2026-06-24 04:00:54.933697+00
92fcb204-4968-41e7-bbeb-6a3de6dcca8c	18114c8e-7e29-4df4-a188-f10c7a0f2075	Arrived	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-19 00:00:54.724766+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809379+00	2026-06-24 04:00:54.809379+00
957227f8-7dfc-4933-baba-9740ce60f240	552937da-6395-429d-acc0-0f27f93f5825	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-11 02:00:54.725343+00	2026-06-12 02:00:54.725343+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935244+00	2026-06-24 04:00:54.935244+00
958c07bf-4db8-4b92-9fcb-456a79ca3c9a	e73dd440-22d0-406f-93c9-993c2682ebc0	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-02 23:00:54.725323+00	2026-06-03 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933698+00	2026-06-24 04:00:54.933698+00
9b53ce85-b19e-43fa-a90a-af7341cf751e	f9959a07-0d0e-4440-8d38-baec9cfcaa57	EntranceExam	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-29 04:00:54.725291+00	2026-05-30 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932585+00	2026-06-24 04:00:54.932585+00
9bf16951-ae06-46d0-b015-717a19f14b46	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-17 14:00:54.725333+00	2026-05-18 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934578+00	2026-06-24 04:00:54.934578+00
9cfa8a91-a203-454e-9cec-1c45b99d5557	552937da-6395-429d-acc0-0f27f93f5825	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-10 02:00:54.725343+00	2026-06-11 02:00:54.725343+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935244+00	2026-06-24 04:00:54.935244+00
9d2a984b-a894-4c10-b7de-f09637f340bf	18114c8e-7e29-4df4-a188-f10c7a0f2075	EntranceExam	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-11 00:00:54.724766+00	2026-06-12 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809369+00	2026-06-24 04:00:54.809369+00
9db59915-47b6-416d-b69c-913665f69ae0	210aad29-1ee0-428e-9fac-2c02bbe4008c	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-27 01:00:54.725336+00	2026-05-28 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934872+00	2026-06-24 04:00:54.934872+00
a1109ccc-0a29-4693-93ec-ad02fd40f860	18114c8e-7e29-4df4-a188-f10c7a0f2075	FullPayment	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-16 00:00:54.724766+00	2026-06-17 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809378+00	2026-06-24 04:00:54.809378+00
a97aa2e2-8396-4c01-9536-bf77e01139de	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-28 04:00:54.725291+00	2026-05-29 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932585+00	2026-06-24 04:00:54.932585+00
abca4aae-7f3d-485e-81bf-ce74968c7345	e73dd440-22d0-406f-93c9-993c2682ebc0	Orientation	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-04 23:00:54.725323+00	2026-06-05 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933699+00	2026-06-24 04:00:54.933699+00
ad693402-8341-4f60-8041-eb7726ac2dc7	70c57d40-2774-4c21-8b93-470282c8e678	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-02 05:00:54.725339+00	2026-06-03 05:00:54.725339+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93508+00	2026-06-24 04:00:54.93508+00
b161b1c4-a995-46fe-bf01-220a2010431e	e73dd440-22d0-406f-93c9-993c2682ebc0	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-03 23:00:54.725323+00	2026-06-04 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933698+00	2026-06-24 04:00:54.933698+00
b3c6ab64-53dc-4386-9fac-c0c428b35d6e	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Selected	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-30 04:00:54.725291+00	2026-05-31 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932585+00	2026-06-24 04:00:54.932585+00
b85b20f6-9277-4c31-9eb4-47988a9d4ab7	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-23 04:00:54.725291+00	2026-05-24 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932584+00	2026-06-24 04:00:54.932584+00
c148533b-a521-4589-8d6c-19933d92cdb4	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-09 16:00:54.725326+00	2026-06-10 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934056+00	2026-06-24 04:00:54.934056+00
c2590e05-6bfd-4793-8cc9-6a06a04f0596	552937da-6395-429d-acc0-0f27f93f5825	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-12 02:00:54.725343+00	2026-06-13 02:00:54.725343+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935245+00	2026-06-24 04:00:54.935245+00
c3d7f0e7-5010-48ce-a250-6aa62c209db2	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-09 19:00:54.72533+00	2026-06-10 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93428+00	2026-06-24 04:00:54.93428+00
c56fba1d-329e-4584-ae50-1963253bf5e4	f9959a07-0d0e-4440-8d38-baec9cfcaa57	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-26 04:00:54.725291+00	2026-05-27 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932584+00	2026-06-24 04:00:54.932584+00
c587e742-d7d5-4b25-96c5-b25e7370f1b0	f8c53e18-2f5a-456d-944c-e52e65275e60	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-03 03:00:54.725287+00	2026-06-04 03:00:54.725287+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932427+00	2026-06-24 04:00:54.932427+00
c78105f2-750e-4a57-8992-08aaba3595bd	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-19 14:00:54.725333+00	2026-05-20 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934579+00	2026-06-24 04:00:54.93458+00
ca81dd49-6942-437d-8879-f701b4722746	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	HealthCheck	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-22 14:00:54.725333+00	2026-05-23 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93458+00	2026-06-24 04:00:54.93458+00
cc6b7acc-75d7-4212-93ee-852c59beb238	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-10 19:00:54.72533+00	2026-06-11 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93428+00	2026-06-24 04:00:54.93428+00
cd6f4cf7-3fb7-4dce-9797-95ff5a23d30f	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	FullPayment	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-29 14:00:54.725333+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934582+00	2026-06-24 04:00:54.934582+00
cfc1ba70-bde3-4274-8b84-d3e6e7f64684	77a0ef1f-9ceb-43ae-a009-c5ce19a851da	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 16:00:54.725326+00	2026-06-09 16:00:54.725326+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934055+00	2026-06-24 04:00:54.934055+00
d06b6e3f-f6ae-43b4-bb98-eca17d98466c	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	SignContract	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-26 14:00:54.725333+00	2026-05-27 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934581+00	2026-06-24 04:00:54.934581+00
d4fd0bb2-9e36-44f9-85fa-81007d7f823f	210aad29-1ee0-428e-9fac-2c02bbe4008c	EntranceExam	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-31 01:00:54.725336+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934872+00	2026-06-24 04:00:54.934873+00
d8414528-6784-4862-83e9-c9a81e60b351	4aeb662c-30f8-4997-a005-0fcb7f25ada7	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-17 10:00:54.725319+00	2026-06-18 10:00:54.725319+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933491+00	2026-06-24 04:00:54.933491+00
db9abbd4-16f5-4e9a-b476-774ef40525e2	70c57d40-2774-4c21-8b93-470282c8e678	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-06 05:00:54.725339+00	2026-06-07 05:00:54.725339+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.935081+00	2026-06-24 04:00:54.935081+00
dd501eba-5134-437f-90f6-8f76eb749f63	4aeb662c-30f8-4997-a005-0fcb7f25ada7	Document	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-19 10:00:54.725319+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.933493+00	2026-06-24 04:00:54.933493+00
df150509-1d1b-4cae-9262-7fcdd182cee9	e73dd440-22d0-406f-93c9-993c2682ebc0	FullPayment	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-10 23:00:54.725323+00	2026-06-11 23:00:54.725323+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.9337+00	2026-06-24 04:00:54.9337+00
e0b96f56-db7e-42ee-9d1a-80fc93e31b18	8af81188-5b54-4a11-9e79-7f4beba87710	Orientation	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-15 22:00:54.725279+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930337+00	2026-06-24 04:00:54.930337+00
e48d8ee2-9f92-48fc-a34d-ad500455a131	e73dd440-22d0-406f-93c9-993c2682ebc0	BookFlight	InProgress	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-11 23:00:54.725323+00	\N	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.9337+00	2026-06-24 04:00:54.9337+00
e4b70dda-81bc-449e-8b61-2e843c3e1d35	89ed62d0-bd57-40d4-a0f4-d7cb2bf07ac6	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-18 14:00:54.725333+00	2026-05-19 14:00:54.725333+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934579+00	2026-06-24 04:00:54.934579+00
e55c6818-3540-4f65-82b2-8f444ef7cba0	18114c8e-7e29-4df4-a188-f10c7a0f2075	VisaApproved	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-15 00:00:54.724766+00	2026-06-16 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809378+00	2026-06-24 04:00:54.809378+00
e581d487-0301-4c01-bd50-76a15647ed1e	18114c8e-7e29-4df4-a188-f10c7a0f2075	Document	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-08 00:00:54.724766+00	2026-06-09 00:00:54.724766+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.809368+00	2026-06-24 04:00:54.809368+00
e6aba9e1-4e14-4467-b770-ee197248e239	4aeb662c-30f8-4997-a005-0fcb7f25ada7	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-16 10:00:54.725319+00	2026-06-17 10:00:54.725319+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93349+00	2026-06-24 04:00:54.93349+00
e8e86467-c3ea-425a-bffd-8b57ac7a09e7	5b0d3934-7391-406b-aefc-a5e295d7dcbc	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-06 19:00:54.72533+00	2026-06-07 19:00:54.72533+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.934279+00	2026-06-24 04:00:54.934279+00
f168ddb6-13e2-4b8a-8bfb-e3958c0ec6d0	8af81188-5b54-4a11-9e79-7f4beba87710	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-09 22:00:54.725279+00	2026-06-10 22:00:54.725279+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.930334+00	2026-06-24 04:00:54.930334+00
f3c004bc-1ba5-43c0-8b6e-25881a920e89	70c57d40-2774-4c21-8b93-470282c8e678	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-04 05:00:54.725339+00	2026-06-05 05:00:54.725339+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93508+00	2026-06-24 04:00:54.93508+00
f7851945-7b68-43e8-ab84-11a130fed6e5	70c57d40-2774-4c21-8b93-470282c8e678	Deposit	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-05 05:00:54.725339+00	2026-06-06 05:00:54.725339+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93508+00	2026-06-24 04:00:54.935081+00
f941dec5-98b5-47de-952a-920fa39b7fcb	f9959a07-0d0e-4440-8d38-baec9cfcaa57	SignContract	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-31 04:00:54.725291+00	2026-06-01 04:00:54.725291+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932586+00	2026-06-24 04:00:54.932586+00
fc20595b-efd0-4921-b01b-9cc1e7d57114	f8c53e18-2f5a-456d-944c-e52e65275e60	Consulting	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-04 03:00:54.725287+00	2026-06-05 03:00:54.725287+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932428+00	2026-06-24 04:00:54.932428+00
fca857b1-aac2-4957-b264-0a19dd55aa12	f8c53e18-2f5a-456d-944c-e52e65275e60	Registration	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-05 03:00:54.725287+00	2026-06-06 03:00:54.725287+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.932428+00	2026-06-24 04:00:54.932428+00
fd682aa2-63d6-4f44-bb96-efda5ca05208	210aad29-1ee0-428e-9fac-2c02bbe4008c	Lead	Completed	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-05-24 01:00:54.725336+00	2026-05-25 01:00:54.725336+00	\N	019ef7b8-c87b-705f-aecb-05cfc3c92827	2026-06-24 04:00:54.93487+00	2026-06-24 04:00:54.93487+00
\.


--
-- Name: AspNetRoleClaims_id_seq; Type: SEQUENCE SET; Schema: public; Owner: polymind
--

SELECT pg_catalog.setval('public."AspNetRoleClaims_id_seq"', 1, false);


--
-- Name: AspNetUserClaims_id_seq; Type: SEQUENCE SET; Schema: public; Owner: polymind
--

SELECT pg_catalog.setval('public."AspNetUserClaims_id_seq"', 1, false);


--
-- Name: __EFMigrationsHistory pk___ef_migrations_history; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id);


--
-- Name: agent_commission_configs pk_agent_commission_configs; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.agent_commission_configs
    ADD CONSTRAINT pk_agent_commission_configs PRIMARY KEY (id);


--
-- Name: agent_commissions pk_agent_commissions; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.agent_commissions
    ADD CONSTRAINT pk_agent_commissions PRIMARY KEY (id);


--
-- Name: agents pk_agents; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.agents
    ADD CONSTRAINT pk_agents PRIMARY KEY (id);


--
-- Name: AspNetRoleClaims pk_asp_net_role_claims; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT pk_asp_net_role_claims PRIMARY KEY (id);


--
-- Name: AspNetUserClaims pk_asp_net_user_claims; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT pk_asp_net_user_claims PRIMARY KEY (id);


--
-- Name: AspNetUserLogins pk_asp_net_user_logins; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT pk_asp_net_user_logins PRIMARY KEY (login_provider, provider_key);


--
-- Name: AspNetUserRoles pk_asp_net_user_roles; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT pk_asp_net_user_roles PRIMARY KEY (user_id, role_id);


--
-- Name: AspNetUserTokens pk_asp_net_user_tokens; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT pk_asp_net_user_tokens PRIMARY KEY (user_id, login_provider, name);


--
-- Name: audit_logs pk_audit_logs; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT pk_audit_logs PRIMARY KEY (id);


--
-- Name: candidate_documents pk_candidate_documents; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.candidate_documents
    ADD CONSTRAINT pk_candidate_documents PRIMARY KEY (id);


--
-- Name: candidate_job_orders pk_candidate_job_orders; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.candidate_job_orders
    ADD CONSTRAINT pk_candidate_job_orders PRIMARY KEY (id);


--
-- Name: candidates pk_candidates; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.candidates
    ADD CONSTRAINT pk_candidates PRIMARY KEY (id);


--
-- Name: document_versions pk_document_versions; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.document_versions
    ADD CONSTRAINT pk_document_versions PRIMARY KEY (id);


--
-- Name: expenses pk_expenses; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.expenses
    ADD CONSTRAINT pk_expenses PRIMARY KEY (id);


--
-- Name: flights pk_flights; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.flights
    ADD CONSTRAINT pk_flights PRIMARY KEY (id);


--
-- Name: job_orders pk_job_orders; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.job_orders
    ADD CONSTRAINT pk_job_orders PRIMARY KEY (id);


--
-- Name: lead_activities pk_lead_activities; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.lead_activities
    ADD CONSTRAINT pk_lead_activities PRIMARY KEY (id);


--
-- Name: leads pk_leads; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.leads
    ADD CONSTRAINT pk_leads PRIMARY KEY (id);


--
-- Name: notifications pk_notifications; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT pk_notifications PRIMARY KEY (id);


--
-- Name: payments pk_payments; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT pk_payments PRIMARY KEY (id);


--
-- Name: permissions pk_permissions; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT pk_permissions PRIMARY KEY (id);


--
-- Name: receipts pk_receipts; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.receipts
    ADD CONSTRAINT pk_receipts PRIMARY KEY (id);


--
-- Name: role_permissions pk_role_permissions; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.role_permissions
    ADD CONSTRAINT pk_role_permissions PRIMARY KEY (role_id, permission_id);


--
-- Name: roles pk_roles; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT pk_roles PRIMARY KEY (id);


--
-- Name: users pk_users; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT pk_users PRIMARY KEY (id);


--
-- Name: visas pk_visas; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.visas
    ADD CONSTRAINT pk_visas PRIMARY KEY (id);


--
-- Name: workflow_step_records pk_workflow_step_records; Type: CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.workflow_step_records
    ADD CONSTRAINT pk_workflow_step_records PRIMARY KEY (id);


--
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX "EmailIndex" ON public.users USING btree (normalized_email);


--
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public.roles USING btree (normalized_name);


--
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX "UserNameIndex" ON public.users USING btree (normalized_user_name);


--
-- Name: ix_agent_commission_configs_agent_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_agent_commission_configs_agent_id ON public.agent_commission_configs USING btree (agent_id);


--
-- Name: ix_agent_commissions_agent_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_agent_commissions_agent_id ON public.agent_commissions USING btree (agent_id);


--
-- Name: ix_agent_commissions_status; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_agent_commissions_status ON public.agent_commissions USING btree (status);


--
-- Name: ix_agents_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_agents_code ON public.agents USING btree (code);


--
-- Name: ix_asp_net_role_claims_role_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_asp_net_role_claims_role_id ON public."AspNetRoleClaims" USING btree (role_id);


--
-- Name: ix_asp_net_user_claims_user_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_asp_net_user_claims_user_id ON public."AspNetUserClaims" USING btree (user_id);


--
-- Name: ix_asp_net_user_logins_user_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_asp_net_user_logins_user_id ON public."AspNetUserLogins" USING btree (user_id);


--
-- Name: ix_asp_net_user_roles_role_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_asp_net_user_roles_role_id ON public."AspNetUserRoles" USING btree (role_id);


--
-- Name: ix_audit_logs_resource_resource_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_audit_logs_resource_resource_id ON public.audit_logs USING btree (resource, resource_id);


--
-- Name: ix_audit_logs_user_id_created_at; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_audit_logs_user_id_created_at ON public.audit_logs USING btree (user_id, created_at);


--
-- Name: ix_candidate_documents_candidate_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_candidate_documents_candidate_id ON public.candidate_documents USING btree (candidate_id);


--
-- Name: ix_candidate_job_orders_candidate_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_candidate_job_orders_candidate_id ON public.candidate_job_orders USING btree (candidate_id);


--
-- Name: ix_candidate_job_orders_job_order_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_candidate_job_orders_job_order_id ON public.candidate_job_orders USING btree (job_order_id);


--
-- Name: ix_candidates_cccd_number; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_candidates_cccd_number ON public.candidates USING btree (cccd_number);


--
-- Name: ix_candidates_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_candidates_code ON public.candidates USING btree (code);


--
-- Name: ix_candidates_passport_number; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_candidates_passport_number ON public.candidates USING btree (passport_number);


--
-- Name: ix_candidates_phone; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_candidates_phone ON public.candidates USING btree (phone);


--
-- Name: ix_document_versions_document_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_document_versions_document_id ON public.document_versions USING btree (document_id);


--
-- Name: ix_expenses_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_expenses_code ON public.expenses USING btree (code);


--
-- Name: ix_job_orders_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_job_orders_code ON public.job_orders USING btree (code);


--
-- Name: ix_lead_activities_lead_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_lead_activities_lead_id ON public.lead_activities USING btree (lead_id);


--
-- Name: ix_leads_assigned_to; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_leads_assigned_to ON public.leads USING btree (assigned_to);


--
-- Name: ix_leads_cccd; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_leads_cccd ON public.leads USING btree (cccd);


--
-- Name: ix_leads_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_leads_code ON public.leads USING btree (code);


--
-- Name: ix_leads_created_at; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_leads_created_at ON public.leads USING btree (created_at);


--
-- Name: ix_leads_phone; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_leads_phone ON public.leads USING btree (phone);


--
-- Name: ix_leads_source; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_leads_source ON public.leads USING btree (source);


--
-- Name: ix_leads_status; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_leads_status ON public.leads USING btree (status);


--
-- Name: ix_payments_candidate_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_payments_candidate_id ON public.payments USING btree (candidate_id);


--
-- Name: ix_payments_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_payments_code ON public.payments USING btree (code);


--
-- Name: ix_payments_status; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_payments_status ON public.payments USING btree (status);


--
-- Name: ix_permissions_name; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_permissions_name ON public.permissions USING btree (name);


--
-- Name: ix_receipts_code; Type: INDEX; Schema: public; Owner: polymind
--

CREATE UNIQUE INDEX ix_receipts_code ON public.receipts USING btree (code);


--
-- Name: ix_role_permissions_permission_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_role_permissions_permission_id ON public.role_permissions USING btree (permission_id);


--
-- Name: ix_workflow_step_records_candidate_job_order_id; Type: INDEX; Schema: public; Owner: polymind
--

CREATE INDEX ix_workflow_step_records_candidate_job_order_id ON public.workflow_step_records USING btree (candidate_job_order_id);


--
-- Name: agent_commission_configs fk_agent_commission_configs_agents_agent_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.agent_commission_configs
    ADD CONSTRAINT fk_agent_commission_configs_agents_agent_id FOREIGN KEY (agent_id) REFERENCES public.agents(id) ON DELETE CASCADE;


--
-- Name: agent_commissions fk_agent_commissions_agents_agent_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.agent_commissions
    ADD CONSTRAINT fk_agent_commissions_agents_agent_id FOREIGN KEY (agent_id) REFERENCES public.agents(id) ON DELETE CASCADE;


--
-- Name: AspNetRoleClaims fk_asp_net_role_claims_asp_net_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT fk_asp_net_role_claims_asp_net_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: AspNetUserClaims fk_asp_net_user_claims_asp_net_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT fk_asp_net_user_claims_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: AspNetUserLogins fk_asp_net_user_logins_asp_net_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT fk_asp_net_user_logins_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: AspNetUserRoles fk_asp_net_user_roles_asp_net_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT fk_asp_net_user_roles_asp_net_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: AspNetUserRoles fk_asp_net_user_roles_asp_net_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT fk_asp_net_user_roles_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: AspNetUserTokens fk_asp_net_user_tokens_asp_net_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT fk_asp_net_user_tokens_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: candidate_documents fk_candidate_documents_candidates_candidate_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.candidate_documents
    ADD CONSTRAINT fk_candidate_documents_candidates_candidate_id FOREIGN KEY (candidate_id) REFERENCES public.candidates(id) ON DELETE CASCADE;


--
-- Name: candidate_job_orders fk_candidate_job_orders_candidates_candidate_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.candidate_job_orders
    ADD CONSTRAINT fk_candidate_job_orders_candidates_candidate_id FOREIGN KEY (candidate_id) REFERENCES public.candidates(id) ON DELETE CASCADE;


--
-- Name: candidate_job_orders fk_candidate_job_orders_job_orders_job_order_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.candidate_job_orders
    ADD CONSTRAINT fk_candidate_job_orders_job_orders_job_order_id FOREIGN KEY (job_order_id) REFERENCES public.job_orders(id) ON DELETE CASCADE;


--
-- Name: document_versions fk_document_versions_candidate_documents_document_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.document_versions
    ADD CONSTRAINT fk_document_versions_candidate_documents_document_id FOREIGN KEY (document_id) REFERENCES public.candidate_documents(id) ON DELETE CASCADE;


--
-- Name: lead_activities fk_lead_activities_leads_lead_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.lead_activities
    ADD CONSTRAINT fk_lead_activities_leads_lead_id FOREIGN KEY (lead_id) REFERENCES public.leads(id) ON DELETE CASCADE;


--
-- Name: role_permissions fk_role_permissions_permissions_permission_id; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.role_permissions
    ADD CONSTRAINT fk_role_permissions_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES public.permissions(id) ON DELETE CASCADE;


--
-- Name: workflow_step_records fk_workflow_step_records_candidate_job_orders_candidate_job_or; Type: FK CONSTRAINT; Schema: public; Owner: polymind
--

ALTER TABLE ONLY public.workflow_step_records
    ADD CONSTRAINT fk_workflow_step_records_candidate_job_orders_candidate_job_or FOREIGN KEY (candidate_job_order_id) REFERENCES public.candidate_job_orders(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict bMYYKo0WspwcUr4paWlkPYRkOLGMH2khNnqNOXtIle94Ih83bPKHlgCQeRGi2Ij

