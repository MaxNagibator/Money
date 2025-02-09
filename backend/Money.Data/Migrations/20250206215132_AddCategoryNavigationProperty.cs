using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryNavigationProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_regular_operations_user_id_category_id",
                table: "regular_operations",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_operations_user_id_category_id",
                table: "operations",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_fast_operations_user_id_category_id",
                table: "fast_operations",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_fast_operations_categories_user_id_category_id",
                table: "fast_operations",
                columns: new[] { "user_id", "category_id" },
                principalTable: "categories",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_operations_categories_user_id_category_id",
                table: "operations",
                columns: new[] { "user_id", "category_id" },
                principalTable: "categories",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_regular_operations_categories_user_id_category_id",
                table: "regular_operations",
                columns: new[] { "user_id", "category_id" },
                principalTable: "categories",
                principalColumns: new[] { "user_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fast_operations_categories_user_id_category_id",
                table: "fast_operations");

            migrationBuilder.DropForeignKey(
                name: "fk_operations_categories_user_id_category_id",
                table: "operations");

            migrationBuilder.DropForeignKey(
                name: "fk_regular_operations_categories_user_id_category_id",
                table: "regular_operations");

            migrationBuilder.DropIndex(
                name: "ix_regular_operations_user_id_category_id",
                table: "regular_operations");

            migrationBuilder.DropIndex(
                name: "ix_operations_user_id_category_id",
                table: "operations");

            migrationBuilder.DropIndex(
                name: "ix_fast_operations_user_id_category_id",
                table: "fast_operations");
        }
    }
}
