using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodNameDescriptionTrigramIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // pg_trgm powers the GIN trigram indexes used by the case-insensitive substring food search.
            migrationBuilder.Sql(
                "CREATE EXTENSION IF NOT EXISTS pg_trgm;",
                suppressTransaction: true);

            // The food search filters on LOWER(col) LIKE '%kw%', so the trigram indexes must be built
            // on the LOWER(...) expression for the planner to use them (a plain-column index is ignored).
            // The Foods table is populated, so build CONCURRENTLY to avoid a write lock; CONCURRENTLY
            // cannot run inside a transaction, hence suppressTransaction: true.
            migrationBuilder.Sql(
                "CREATE INDEX CONCURRENTLY IF NOT EXISTS \"IX_Foods_Name\" ON \"Foods\" USING gin (LOWER(\"Name\") gin_trgm_ops);",
                suppressTransaction: true);

            migrationBuilder.Sql(
                "CREATE INDEX CONCURRENTLY IF NOT EXISTS \"IX_Foods_Description\" ON \"Foods\" USING gin (LOWER(\"Description\") gin_trgm_ops);",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS \"IX_Foods_Description\";",
                suppressTransaction: true);

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS \"IX_Foods_Name\";",
                suppressTransaction: true);

            // Leave the pg_trgm extension in place; dropping a shared extension on rollback is unsafe.
        }
    }
}
