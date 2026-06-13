using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReUse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addsystemactivitylogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_activity_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Info"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Success"),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_activity_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_system_activity_logs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_ActionType",
                table: "system_activity_logs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_ActorUserId",
                table: "system_activity_logs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_Category",
                table: "system_activity_logs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_Category_CreatedAt",
                table: "system_activity_logs",
                columns: new[] { "Category", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_CreatedAt",
                table: "system_activity_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_EntityType_EntityId",
                table: "system_activity_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_system_activity_logs_Severity",
                table: "system_activity_logs",
                column: "Severity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_activity_logs");
        }
    }
}