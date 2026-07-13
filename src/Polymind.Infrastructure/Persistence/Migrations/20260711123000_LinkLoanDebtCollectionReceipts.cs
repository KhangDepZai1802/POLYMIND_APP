using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260711123000_LinkLoanDebtCollectionReceipts")]
public partial class LinkLoanDebtCollectionReceipts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "loan_id",
            table: "receipts",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "loan_repayment_id",
            table: "receipts",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_receipts_loan_id",
            table: "receipts",
            column: "loan_id");

        migrationBuilder.CreateIndex(
            name: "ix_receipts_loan_repayment_id",
            table: "receipts",
            column: "loan_repayment_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_receipts_loan_id",
            table: "receipts");

        migrationBuilder.DropIndex(
            name: "ix_receipts_loan_repayment_id",
            table: "receipts");

        migrationBuilder.DropColumn(
            name: "loan_id",
            table: "receipts");

        migrationBuilder.DropColumn(
            name: "loan_repayment_id",
            table: "receipts");
    }
}
