using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations;

/// <inheritdoc />
public partial class CreateDomainUserNextCategoryIdColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "next_category_id",
            table: "domain_users",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<int>(
            name: "next_payment_id",
            table: "domain_users",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.Sql(
@"UPDATE public.domain_users
SET next_category_id = u2.next_id
FROM public.domain_users u
    INNER JOIN
        (
            SELECT (MAX(id) + 1) next_id, user_id
            FROM public.categories
            GROUP BY user_id
        ) AS u2 ON u2.user_id = u.id
WHERE public.domain_users.id = u.id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "next_category_id",
            table: "domain_users");

        migrationBuilder.DropColumn(
            name: "next_payment_id",
            table: "domain_users");
    }
}
