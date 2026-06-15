using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReUse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class newReportcolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorEmail",
                table: "system_activity_logs",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActorName",
                table: "system_activity_logs",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReporterEmail",
                table: "reports",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReporterName",
                table: "reports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorEmail",
                table: "system_activity_logs");

            migrationBuilder.DropColumn(
                name: "ActorName",
                table: "system_activity_logs");

            migrationBuilder.DropColumn(
                name: "ReporterEmail",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "ReporterName",
                table: "reports");
        }
    }
}