using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseBenefits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CouponDiscount",
                schema: "payment",
                table: "PurchaseTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                schema: "payment",
                table: "PurchaseTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RedeemedAt",
                schema: "payment",
                table: "GiftCards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "RedeemedByUserIds",
                schema: "payment",
                table: "Coupons",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponDiscount",
                schema: "payment",
                table: "PurchaseTokens");

            migrationBuilder.DropColumn(
                name: "CouponId",
                schema: "payment",
                table: "PurchaseTokens");

            migrationBuilder.DropColumn(
                name: "RedeemedAt",
                schema: "payment",
                table: "GiftCards");

            migrationBuilder.DropColumn(
                name: "RedeemedByUserIds",
                schema: "payment",
                table: "Coupons");
        }
    }
}
