using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleCerto.Migrations
{
    /// <inheritdoc />
    public partial class Recurring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurrenceRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    IsEveryDay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DaysOfWeek = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    DayOfMonth = table.Column<int>(type: "integer", nullable: true),
                    MonthOfYear = table.Column<int>(type: "integer", nullable: true),
                    DayOfMonthForYearly = table.Column<int>(type: "integer", nullable: true),
                    Interval = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrenceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecurringTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Destination = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    JustForRecord = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Amount = table.Column<double>(type: "numeric(18,2)", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RecurrenceRuleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_RecurrenceRules_RecurrenceRuleId",
                        column: x => x.RecurrenceRuleId,
                        principalTable: "RecurrenceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecurringTransactionInstances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecurringTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "date", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ActualTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringTransactionInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringTransactionInstances_RecurringTransactions_Recurri~",
                        column: x => x.RecurringTransactionId,
                        principalTable: "RecurringTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringTransactionInstances_Transactions_ActualTransactio~",
                        column: x => x.ActualTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceRules_DayOfMonth",
                table: "RecurrenceRules",
                column: "DayOfMonth");

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceRules_DayOfWeek",
                table: "RecurrenceRules",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceRules_Frequency",
                table: "RecurrenceRules",
                column: "Frequency");

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceRules_MonthOfYear",
                table: "RecurrenceRules",
                column: "MonthOfYear");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_ActualTransactionId",
                table: "RecurringTransactionInstances",
                column: "ActualTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_ProcessedDate",
                table: "RecurringTransactionInstances",
                column: "ProcessedDate");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_RecurringTransactionId",
                table: "RecurringTransactionInstances",
                column: "RecurringTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_RecurringTransactionId_Schedu~",
                table: "RecurringTransactionInstances",
                columns: new[] { "RecurringTransactionId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_ScheduledDate",
                table: "RecurringTransactionInstances",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_Status",
                table: "RecurringTransactionInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionInstances_Status_ScheduledDate",
                table: "RecurringTransactionInstances",
                columns: new[] { "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_AccountId",
                table: "RecurringTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_CategoryId",
                table: "RecurringTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_EndDate",
                table: "RecurringTransactions",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_IsActive",
                table: "RecurringTransactions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_RecurrenceRuleId",
                table: "RecurringTransactions",
                column: "RecurrenceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_StartDate",
                table: "RecurringTransactions",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_Type",
                table: "RecurringTransactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_UserId",
                table: "RecurringTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_UserId_IsActive_StartDate",
                table: "RecurringTransactions",
                columns: new[] { "UserId", "IsActive", "StartDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecurringTransactionInstances");

            migrationBuilder.DropTable(
                name: "RecurringTransactions");

            migrationBuilder.DropTable(
                name: "RecurrenceRules");
        }
    }
}
