using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobCategoryAndDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "application_deadline",
                table: "job_orders",
                type: "date",
                nullable: true);

            // Job cũ đều là XKLĐ → mặc định nhóm "Việc làm ngoài nước" (chuỗi rỗng sẽ không parse được enum).
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "job_orders",
                type: "text",
                nullable: false,
                defaultValue: "OverseasJob");

            migrationBuilder.AddColumn<DateOnly>(
                name: "posted_date",
                table: "job_orders",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "application_deadline",
                table: "job_orders");

            migrationBuilder.DropColumn(
                name: "category",
                table: "job_orders");

            migrationBuilder.DropColumn(
                name: "posted_date",
                table: "job_orders");
        }
    }
}
