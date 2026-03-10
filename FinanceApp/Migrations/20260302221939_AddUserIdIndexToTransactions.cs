using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdIndexToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                newName: "IX_Transaction_UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Transaction_UserId",
                table: "Transactions",
                newName: "IX_Transactions_UserId");
        }
    }
}
