using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnableCaseInsensitiveSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transporter_email",
                table: "domain_users");

            migrationBuilder.DropColumn(
                name: "transporter_login",
                table: "domain_users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "places",
                type: "citext",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "operations",
                type: "citext",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_places_name",
                table: "places",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_operations_comment",
                table: "operations",
                column: "comment");

            migrationBuilder.CreateIndex(
                name: "ix_car_events_user_id_car_id",
                table: "car_events",
                columns: new[] { "user_id", "car_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_car_events_cars_user_id_car_id",
                table: "car_events",
                columns: new[] { "user_id", "car_id" },
                principalTable: "cars",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_car_events_cars_user_id_car_id",
                table: "car_events");

            migrationBuilder.DropIndex(
                name: "ix_places_name",
                table: "places");

            migrationBuilder.DropIndex(
                name: "ix_operations_comment",
                table: "operations");

            migrationBuilder.DropIndex(
                name: "ix_car_events_user_id_car_id",
                table: "car_events");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "places",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "operations",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transporter_email",
                table: "domain_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transporter_login",
                table: "domain_users",
                type: "text",
                nullable: true);
        }
    }
}
