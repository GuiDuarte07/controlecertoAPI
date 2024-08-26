using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleCerto.Migrations
{
    /// <inheritdoc />
    public partial class SkeepWekeend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SkipWeekend",
                table: "CreditCards",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkipWeekend",
                table: "CreditCards");
        }
    }
}
