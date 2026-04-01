using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notism.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToFood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Foods_Category",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_Foods_Category_IsAvailable",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Foods");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Foods",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Foods_CategoryId",
                table: "Foods",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_CategoryId_IsAvailable",
                table: "Foods",
                columns: new[] { "CategoryId", "IsAvailable" });

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_Categories_CategoryId",
                table: "Foods",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Foods_Categories_CategoryId",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_Foods_CategoryId",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_Foods_CategoryId_IsAvailable",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Foods");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Foods",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_Category",
                table: "Foods",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_Category_IsAvailable",
                table: "Foods",
                columns: new[] { "Category", "IsAvailable" });
        }
    }
}
