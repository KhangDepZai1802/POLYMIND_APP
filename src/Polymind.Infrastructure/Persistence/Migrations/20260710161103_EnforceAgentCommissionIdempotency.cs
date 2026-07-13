using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polymind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAgentCommissionIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Không tự xóa/gộp dữ liệu tiền. Nếu DB cũ đã có trùng, dừng migration để người vận hành
            // đối soát thủ công trước khi áp unique index.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM agent_commissions
                        GROUP BY agent_id, candidate_id, milestone
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Duplicate agent commissions exist; reconcile them before applying the idempotency index.';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_agent_commissions_agent_id_candidate_id_milestone",
                table: "agent_commissions",
                columns: new[] { "agent_id", "candidate_id", "milestone" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_agent_commissions_agent_id_candidate_id_milestone",
                table: "agent_commissions");
        }
    }
}
