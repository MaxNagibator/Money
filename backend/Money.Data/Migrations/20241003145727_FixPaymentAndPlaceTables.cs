using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations;

/// <inheritdoc />
public partial class FixPaymentAndPlaceTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "type_id",
            table: "payments");

        migrationBuilder.AlterColumn<DateTime>(
            name: "date",
            table: "payments",
            type: "date",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<int>(
            name: "category_id",
            table: "payments",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "date",
            table: "payments",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "date");

        migrationBuilder.AlterColumn<int>(
            name: "category_id",
            table: "payments",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AddColumn<int>(
            name: "type_id",
            table: "payments",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
