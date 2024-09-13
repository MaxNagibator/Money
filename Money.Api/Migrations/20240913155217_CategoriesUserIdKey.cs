using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Api.Migrations
{
    /// <inheritdoc />
    public partial class CategoriesUserIdKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_categories_domain_users_user_id",
                table: "categories",
                column: "user_id",
                principalTable: "domain_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_domain_users_user_id",
                table: "categories");
        }
    }
}
