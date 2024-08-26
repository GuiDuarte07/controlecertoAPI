using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleCerto.Migrations
{
    /// <inheritdoc />
    public partial class DeleteAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_Accounts_PaidAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_PaidAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "PaidAccountId",
                table: "InvoicePayments");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Transferences",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_AccountPaidId",
                table: "InvoicePayments",
                column: "AccountPaidId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_Accounts_AccountPaidId",
                table: "InvoicePayments",
                column: "AccountPaidId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_Accounts_AccountPaidId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_AccountPaidId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Accounts");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Transferences",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "numeric(10,2)");

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
    }
}
