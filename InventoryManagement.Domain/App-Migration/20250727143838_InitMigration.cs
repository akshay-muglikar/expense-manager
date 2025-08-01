using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Domain.AppMigration
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillItems_Items_ItemId1",
                table: "BillItems");

            migrationBuilder.DropIndex(
                name: "IX_BillItems_ItemId1",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemId1",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "OtherItem",
                table: "BillItems");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Bills",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CalculatedBillAmount",
                table: "Bills",
                newName: "PaymentMode");

            migrationBuilder.AddColumn<string>(
                name: "PaymentUser",
                table: "Bills",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentUser",
                table: "Bills");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Bills",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "PaymentMode",
                table: "Bills",
                newName: "CalculatedBillAmount");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemId1",
                table: "BillItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherItem",
                table: "BillItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillItems_ItemId1",
                table: "BillItems",
                column: "ItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BillItems_Items_ItemId1",
                table: "BillItems",
                column: "ItemId1",
                principalTable: "Items",
                principalColumn: "Id");
        }
    }
}
