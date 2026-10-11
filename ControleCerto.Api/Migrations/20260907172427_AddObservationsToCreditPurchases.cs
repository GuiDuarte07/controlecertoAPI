using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleCerto.Migrations
{
    /// <inheritdoc />
    public partial class AddObservationsToCreditPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "CreditPurchases",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observations",
                table: "CreditPurchases");
        }
    }
}
