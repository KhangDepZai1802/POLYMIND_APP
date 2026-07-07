using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingAndLoanRepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "deduction_start_date",
                table: "loans",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "kind",
                table: "loans",
                type: "text",
                nullable: false,
                defaultValue: "Bank");

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_deduction_amount",
                table: "loans",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "loan_repayments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_no = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    paid_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loan_repayments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_evaluations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track = table.Column<string>(type: "text", nullable: true),
                    evaluation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    attendance = table.Column<string>(type: "text", nullable: false),
                    professional = table.Column<string>(type: "text", nullable: false),
                    discipline = table.Column<string>(type: "text", nullable: false),
                    financial = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    attachments_json = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_evaluations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track = table.Column<string>(type: "text", nullable: false),
                    is_enrolled = table.Column<bool>(type: "boolean", nullable: false),
                    level_label = table.Column<string>(type: "text", nullable: true),
                    progress_percent = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_loan_repayments_loan_id",
                table: "loan_repayments",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "ix_loan_repayments_status",
                table: "loan_repayments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_training_evaluations_candidate_id",
                table: "training_evaluations",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_evaluations_evaluation_date",
                table: "training_evaluations",
                column: "evaluation_date");

            migrationBuilder.CreateIndex(
                name: "ix_training_records_candidate_id_track",
                table: "training_records",
                columns: new[] { "candidate_id", "track" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loan_repayments");

            migrationBuilder.DropTable(
                name: "training_evaluations");

            migrationBuilder.DropTable(
                name: "training_records");

            migrationBuilder.DropColumn(
                name: "deduction_start_date",
                table: "loans");

            migrationBuilder.DropColumn(
                name: "kind",
                table: "loans");

            migrationBuilder.DropColumn(
                name: "monthly_deduction_amount",
                table: "loans");
        }
    }
}
