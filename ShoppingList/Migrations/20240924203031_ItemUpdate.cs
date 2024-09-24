using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabWeb.Migrations
{
    /// <inheritdoc />
    public partial class ItemUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "decimal(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Items");
        }
    }
}
