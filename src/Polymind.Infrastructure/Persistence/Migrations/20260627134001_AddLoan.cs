using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    term_months = table.Column<int>(type: "integer", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    interest_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    disbursed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loans", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_loans_candidate_id",
                table: "loans",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_loans_code",
                table: "loans",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loans_status",
                table: "loans",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loans");
        }
    }
}
