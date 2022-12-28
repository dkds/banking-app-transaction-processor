using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Migrations
{
    /// <inheritdoc />
    public partial class TransactionAccountNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberFrom",
                table: "Transaction",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberTo",
                table: "Transaction",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumberFrom",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AccountNumberTo",
                table: "Transaction");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Transaction",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
