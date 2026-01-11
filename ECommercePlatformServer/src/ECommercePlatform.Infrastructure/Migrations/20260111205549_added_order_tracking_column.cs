using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_order_tracking_column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CargoTrackingNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoTrackingNumber",
                table: "Orders");
        }
    }
}
