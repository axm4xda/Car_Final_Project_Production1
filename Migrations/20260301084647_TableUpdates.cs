using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Project.Migrations
{
    /// <inheritdoc />
    public partial class TableUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("UPDATE Cars SET Condition = 0 WHERE Id = 10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Cars");
        }
    }
}
