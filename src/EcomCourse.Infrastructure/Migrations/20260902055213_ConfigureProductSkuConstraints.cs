using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomCourse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureProductSkuConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SKUValue",
                table: "Products",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKUValue",
                table: "Products",
                column: "SKUValue",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SKUValue",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "SKUValue",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);
        }
    }
}
