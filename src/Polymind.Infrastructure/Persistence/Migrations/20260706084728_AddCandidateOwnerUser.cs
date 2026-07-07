using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateOwnerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_user_id",
                table: "candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_candidates_owner_user_id",
                table: "candidates",
                column: "owner_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_candidates_owner_user_id",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                table: "candidates");
        }
    }
}
