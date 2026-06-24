using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptSourceLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "expense_id",
                table: "receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "payment_id",
                table: "receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_receipts_expense_id",
                table: "receipts",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "ix_receipts_payment_id",
                table: "receipts",
                column: "payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_receipts_expense_id",
                table: "receipts");

            migrationBuilder.DropIndex(
                name: "ix_receipts_payment_id",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "expense_id",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "payment_id",
                table: "receipts");
        }
    }
}
