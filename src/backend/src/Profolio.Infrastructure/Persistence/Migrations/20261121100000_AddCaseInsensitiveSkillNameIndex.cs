namespace Profolio.Infrastructure.Persistence.Migrations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Profolio.Infrastructure.Persistence;

public partial class AddCaseInsensitiveSkillNameIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // PostgreSQL columns retain the quoted PascalCase names emitted by InitialCreate.
        // Fail before creating the index when historical data violates the new invariant.
        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM portfolio.skills
                    GROUP BY "TenantId", LOWER("Name")
                    HAVING COUNT(*) > 1
                ) THEN
                    RAISE EXCEPTION 'Cannot create ix_skills_tenant_id_lower_name: case-insensitive duplicate skill names exist.'
                        USING ERRCODE = '23514',
                              HINT = 'Review duplicate rows in portfolio.skills and resolve them explicitly before retrying this migration.';
                END IF;
            END;
            $$;

            CREATE UNIQUE INDEX ix_skills_tenant_id_lower_name
            ON portfolio.skills ("TenantId", LOWER("Name"));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS portfolio.ix_skills_tenant_id_lower_name;");
    }
}
