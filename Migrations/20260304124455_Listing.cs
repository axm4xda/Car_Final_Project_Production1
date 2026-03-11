using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Project.Migrations
{
    /// <inheritdoc />
    public partial class Listing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SellCarRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "SellCarRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrashedDate",
                table: "SellCarRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedCarId",
                table: "SellCarRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SellCarRequests");

            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "SellCarRequests");

            migrationBuilder.DropColumn(
                name: "TrashedDate",
                table: "SellCarRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedCarId",
                table: "SellCarRequests");
        }
    }
}
