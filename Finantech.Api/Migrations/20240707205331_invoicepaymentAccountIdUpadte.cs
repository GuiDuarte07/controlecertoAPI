using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finantech.Migrations
{
    /// <inheritdoc />
    public partial class invoicepaymentAccountIdUpadte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_Accounts_AccountPaidId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_AccountPaidId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "AccountPaidId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "CreditCards");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosingDate",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AccountId",
                table: "InvoicePayments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_AccountId",
                table: "InvoicePayments",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_Accounts_AccountId",
                table: "InvoicePayments",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_Accounts_AccountId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_AccountId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "InvoicePayments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosingDate",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<long>(
                name: "AccountPaidId",
                table: "InvoicePayments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "CreditCards",
                type: "character varying(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

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
    }
}
