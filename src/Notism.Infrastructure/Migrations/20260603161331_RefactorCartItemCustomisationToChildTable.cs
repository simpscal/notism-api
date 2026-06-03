using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCartItemCustomisationToChildTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_FoodCustomisationGroups_CustomisationGroupId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_FoodCustomisationOptions_CustomisationOptionId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CustomisationGroupId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CustomisationOptionId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CustomisationGroupId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CustomisationLabel",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CustomisationOptionId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Surcharge",
                table: "CartItems");

            migrationBuilder.CreateTable(
                name: "CartItemCustomisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomisationGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomisationOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OptionLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Surcharge = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemCustomisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItemCustomisations_CartItems_CartItemId",
                        column: x => x.CartItemId,
                        principalTable: "CartItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItemCustomisations_FoodCustomisationGroups_Customisatio~",
                        column: x => x.CustomisationGroupId,
                        principalTable: "FoodCustomisationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CartItemCustomisations_FoodCustomisationOptions_Customisati~",
                        column: x => x.CustomisationOptionId,
                        principalTable: "FoodCustomisationOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItemCustomisations_CartItemId",
                table: "CartItemCustomisations",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemCustomisations_CustomisationGroupId",
                table: "CartItemCustomisations",
                column: "CustomisationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemCustomisations_CustomisationOptionId",
                table: "CartItemCustomisations",
                column: "CustomisationOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItemCustomisations");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomisationGroupId",
                table: "CartItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomisationLabel",
                table: "CartItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomisationOptionId",
                table: "CartItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Surcharge",
                table: "CartItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CustomisationGroupId",
                table: "CartItems",
                column: "CustomisationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CustomisationOptionId",
                table: "CartItems",
                column: "CustomisationOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_FoodCustomisationGroups_CustomisationGroupId",
                table: "CartItems",
                column: "CustomisationGroupId",
                principalTable: "FoodCustomisationGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_FoodCustomisationOptions_CustomisationOptionId",
                table: "CartItems",
                column: "CustomisationOptionId",
                principalTable: "FoodCustomisationOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
