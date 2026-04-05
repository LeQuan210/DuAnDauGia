using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDauGiaInfrasData.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerID",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "SellerID",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Products",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "Products",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "StartPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StartPrice",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Products",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Products",
                newName: "SellerID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerID",
                table: "Products",
                column: "SellerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerID",
                table: "Products",
                column: "SellerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
