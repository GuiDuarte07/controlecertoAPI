using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finantech.Migrations
{
    /// <inheritdoc />
    public partial class closeDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalInstalment",
                table: "CreditPurchases",
                newName: "TotalInstallment");

            migrationBuilder.RenameColumn(
                name: "InstalmentsPaid",
                table: "CreditPurchases",
                newName: "InstallmentsPaid");

            migrationBuilder.RenameColumn(
                name: "Limit",
                table: "CreditCards",
                newName: "UsedLimit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TotalPaid",
                table: "Invoices",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TotalAmount",
                table: "Invoices",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPaid",
                table: "Invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DueDate",
                table: "Invoices",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ClosingDate",
                table: "Invoices",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CreditExpenses",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CloseDate",
                table: "CreditCards",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "CreditCards",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<double>(
                name: "TotalLimit",
                table: "CreditCards",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "TotalLimit",
                table: "CreditCards");

            migrationBuilder.RenameColumn(
                name: "TotalInstallment",
                table: "CreditPurchases",
                newName: "TotalInstalment");

            migrationBuilder.RenameColumn(
                name: "InstallmentsPaid",
                table: "CreditPurchases",
                newName: "InstalmentsPaid");

            migrationBuilder.RenameColumn(
                name: "UsedLimit",
                table: "CreditCards",
                newName: "Limit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<double>(
                name: "TotalPaid",
                table: "Invoices",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<double>(
                name: "TotalAmount",
                table: "Invoices",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPaid",
                table: "Invoices",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosingDate",
                table: "Invoices",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CreditExpenses",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
