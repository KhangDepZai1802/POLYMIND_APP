using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateGuardianFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "guardian_address",
                table: "candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "guardian_cccd",
                table: "candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "guardian_name",
                table: "candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "guardian_occupation",
                table: "candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "guardian_phone",
                table: "candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "guardian_relation",
                table: "candidates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guardian_address",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "guardian_cccd",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "guardian_name",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "guardian_occupation",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "guardian_phone",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "guardian_relation",
                table: "candidates");
        }
    }
}
