using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace flatshare_server.Migrations
{
    /// <inheritdoc />
    public partial class AddStripePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ViolationReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationReports", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViolationReports");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");
        }
    }
}
