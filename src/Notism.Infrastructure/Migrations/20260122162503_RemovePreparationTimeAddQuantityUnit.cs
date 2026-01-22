using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreparationTimeAddQuantityUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'CreatedBy') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""CreatedBy"" uuid;
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'UpdatedBy') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""UpdatedBy"" uuid;
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RefreshTokens' AND column_name = 'CreatedBy') THEN
                        ALTER TABLE ""RefreshTokens"" ADD COLUMN ""CreatedBy"" uuid;
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RefreshTokens' AND column_name = 'UpdatedBy') THEN
                        ALTER TABLE ""RefreshTokens"" ADD COLUMN ""UpdatedBy"" uuid;
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PasswordResetTokens' AND column_name = 'CreatedBy') THEN
                        ALTER TABLE ""PasswordResetTokens"" ADD COLUMN ""CreatedBy"" uuid;
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PasswordResetTokens' AND column_name = 'UpdatedBy') THEN
                        ALTER TABLE ""PasswordResetTokens"" ADD COLUMN ""UpdatedBy"" uuid;
                    END IF;
                END $$;");

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FileKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    QuantityUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiscountPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Foods_Category",
                table: "Foods",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_Category_IsAvailable",
                table: "Foods",
                columns: new[] { "Category", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_Foods_IsAvailable",
                table: "Foods",
                column: "IsAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PasswordResetTokens");
        }
    }
}
