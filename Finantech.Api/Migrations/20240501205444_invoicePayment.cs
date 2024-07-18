using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finantech.Migrations
{
    /// <inheritdoc />
    public partial class invoicePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountPaidId",
                table: "InvoicePayments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "JustForRecord",
                table: "InvoicePayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaidAccountId",
                table: "InvoicePayments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_PaidAccountId",
                table: "InvoicePayments",
                column: "PaidAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_Accounts_PaidAccountId",
                table: "InvoicePayments",
                column: "PaidAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_Accounts_PaidAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_PaidAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "AccountPaidId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "JustForRecord",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "PaidAccountId",
                table: "InvoicePayments");
        }
    }
}
