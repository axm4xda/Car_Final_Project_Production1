using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Project.Migrations
{
    /// <inheritdoc />
    public partial class CommentReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentReviewId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "BlogComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ParentReviewId",
                table: "Reviews",
                column: "ParentReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_ParentCommentId",
                table: "BlogComments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogComments_BlogComments_ParentCommentId",
                table: "BlogComments",
                column: "ParentCommentId",
                principalTable: "BlogComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Reviews_ParentReviewId",
                table: "Reviews",
                column: "ParentReviewId",
                principalTable: "Reviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogComments_BlogComments_ParentCommentId",
                table: "BlogComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Reviews_ParentReviewId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ParentReviewId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_BlogComments_ParentCommentId",
                table: "BlogComments");

            migrationBuilder.DropColumn(
                name: "ParentReviewId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "BlogComments");
        }
    }
}
