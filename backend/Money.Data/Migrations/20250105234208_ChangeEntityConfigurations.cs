using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntityConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "next_regular_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "next_place_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "next_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "next_fast_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "next_debt_user_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "next_debt_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "next_category_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "domain_users",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "pay_comment",
                table: "debts",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "debts",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "debt_users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_debts_user_id_debt_user_id",
                table: "debts",
                columns: new[] { "user_id", "debt_user_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_debts_debt_users_user_id_debt_user_id",
                table: "debts",
                columns: new[] { "user_id", "debt_user_id" },
                principalTable: "debt_users",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(@"
            UPDATE domain_users
            SET next_category_id = 1
            WHERE next_category_id = 0;
            
            UPDATE domain_users
            SET next_debt_id = 1
            WHERE next_debt_id = 0;
            
            UPDATE domain_users
            SET next_debt_user_id = 1
            WHERE next_debt_user_id = 0;
            
            UPDATE domain_users
            SET next_fast_operation_id = 1
            WHERE next_fast_operation_id = 0;
            
            UPDATE domain_users
            SET next_operation_id = 1
            WHERE next_operation_id = 0;
            
            UPDATE domain_users
            SET next_place_id = 1
            WHERE next_place_id = 0;
            
            UPDATE domain_users
            SET next_regular_operation_id = 1
            WHERE next_regular_operation_id = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_debts_debt_users_user_id_debt_user_id",
                table: "debts");

            migrationBuilder.DropIndex(
                name: "ix_debts_user_id_debt_user_id",
                table: "debts");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "domain_users");

            migrationBuilder.AlterColumn<int>(
                name: "next_regular_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "next_place_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "next_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "next_fast_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "next_debt_user_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "next_debt_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "next_category_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "pay_comment",
                table: "debts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "debts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "debt_users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
