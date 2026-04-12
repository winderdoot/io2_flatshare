using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace flatshare_server.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantPreferences_Currency",
                table: "UserRole",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TenantPreferences_MaxPrice",
                table: "UserRole",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TenantPreferences_PetsAllowed",
                table: "UserRole",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "TenantPreferences_PreferredDistricts",
                table: "UserRole",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TenantPreferences_SmokingAllowed",
                table: "UserRole",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantPreferences_Currency",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "TenantPreferences_MaxPrice",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "TenantPreferences_PetsAllowed",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "TenantPreferences_PreferredDistricts",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "TenantPreferences_SmokingAllowed",
                table: "UserRole");
        }
    }
}
