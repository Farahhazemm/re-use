using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReUse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityEventIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityEvents_UserId",
                table: "ActivityEvents");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEvents_UserId_Timestamp",
                table: "ActivityEvents",
                columns: new[] { "UserId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityEvents_UserId_Timestamp",
                table: "ActivityEvents");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEvents_UserId",
                table: "ActivityEvents",
                column: "UserId");
        }
    }
}