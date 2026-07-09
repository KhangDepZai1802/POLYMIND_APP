using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateParentUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_user_id",
                table: "candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_candidates_parent_user_id",
                table: "candidates",
                column: "parent_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_candidates_parent_user_id",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "parent_user_id",
                table: "candidates");
        }
    }
}
