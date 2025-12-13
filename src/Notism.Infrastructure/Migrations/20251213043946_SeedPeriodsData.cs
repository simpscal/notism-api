using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPeriodsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Insert mock periods
            migrationBuilder.InsertData(
                table: "Periods",
                columns: new[] { "Id", "Name", "StartYear", "EndYear", "Description", "DisplayOrder", "IsPublished", "CreatedAt", "UpdatedAt", "IsDeleted" },
                values: new object[,]
                {
                    {
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        "Ancient History",
                        3000,
                        500,
                        "The period covering early civilizations, ancient empires, and the foundations of human society.",
                        1,
                        true,
                        now,
                        now,
                        false
                    },
                    {
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        "Medieval Period",
                        500,
                        1500,
                        "The Middle Ages, characterized by feudalism, the rise of Christianity, and significant cultural developments.",
                        2,
                        true,
                        now,
                        now,
                        false
                    },
                    {
                        Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        "Renaissance",
                        1400,
                        1600,
                        "A period of cultural rebirth, artistic innovation, and scientific discovery in Europe.",
                        3,
                        true,
                        now,
                        now,
                        false
                    },
                    {
                        Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        "Modern Era",
                        1500,
                        1800,
                        "The age of exploration, colonization, and the beginning of the modern world.",
                        4,
                        true,
                        now,
                        now,
                        false
                    },
                    {
                        Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        "Industrial Revolution",
                        1760,
                        1840,
                        "A period of major industrialization and technological advancement that transformed society.",
                        5,
                        true,
                        now,
                        now,
                        false
                    },
                    {
                        Guid.Parse("66666666-6666-6666-6666-666666666666"),
                        "Contemporary Period",
                        1900,
                        2024,
                        "The modern era including world wars, technological revolution, and globalization.",
                        6,
                        true,
                        now,
                        now,
                        false
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Periods",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Guid.Parse("66666666-6666-6666-6666-666666666666")
                });
        }
    }
}
