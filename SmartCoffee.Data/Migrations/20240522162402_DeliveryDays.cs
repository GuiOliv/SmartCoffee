using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCoffee.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryDays",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDays",
                table: "Products");
        }
    }
}
