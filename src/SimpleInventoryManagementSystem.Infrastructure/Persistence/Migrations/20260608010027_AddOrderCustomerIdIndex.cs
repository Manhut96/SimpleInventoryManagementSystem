using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCustomerIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tbl_orders_CustomerId",
                schema: "ordering",
                table: "tbl_orders",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_orders_CustomerId",
                schema: "ordering",
                table: "tbl_orders");
        }
    }
}
