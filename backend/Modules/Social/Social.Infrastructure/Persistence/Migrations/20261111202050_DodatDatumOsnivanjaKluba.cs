using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatDatumOsnivanjaKluba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "social",
                table: "Clubs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "social",
                table: "Clubs");
        }
    }
}
