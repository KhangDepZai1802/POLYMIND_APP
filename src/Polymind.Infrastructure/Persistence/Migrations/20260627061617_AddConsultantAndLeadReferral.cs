using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultantAndLeadReferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "collaborator_id",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "consultant_id",
                table: "candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_leads_collaborator_id",
                table: "leads",
                column: "collaborator_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_consultant_id",
                table: "candidates",
                column: "consultant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_leads_collaborator_id",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "ix_candidates_consultant_id",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "collaborator_id",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "consultant_id",
                table: "candidates");
        }
    }
}
