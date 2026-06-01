using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReUse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecentFavoriteCount",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_products_premium",
                table: "products",
                columns: new[] { "IsPremium", "PremiumExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "ix_products_status_created",
                table: "products",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_products_status_favcount",
                table: "products",
                columns: new[] { "Status", "RecentFavoriteCount" });

            migrationBuilder.CreateIndex(
                name: "ix_products_status_location",
                table: "products",
                columns: new[] { "Status", "LocationCountry", "LocationCity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_premium",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_status_created",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_status_favcount",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_status_location",
                table: "products");

            migrationBuilder.DropColumn(
                name: "RecentFavoriteCount",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "products");
        }
    }
}