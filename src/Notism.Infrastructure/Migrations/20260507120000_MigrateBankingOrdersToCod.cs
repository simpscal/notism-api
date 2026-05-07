using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateBankingOrdersToCod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"PaymentMethod\" = 'cashOnDelivery' WHERE \"PaymentMethod\" = 'banking' AND \"PaymentStatus\" = 'unpaid'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"PaymentMethod\" = 'banking' WHERE \"PaymentMethod\" = 'cashOnDelivery' AND \"PaymentStatus\" = 'unpaid'");
        }
    }
}
