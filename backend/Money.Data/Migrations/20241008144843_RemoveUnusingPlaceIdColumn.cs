using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusingPlaceIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "place_id",
                table: "places");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "payments",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "place_id",
                table: "places",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "payments",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
