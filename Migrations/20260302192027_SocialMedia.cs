using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Project.Migrations
{
    /// <inheritdoc />
    public partial class SocialMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorFacebookUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorInstagramUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorLinkedInUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorTwitterUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorFacebookUrl",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "AuthorInstagramUrl",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "AuthorLinkedInUrl",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "AuthorTwitterUrl",
                table: "BlogPosts");
        }
    }
}
