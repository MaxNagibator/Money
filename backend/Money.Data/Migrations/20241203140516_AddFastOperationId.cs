using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFastOperationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "next_fast_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "next_regular_operation_id",
                table: "domain_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_fast_operation_id",
                table: "domain_users");

            migrationBuilder.DropColumn(
                name: "next_regular_operation_id",
                table: "domain_users");
        }
    }
}
