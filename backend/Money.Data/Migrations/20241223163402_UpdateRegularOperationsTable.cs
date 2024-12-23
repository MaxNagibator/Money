using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRegularOperationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_from",
                table: "regular_operations",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_to",
                table: "regular_operations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "run_time",
                table: "regular_operations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "time_type_id",
                table: "regular_operations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "time_value",
                table: "regular_operations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_from",
                table: "regular_operations");

            migrationBuilder.DropColumn(
                name: "date_to",
                table: "regular_operations");

            migrationBuilder.DropColumn(
                name: "run_time",
                table: "regular_operations");

            migrationBuilder.DropColumn(
                name: "time_type_id",
                table: "regular_operations");

            migrationBuilder.DropColumn(
                name: "time_value",
                table: "regular_operations");
        }
    }
}
