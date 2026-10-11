using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleCerto.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringInstanceUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactionInstances_RecurringTransactionId_Schedu~",
                table: "RecurringTransactionInstances");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_RecurringTransactionId_Schedu~",
                table: "RecurringTransactionInstances",
                columns: new[] { "RecurringTransactionId", "ScheduledDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactionInstances_RecurringTransactionId_Schedu~",
                table: "RecurringTransactionInstances");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_RecurringTransactionId_Schedu~",
                table: "RecurringTransactionInstances",
                columns: new[] { "RecurringTransactionId", "ScheduledDate" });
        }
    }
}
