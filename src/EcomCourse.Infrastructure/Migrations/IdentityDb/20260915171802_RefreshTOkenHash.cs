using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomCourse.Infrastructure.Migrations.IdentityDb
{
    /// <inheritdoc />
    public partial class RefreshTOkenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Token",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByTokenHash",
                schema: "identity",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                schema: "identity",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                schema: "identity",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                schema: "identity",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByTokenHash",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                schema: "identity",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                schema: "identity",
                table: "RefreshTokens",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "identity",
                table: "RefreshTokens",
                column: "Token",
                unique: true);
        }
    }
}
