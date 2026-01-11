using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class shipping_feature_added_to_company_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreeShippingThreshold",
                table: "Companies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFlatRate",
                table: "Companies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeShippingThreshold",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ShippingFlatRate",
                table: "Companies");
        }
    }
}
