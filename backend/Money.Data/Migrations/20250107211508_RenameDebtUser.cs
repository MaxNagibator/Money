using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDebtUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "next_debt_user_id",
                table: "domain_users",
                newName: "next_debt_owner_id");

            migrationBuilder.RenameColumn(
                name: "debt_user_id",
                table: "debts",
                newName: "owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_debts_user_id_debt_user_id",
                table: "debts",
                newName: "ix_debts_user_id_owner_id");

            migrationBuilder.CreateTable(
                name: "debt_owners",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_debt_owners", x => new { x.user_id, x.id });
                    table.ForeignKey(
                        name: "fk_debt_owners_domain_users_user_id",
                        column: x => x.user_id,
                        principalTable: "domain_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO debt_owners (user_id, id, name)
                SELECT user_id, id, name
                FROM debt_users
            ");

            migrationBuilder.AddForeignKey(
                name: "fk_debts_debt_owners_user_id_owner_id",
                table: "debts",
                columns: new[] { "user_id", "owner_id" },
                principalTable: "debt_owners",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "fk_debts_debt_users_user_id_debt_user_id",
                table: "debts");

            migrationBuilder.DropTable(
                name: "debt_users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_debts_debt_owners_user_id_owner_id",
                table: "debts");

            migrationBuilder.DropTable(
                name: "debt_owners");

            migrationBuilder.RenameColumn(
                name: "next_debt_owner_id",
                table: "domain_users",
                newName: "next_debt_user_id");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "debts",
                newName: "debt_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_debts_user_id_owner_id",
                table: "debts",
                newName: "ix_debts_user_id_debt_user_id");

            migrationBuilder.CreateTable(
                name: "debt_users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "fk_debts_debt_users_user_id_debt_user_id",
                table: "debts",
                columns: new[] { "user_id", "debt_user_id" },
                principalTable: "debt_users",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
