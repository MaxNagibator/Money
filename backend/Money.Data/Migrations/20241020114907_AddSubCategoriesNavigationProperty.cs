using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoriesNavigationProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_categories_user_id_parent_id",
                table: "categories",
                columns: new[] { "user_id", "parent_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_categories_categories_user_id_parent_id",
                table: "categories",
                columns: new[] { "user_id", "parent_id" },
                principalTable: "categories",
                principalColumns: new[] { "user_id", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_categories_user_id_parent_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_user_id_parent_id",
                table: "categories");
        }
    }
}
