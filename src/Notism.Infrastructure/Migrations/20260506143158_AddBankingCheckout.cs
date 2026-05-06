using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBankingCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankingCheckouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CartItemIds = table.Column<string>(type: "jsonb", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingCheckouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankingCheckouts_IsUsed",
                table: "BankingCheckouts",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_BankingCheckouts_UserId",
                table: "BankingCheckouts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankingCheckouts");
        }
    }
}
