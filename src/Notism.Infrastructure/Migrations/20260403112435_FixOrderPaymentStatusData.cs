using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderPaymentStatusData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"PaymentStatus\" = 'unpaid' WHERE \"PaymentStatus\" = '0'");
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"PaymentStatus\" = 'paid' WHERE \"PaymentStatus\" = '1'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"PaymentStatus\" = '0' WHERE \"PaymentStatus\" = 'unpaid'");
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"PaymentStatus\" = '1' WHERE \"PaymentStatus\" = 'paid'");
        }
    }
}
