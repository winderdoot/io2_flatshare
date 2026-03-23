using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace flatshare_server.Migrations
{
    /// <inheritdoc />
    public partial class Addaddressindexandphotostolistings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Price_Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Price_Curr = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableSince = table.Column<DateOnly>(type: "date", nullable: false),
                    AvailableUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    OwnerContact = table.Column<string>(type: "text", nullable: false),
                    AreaMeterSq = table.Column<float>(type: "real", nullable: false),
                    Address_City = table.Column<string>(type: "text", nullable: false),
                    Address_District = table.Column<string>(type: "text", nullable: false),
                    Address_Street = table.Column<string>(type: "text", nullable: false),
                    Address_AptNumber = table.Column<string>(type: "text", nullable: false),
                    Attributes_PetsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    Attributes_NonSmokingOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Attributes_CloseToShops = table.Column<bool>(type: "boolean", nullable: false),
                    Attributes_Profile = table.Column<int>(type: "integer", nullable: false),
                    Photos = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Listing_Address",
                table: "Listings",
                columns: new[] { "Address_City", "Address_District", "Address_Street", "Address_AptNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Listings");
        }
    }
}
