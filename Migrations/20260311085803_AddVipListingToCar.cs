using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddVipListingToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ListingType",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VipPaidAt",
                table: "Cars",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListingType",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "VipPaidAt",
                table: "Cars");
        }
    }
}
