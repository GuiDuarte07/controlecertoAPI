using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finantech.Migrations
{
    /// <inheritdoc />
    public partial class removeCardNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditCards_Number",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CreditCards");

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

            migrationBuilder.AddColumn<int>(
                name: "CloseDay",
                table: "CreditCards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DueDay",
                table: "CreditCards",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseDay",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "DueDay",
                table: "CreditCards");

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

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "CreditCards",
                type: "char(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_Number",
                table: "CreditCards",
                column: "Number",
                unique: true);
        }
    }
}
