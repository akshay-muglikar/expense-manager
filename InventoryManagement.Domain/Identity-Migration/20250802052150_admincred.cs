using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Domain.IdentityMigration
{
    /// <inheritdoc />
    public partial class admincred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO Users (Username, Password, ClientId) 
            VALUES ('admin', 'amadmin@123', '00000000-0000-0000-0000-000000000000');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
