using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260711170000_SnapshotCollaboratorCommissionShare")]
public partial class SnapshotCollaboratorCommissionShare : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "collaborator_id",
            table: "agent_commissions",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "collaborator_share_percentage",
            table: "agent_commissions",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: true);

        // Dữ liệu cũ được đóng băng theo assignment + tỷ lệ hiện hành tại thời điểm migration.
        // Clamp 30..40 khớp validation nghiệp vụ, tránh giữ default DB cũ 50% nếu có raw insert.
        migrationBuilder.Sql(
            """
            UPDATE agent_commissions AS ac
            SET collaborator_id = c.collaborator_id,
                collaborator_share_percentage = LEAST(40, GREATEST(30, co.commission_share_percentage))
            FROM candidates AS c
            INNER JOIN collaborators AS co ON co.id = c.collaborator_id
            WHERE ac.candidate_id = c.id
              AND ac.agent_id = co.agent_id
              AND c.collaborator_id IS NOT NULL;
            """);

        migrationBuilder.CreateIndex(
            name: "ix_agent_commissions_collaborator_id",
            table: "agent_commissions",
            column: "collaborator_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_agent_commissions_collaborator_id",
            table: "agent_commissions");

        migrationBuilder.DropColumn(
            name: "collaborator_id",
            table: "agent_commissions");

        migrationBuilder.DropColumn(
            name: "collaborator_share_percentage",
            table: "agent_commissions");
    }
}
