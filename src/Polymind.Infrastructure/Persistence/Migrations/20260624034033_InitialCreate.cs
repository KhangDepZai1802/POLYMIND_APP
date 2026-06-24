using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    representative_name = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    contract_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    resource = table.Column<string>(type: "text", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "candidates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    cccd_number = table.Column<string>(type: "text", nullable: true),
                    cccd_issue_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cccd_issue_place = table.Column<string>(type: "text", nullable: true),
                    passport_number = table.Column<string>(type: "text", nullable: true),
                    passport_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    province = table.Column<string>(type: "text", nullable: true),
                    marital_status = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_relation = table.Column<string>(type: "text", nullable: true),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    expense_date = table.Column<DateOnly>(type: "date", nullable: false),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline = table.Column<string>(type: "text", nullable: true),
                    ticket_code = table.Column<string>(type: "text", nullable: true),
                    departure_date = table.Column<DateOnly>(type: "date", nullable: true),
                    departure_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    departure_airport = table.Column<string>(type: "text", nullable: true),
                    destination_country = table.Column<string>(type: "text", nullable: true),
                    destination_airport = table.Column<string>(type: "text", nullable: true),
                    actual_departure_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    assigned_to = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flights", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    union_name = table.Column<string>(type: "text", nullable: true),
                    company_name = table.Column<string>(type: "text", nullable: true),
                    field = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    salary_description = table.Column<string>(type: "text", nullable: true),
                    cost_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    recruitment_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expected_departure_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: true),
                    cccd = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    province = table.Column<string>(type: "text", nullable: true),
                    occupation = table.Column<string>(type: "text", nullable: true),
                    education_level = table.Column<string>(type: "text", nullable: true),
                    work_experience = table.Column<string>(type: "text", nullable: true),
                    languages = table.Column<string>(type: "text", nullable: true),
                    target_country = table.Column<string>(type: "text", nullable: true),
                    interested_job_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_to = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: true),
                    channel = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    reference_type = table.Column<string>(type: "text", nullable: true),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    paid_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    payment_method = table.Column<string>(type: "text", nullable: true),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    resource = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    receipt_type = table.Column<string>(type: "text", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    receipt_date = table.Column<DateOnly>(type: "date", nullable: false),
                    pdf_url = table.Column<string>(type: "text", nullable: true),
                    signed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receipts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visa_type = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: false),
                    submitted_date = table.Column<DateOnly>(type: "date", nullable: true),
                    interview_date = table.Column<DateOnly>(type: "date", nullable: true),
                    result_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    handled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agent_commission_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    milestone = table.Column<string>(type: "text", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    fixed_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_commission_configs", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_commission_configs_agents_agent_id",
                        column: x => x.agent_id,
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agent_commissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    config_id = table.Column<Guid>(type: "uuid", nullable: true),
                    milestone = table.Column<string>(type: "text", nullable: false),
                    base_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    paid_date = table.Column<DateOnly>(type: "date", nullable: true),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_commissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_commissions_agents_agent_id",
                        column: x => x.agent_id,
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doc_type = table.Column<string>(type: "text", nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidate_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidate_documents_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_job_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_step = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assigned_to = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidate_job_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidate_job_orders_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_candidate_job_orders_job_orders_job_order_id",
                        column: x => x.job_order_id,
                        principalTable: "job_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lead_activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    old_status = table.Column<string>(type: "text", nullable: true),
                    new_status = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lead_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_lead_activities_leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    file_url = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "text", nullable: true),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_versions_candidate_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "candidate_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_step_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_job_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assigned_to = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_step_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflow_step_records_candidate_job_orders_candidate_job_or",
                        column: x => x.candidate_job_order_id,
                        principalTable: "candidate_job_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agent_commission_configs_agent_id",
                table: "agent_commission_configs",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_commissions_agent_id",
                table: "agent_commissions",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_commissions_status",
                table: "agent_commissions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_agents_code",
                table: "agents",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_resource_resource_id",
                table: "audit_logs",
                columns: new[] { "resource", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id_created_at",
                table: "audit_logs",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_candidate_documents_candidate_id",
                table: "candidate_documents",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_job_orders_candidate_id",
                table: "candidate_job_orders",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_job_orders_job_order_id",
                table: "candidate_job_orders",
                column: "job_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_cccd_number",
                table: "candidates",
                column: "cccd_number");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_code",
                table: "candidates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_candidates_passport_number",
                table: "candidates",
                column: "passport_number");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_phone",
                table: "candidates",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "ix_document_versions_document_id",
                table: "document_versions",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_code",
                table: "expenses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_job_orders_code",
                table: "job_orders",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lead_activities_lead_id",
                table: "lead_activities",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "ix_leads_assigned_to",
                table: "leads",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "ix_leads_cccd",
                table: "leads",
                column: "cccd");

            migrationBuilder.CreateIndex(
                name: "ix_leads_code",
                table: "leads",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_leads_created_at",
                table: "leads",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_leads_phone",
                table: "leads",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "ix_leads_source",
                table: "leads",
                column: "source");

            migrationBuilder.CreateIndex(
                name: "ix_leads_status",
                table: "leads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_payments_candidate_id",
                table: "payments",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_code",
                table: "payments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_status",
                table: "payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_receipts_code",
                table: "receipts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_step_records_candidate_job_order_id",
                table: "workflow_step_records",
                column: "candidate_job_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_commission_configs");

            migrationBuilder.DropTable(
                name: "agent_commissions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "document_versions");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "flights");

            migrationBuilder.DropTable(
                name: "lead_activities");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "receipts");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "visas");

            migrationBuilder.DropTable(
                name: "workflow_step_records");

            migrationBuilder.DropTable(
                name: "agents");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "candidate_documents");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "candidate_job_orders");

            migrationBuilder.DropTable(
                name: "candidates");

            migrationBuilder.DropTable(
                name: "job_orders");
        }
    }
}
