using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DissolvePaymentDomainRenameBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "StorerId",
                table: "BankAccounts",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_StorerId",
                table: "BankAccounts",
                newName: "IX_BankAccounts_OwnerId");

            migrationBuilder.Sql(
                "ALTER TABLE \"BankAccounts\" RENAME CONSTRAINT \"PK_Payments\" TO \"PK_BankAccounts\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"BankAccounts\" RENAME CONSTRAINT \"PK_BankAccounts\" TO \"PK_Payments\";");

            migrationBuilder.RenameIndex(
                name: "IX_BankAccounts_OwnerId",
                table: "BankAccounts",
                newName: "IX_Payments_StorerId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "BankAccounts",
                newName: "StorerId");

            migrationBuilder.RenameTable(
                name: "BankAccounts",
                newName: "Payments");
        }
    }
}
