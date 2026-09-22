using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomCourse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_City",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Country",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_PostalCode",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Street",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Backfill existing Orders from their Customer's current address instead
            // of leaving them with an empty string — the address at order time was
            // never captured before this migration, so the customer's current
            // address is the closest available approximation.
            migrationBuilder.Sql(@"
                UPDATE o
                SET o.ShippingAddress_Street = c.Address_Street,
                    o.ShippingAddress_City = c.Address_City,
                    o.ShippingAddress_PostalCode = c.Address_PostalCode,
                    o.ShippingAddress_Country = c.Address_Country
                FROM Orders o
                INNER JOIN Customers c ON c.Id = o.CustomerId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Country",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_PostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Street",
                table: "Orders");
        }
    }
}
