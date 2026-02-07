using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleCerto.Migrations
{
    /// <inheritdoc />
    public partial class RemovingCategoryDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriesDefault");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentHistories_SourceAccountId",
                table: "InvestmentHistories",
                column: "SourceAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentHistories_Accounts_SourceAccountId",
                table: "InvestmentHistories",
                column: "SourceAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentHistories_Accounts_SourceAccountId",
                table: "InvestmentHistories");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentHistories_SourceAccountId",
                table: "InvestmentHistories");

            migrationBuilder.CreateTable(
                name: "CategoriesDefault",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillType = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Icon = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesDefault", x => x.Id);
                });
        }
    }
}
