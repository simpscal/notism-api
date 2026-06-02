using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemCustomisationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
