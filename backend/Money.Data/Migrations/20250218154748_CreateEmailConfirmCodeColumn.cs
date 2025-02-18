using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateEmailConfirmCodeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_confirm_code",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_confirm_code",
                table: "AspNetUsers");
        }
    }
}
