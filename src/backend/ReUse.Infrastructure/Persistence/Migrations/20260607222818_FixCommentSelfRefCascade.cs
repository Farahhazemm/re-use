using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReUse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentSelfRefCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_comments_Users_UserId",
                table: "product_comments");

            migrationBuilder.DropForeignKey(
                name: "FK_product_comments_product_comments_ParentCommentId",
                table: "product_comments");

            migrationBuilder.AddForeignKey(
                name: "FK_product_comments_Users_UserId",
                table: "product_comments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_comments_product_comments_ParentCommentId",
                table: "product_comments",
                column: "ParentCommentId",
                principalTable: "product_comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_comments_Users_UserId",
                table: "product_comments");

            migrationBuilder.DropForeignKey(
                name: "FK_product_comments_product_comments_ParentCommentId",
                table: "product_comments");

            migrationBuilder.AddForeignKey(
                name: "FK_product_comments_Users_UserId",
                table: "product_comments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_comments_product_comments_ParentCommentId",
                table: "product_comments",
                column: "ParentCommentId",
                principalTable: "product_comments",
                principalColumn: "Id");
        }
    }
}