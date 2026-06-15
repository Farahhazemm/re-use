using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReUse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeReportReporterSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reports_Users_ReporterUserId",
                table: "reports");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReporterUserId",
                table: "reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_reports_Users_ReporterUserId",
                table: "reports",
                column: "ReporterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reports_Users_ReporterUserId",
                table: "reports");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReporterUserId",
                table: "reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_Users_ReporterUserId",
                table: "reports",
                column: "ReporterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}