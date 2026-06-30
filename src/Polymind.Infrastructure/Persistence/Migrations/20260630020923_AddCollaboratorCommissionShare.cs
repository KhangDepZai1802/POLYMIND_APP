using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollaboratorCommissionShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "commission_share_percentage",
                table: "collaborators",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 50m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "commission_share_percentage",
                table: "collaborators");
        }
    }
}
