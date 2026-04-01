using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSlugId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column as nullable first
            migrationBuilder.AddColumn<string>(
                name: "SlugId",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Generate slugs for existing orders using a combination of timestamp and ID
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""SlugId"" = 'ORD-' || UPPER(
                    SUBSTRING(
                        MD5(CAST(""Id"" AS TEXT) || CAST(""CreatedAt"" AS TEXT)),
                        1,
                        8
                    )
                )
                WHERE ""SlugId"" IS NULL;
            ");

            // Make column non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "SlugId",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SlugId",
                table: "Orders",
                column: "SlugId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SlugId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SlugId",
                table: "Orders");
        }
    }
}
