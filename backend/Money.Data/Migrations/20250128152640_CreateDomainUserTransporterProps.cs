using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateDomainUserTransporterProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "transporter_create_date",
                table: "domain_users",
                type: "timestamp with time zone",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "transporter_password",
                table: "domain_users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transporter_create_date",
                table: "domain_users");

            migrationBuilder.DropColumn(
                name: "transporter_email",
                table: "domain_users");

            migrationBuilder.DropColumn(
                name: "transporter_login",
                table: "domain_users");

            migrationBuilder.DropColumn(
                name: "transporter_password",
                table: "domain_users");
        }
    }
}
