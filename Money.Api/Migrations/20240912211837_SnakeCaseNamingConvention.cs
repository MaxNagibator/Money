using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Api.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictTokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictScopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictAuthorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "domain_users");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictTokens",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictTokens",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OpenIddictTokens",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictTokens",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "OpenIddictTokens",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictTokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "OpenIddictTokens",
                newName: "reference_id");

            migrationBuilder.RenameColumn(
                name: "RedemptionDate",
                table: "OpenIddictTokens",
                newName: "redemption_date");

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "OpenIddictTokens",
                newName: "expiration_date");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OpenIddictTokens",
                newName: "creation_date");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictTokens",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "AuthorizationId",
                table: "OpenIddictTokens",
                newName: "authorization_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "OpenIddictTokens",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_reference_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_authorization_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_application_id_status_subject_type");

            migrationBuilder.RenameColumn(
                name: "Resources",
                table: "OpenIddictScopes",
                newName: "resources");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictScopes",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OpenIddictScopes",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Descriptions",
                table: "OpenIddictScopes",
                newName: "descriptions");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "OpenIddictScopes",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictScopes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DisplayNames",
                table: "OpenIddictScopes",
                newName: "display_names");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "OpenIddictScopes",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictScopes",
                newName: "concurrency_token");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictScopes_Name",
                table: "OpenIddictScopes",
                newName: "ix_open_iddict_scopes_name");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictAuthorizations",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictAuthorizations",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OpenIddictAuthorizations",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Scopes",
                table: "OpenIddictAuthorizations",
                newName: "scopes");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictAuthorizations",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictAuthorizations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OpenIddictAuthorizations",
                newName: "creation_date");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictAuthorizations",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "OpenIddictAuthorizations",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                newName: "ix_open_iddict_authorizations_application_id_status_subject_type");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "OpenIddictApplications",
                newName: "settings");

            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "OpenIddictApplications",
                newName: "requirements");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictApplications",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Permissions",
                table: "OpenIddictApplications",
                newName: "permissions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictApplications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RedirectUris",
                table: "OpenIddictApplications",
                newName: "redirect_uris");

            migrationBuilder.RenameColumn(
                name: "PostLogoutRedirectUris",
                table: "OpenIddictApplications",
                newName: "post_logout_redirect_uris");

            migrationBuilder.RenameColumn(
                name: "JsonWebKeySet",
                table: "OpenIddictApplications",
                newName: "json_web_key_set");

            migrationBuilder.RenameColumn(
                name: "DisplayNames",
                table: "OpenIddictApplications",
                newName: "display_names");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "OpenIddictApplications",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "ConsentType",
                table: "OpenIddictApplications",
                newName: "consent_type");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictApplications",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "OpenIddictApplications",
                newName: "client_type");

            migrationBuilder.RenameColumn(
                name: "ClientSecret",
                table: "OpenIddictApplications",
                newName: "client_secret");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "OpenIddictApplications",
                newName: "client_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationType",
                table: "OpenIddictApplications",
                newName: "application_type");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictApplications_ClientId",
                table: "OpenIddictApplications",
                newName: "ix_open_iddict_applications_client_id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "AspNetUserTokens",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUserTokens",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                newName: "login_provider");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserTokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "AspNetUsers",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetUsers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "AspNetUsers",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                newName: "two_factor_enabled");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "AspNetUsers",
                newName: "security_stamp");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                newName: "phone_number_confirmed");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "AspNetUsers",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "AspNetUsers",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                newName: "normalized_user_name");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                newName: "normalized_email");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "AspNetUsers",
                newName: "lockout_end");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                newName: "lockout_enabled");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                newName: "email_confirmed");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "AspNetUsers",
                newName: "access_failed_count");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetUserRoles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserRoles",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "ix_asp_net_user_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserLogins",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "AspNetUserLogins",
                newName: "provider_display_name");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                newName: "provider_key");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                newName: "login_provider");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                newName: "ix_asp_net_user_logins_user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetUserClaims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserClaims",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "AspNetUserClaims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "AspNetUserClaims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                newName: "ix_asp_net_user_claims_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetRoles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetRoles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "AspNetRoles",
                newName: "normalized_name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "AspNetRoles",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetRoleClaims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetRoleClaims",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "AspNetRoleClaims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "AspNetRoleClaims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                newName: "ix_asp_net_role_claims_role_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "categories",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "color",
                table: "categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_categories",
                table: "categories",
                columns: new[] { "user_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_domain_users",
                table: "domain_users",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens",
                column: "authorization_id",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications");

            migrationBuilder.DropPrimaryKey(
                name: "pk_categories",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_domain_users",
                table: "domain_users");

            migrationBuilder.RenameTable(
                name: "domain_users",
                newName: "users");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OpenIddictTokens",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "OpenIddictTokens",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OpenIddictTokens",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictTokens",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "payload",
                table: "OpenIddictTokens",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictTokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reference_id",
                table: "OpenIddictTokens",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "redemption_date",
                table: "OpenIddictTokens",
                newName: "RedemptionDate");

            migrationBuilder.RenameColumn(
                name: "expiration_date",
                table: "OpenIddictTokens",
                newName: "ExpirationDate");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "OpenIddictTokens",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictTokens",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "authorization_id",
                table: "OpenIddictTokens",
                newName: "AuthorizationId");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "OpenIddictTokens",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_reference_id",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_AuthorizationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type");

            migrationBuilder.RenameColumn(
                name: "resources",
                table: "OpenIddictScopes",
                newName: "Resources");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictScopes",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "OpenIddictScopes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "descriptions",
                table: "OpenIddictScopes",
                newName: "Descriptions");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "OpenIddictScopes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictScopes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "display_names",
                table: "OpenIddictScopes",
                newName: "DisplayNames");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "OpenIddictScopes",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictScopes",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_scopes_name",
                table: "OpenIddictScopes",
                newName: "IX_OpenIddictScopes_Name");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OpenIddictAuthorizations",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "OpenIddictAuthorizations",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OpenIddictAuthorizations",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scopes",
                table: "OpenIddictAuthorizations",
                newName: "Scopes");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictAuthorizations",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictAuthorizations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "OpenIddictAuthorizations",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictAuthorizations",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "OpenIddictAuthorizations",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                table: "OpenIddictAuthorizations",
                newName: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type");

            migrationBuilder.RenameColumn(
                name: "settings",
                table: "OpenIddictApplications",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "requirements",
                table: "OpenIddictApplications",
                newName: "Requirements");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictApplications",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "permissions",
                table: "OpenIddictApplications",
                newName: "Permissions");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictApplications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "redirect_uris",
                table: "OpenIddictApplications",
                newName: "RedirectUris");

            migrationBuilder.RenameColumn(
                name: "post_logout_redirect_uris",
                table: "OpenIddictApplications",
                newName: "PostLogoutRedirectUris");

            migrationBuilder.RenameColumn(
                name: "json_web_key_set",
                table: "OpenIddictApplications",
                newName: "JsonWebKeySet");

            migrationBuilder.RenameColumn(
                name: "display_names",
                table: "OpenIddictApplications",
                newName: "DisplayNames");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "OpenIddictApplications",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "consent_type",
                table: "OpenIddictApplications",
                newName: "ConsentType");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictApplications",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "client_type",
                table: "OpenIddictApplications",
                newName: "ClientType");

            migrationBuilder.RenameColumn(
                name: "client_secret",
                table: "OpenIddictApplications",
                newName: "ClientSecret");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "OpenIddictApplications",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "application_type",
                table: "OpenIddictApplications",
                newName: "ApplicationType");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_applications_client_id",
                table: "OpenIddictApplications",
                newName: "IX_OpenIddictApplications_ClientId");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "AspNetUserTokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetUserTokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "AspNetUserTokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "AspNetUsers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUsers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "AspNetUsers",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "two_factor_enabled",
                table: "AspNetUsers",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "security_stamp",
                table: "AspNetUsers",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "phone_number_confirmed",
                table: "AspNetUsers",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "AspNetUsers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "AspNetUsers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "normalized_user_name",
                table: "AspNetUsers",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "normalized_email",
                table: "AspNetUsers",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "lockout_end",
                table: "AspNetUsers",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "lockout_enabled",
                table: "AspNetUsers",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "email_confirmed",
                table: "AspNetUsers",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "AspNetUsers",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "access_failed_count",
                table: "AspNetUsers",
                newName: "AccessFailedCount");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AspNetUserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserLogins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "provider_display_name",
                table: "AspNetUserLogins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "provider_key",
                table: "AspNetUserLogins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "AspNetUserLogins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUserClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserClaims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "AspNetUserClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "AspNetUserClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "normalized_name",
                table: "AspNetRoles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "AspNetRoles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoleClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AspNetRoleClaims",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "AspNetRoleClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "AspNetRoleClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "categories",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "color",
                table: "categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictTokens",
                table: "OpenIddictTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictScopes",
                table: "OpenIddictScopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictAuthorizations",
                table: "OpenIddictAuthorizations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                table: "categories",
                columns: new[] { "user_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                table: "OpenIddictAuthorizations",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                table: "OpenIddictTokens",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "Id");
        }
    }
}
