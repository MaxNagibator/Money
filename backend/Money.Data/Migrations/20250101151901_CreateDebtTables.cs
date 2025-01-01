using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateDebtTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "debt_users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_debt_users", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_debt_users_domain_users_user_id",
                        column: x => x.user_id,
                        principalTable: "domain_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "debts",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    sum = table.Column<decimal>(type: "numeric", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    pay_sum = table.Column<decimal>(type: "numeric", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    pay_comment = table.Column<string>(type: "text", nullable: true),
                    debt_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_debts", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_debts_domain_users_user_id",
                        column: x => x.user_id,
                        principalTable: "domain_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "debt_users");

            migrationBuilder.DropTable(
                name: "debts");
        }
    }
}
