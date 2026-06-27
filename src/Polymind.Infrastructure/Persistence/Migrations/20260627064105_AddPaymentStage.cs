using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "stage",
                table: "payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stage",
                table: "payments");
        }
    }
}
