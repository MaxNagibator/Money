using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "next_payment_id",
                table: "domain_users",
                newName: "next_operation_id");

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    sum = table.Column<decimal>(type: "numeric", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    task_id = table.Column<int>(type: "integer", nullable: true),
                    created_task_id = table.Column<int>(type: "integer", nullable: true),
                    place_id = table.Column<int>(type: "integer", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operations", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_operations_domain_users_user_id",
                        column: x => x.user_id,
                        principalTable: "domain_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                                 INSERT INTO operations (user_id, id, sum, category_id, comment, date, task_id, created_task_id, place_id, is_deleted)
                                 SELECT user_id, id, sum, category_id, comment, date, task_id, created_task_id, place_id, is_deleted
                                 FROM payments
                                 """);

            migrationBuilder.DropTable(
                name: "payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "next_operation_id",
                table: "domain_users",
                newName: "next_payment_id");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_task_id = table.Column<int>(type: "integer", nullable: true),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    place_id = table.Column<int>(type: "integer", nullable: true),
                    sum = table.Column<decimal>(type: "numeric", nullable: false),
                    task_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_payments_domain_users_user_id",
                        column: x => x.user_id,
                        principalTable: "domain_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                                 INSERT INTO payments (user_id, id, sum, category_id, comment, date, task_id, created_task_id, place_id, is_deleted)
                                 SELECT user_id, id, sum, category_id, comment, date, task_id, created_task_id, place_id, is_deleted
                                 FROM operations
                                 """);

            migrationBuilder.DropTable(
                name: "operations");
        }
    }
}
