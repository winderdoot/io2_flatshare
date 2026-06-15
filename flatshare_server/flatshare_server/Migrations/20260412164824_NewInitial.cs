using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace flatshare_server.Migrations
{
    /// <inheritdoc />
    public partial class NewInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Foos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bar = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PassHash = table.Column<string>(type: "text", nullable: false),
                    Status_Value = table.Column<string>(type: "text", nullable: false),
                    Status_Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

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
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Photos = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResetCode = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TenantPreferences_MaxPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    TenantPreferences_Currency = table.Column<string>(type: "text", nullable: true),
                    TenantPreferences_SmokingAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    TenantPreferences_PetsAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    TenantPreferences_PreferredDistricts = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Listing_Address",
                table: "Listings",
                columns: new[] { "Address_City", "Address_District", "Address_Street", "Address_AptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_OwnerId",
                table: "Listings",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetEntries_UserId_ResetCode",
                table: "PasswordResetEntries",
                columns: new[] { "UserId", "ResetCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foos");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "PasswordResetEntries");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
