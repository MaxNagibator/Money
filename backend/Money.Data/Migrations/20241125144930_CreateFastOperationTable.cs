using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateFastOperationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "task_id",
                table: "operations");

            migrationBuilder.CreateTable(
                name: "fast_operations",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    sum = table.Column<decimal>(type: "numeric", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    place_id = table.Column<int>(type: "integer", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fast_operations", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_fast_operations_domain_users_user_id",
                        column: x => x.user_id,
                        principalTable: "domain_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regular_operations",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    sum = table.Column<decimal>(type: "numeric", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    place_id = table.Column<int>(type: "integer", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regular_operations", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_regular_operations_domain_users_user_id",
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
                name: "fast_operations");

            migrationBuilder.DropTable(
                name: "regular_operations");

            migrationBuilder.AddColumn<int>(
                name: "task_id",
                table: "operations",
                type: "integer",
                nullable: true);
        }
    }
}
