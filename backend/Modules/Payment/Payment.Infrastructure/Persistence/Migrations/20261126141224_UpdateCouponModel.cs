using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCouponModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValidUntil",
                schema: "payment",
                table: "Coupons",
                newName: "ExpiresAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "payment",
                table: "Coupons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "payment",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                schema: "payment",
                table: "Coupons",
                newName: "ValidUntil");
        }
    }
}
