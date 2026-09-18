using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomCourse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderLineCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "OrderLines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "OrderLines");
        }
    }
}
