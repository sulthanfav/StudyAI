using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace StudyAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityAndMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "access_failed_count",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "concurrency_stamp",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "email_confirmed",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "lockout_enabled",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "lockout_end",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "normalized_email",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "normalized_user_name",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_stamp",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "data_space_memberships",
                schema: "ownership",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "owner"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_space_memberships", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_space_memberships_data_spaces_data_space_id",
                        column: x => x.data_space_id,
                        principalSchema: "ownership",
                        principalTable: "data_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_data_space_memberships_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_space_memberships_data_space_id_user_id",
                schema: "ownership",
                table: "data_space_memberships",
                columns: new[] { "data_space_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_space_memberships_user",
                schema: "ownership",
                table: "data_space_memberships",
                column: "user_id");

            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS identity.ux_users_email_normalized;
CREATE UNIQUE INDEX ux_users_email_normalized ON identity.users (lower(email));

ALTER TABLE ownership.data_space_memberships ENABLE ROW LEVEL SECURITY;
ALTER TABLE ownership.data_space_memberships FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS actor_membership_policy ON ownership.data_space_memberships;
CREATE POLICY actor_membership_policy ON ownership.data_space_memberships
    FOR ALL TO studyai_app
    USING (user_id = NULLIF(current_setting('app.current_actor_id', true), '')::uuid)
    WITH CHECK (user_id = NULLIF(current_setting('app.current_actor_id', true), '')::uuid);

GRANT SELECT, INSERT, UPDATE, DELETE ON ownership.data_space_memberships TO studyai_app;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_space_memberships",
                schema: "ownership");

            migrationBuilder.DropColumn(
                name: "access_failed_count",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email_confirmed",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_enabled",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_end",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "normalized_email",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "normalized_user_name",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_hash",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "security_stamp",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "user_name",
                schema: "identity",
                table: "users");
        }
    }
}
