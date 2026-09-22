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

            // Backfill existing OrderLines from their Product's actual currency
            // instead of leaving them at the column default (USD).
            migrationBuilder.Sql(@"
                UPDATE ol
                SET ol.Currency = CASE p.Currency
                    WHEN 'USD' THEN 0
                    WHEN 'EUR' THEN 1
                    WHEN 'UAH' THEN 2
                    ELSE ol.Currency
                END
                FROM OrderLines ol
                INNER JOIN Products p ON p.Id = ol.ProductId;
            ");
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
