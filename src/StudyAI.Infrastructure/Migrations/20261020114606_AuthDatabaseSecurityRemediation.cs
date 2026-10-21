using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuthDatabaseSecurityRemediation : Migration
    {
        private static readonly string[] ActiveSessionIndexColumns =
            ["user_id", "revoked_at", "absolute_expires_at"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $studyai$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM identity.users
                        GROUP BY upper(btrim(email))
                        HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION USING
                            ERRCODE = '23505',
                            MESSAGE = 'Normalized email collisions detected; resolve duplicate accounts before applying the security migration.';
                    END IF;
                END
                $studyai$;
                """);

            migrationBuilder.DropIndex(
                name: "ux_users_email_normalized",
                schema: "identity",
                table: "users");

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX ux_users_normalized_email
                ON identity.users (upper(btrim(email)));
                """);

            migrationBuilder.Sql(
                """
                DO $studyai$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM ownership.data_space_memberships
                        WHERE role NOT IN ('owner', 'member', 'viewer')
                    ) THEN
                        RAISE EXCEPTION USING
                            ERRCODE = '23514',
                            MESSAGE = 'Invalid Data Space membership roles detected; resolve invalid rows before applying the security migration.';
                    END IF;
                END
                $studyai$;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "ck_data_space_memberships_role",
                schema: "ownership",
                table: "data_space_memberships",
                sql: "role IN ('owner', 'member', 'viewer')");

            migrationBuilder.Sql(
                """
                ALTER TABLE ownership.data_space_memberships ENABLE ROW LEVEL SECURITY;
                ALTER TABLE ownership.data_space_memberships FORCE ROW LEVEL SECURITY;
                DROP POLICY IF EXISTS actor_membership_policy ON ownership.data_space_memberships;
                DROP POLICY IF EXISTS actor_membership_isolation_policy ON ownership.data_space_memberships;
                CREATE POLICY actor_membership_isolation_policy
                ON ownership.data_space_memberships
                FOR ALL TO studyai_app
                USING (user_id = NULLIF(current_setting('app.current_actor_id', true), '')::uuid)
                WITH CHECK (user_id = NULLIF(current_setting('app.current_actor_id', true), '')::uuid);
                """);

            migrationBuilder.CreateTable(
                name: "user_sessions",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_identifier_hash = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    security_stamp_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    absolute_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_active_expiry",
                schema: "identity",
                table: "user_sessions",
                columns: ActiveSessionIndexColumns);

            migrationBuilder.CreateIndex(
                name: "ux_user_sessions_identifier_hash",
                schema: "identity",
                table: "user_sessions",
                column: "session_identifier_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_sessions",
                schema: "identity");

            migrationBuilder.DropCheckConstraint(
                name: "ck_data_space_memberships_role",
                schema: "ownership",
                table: "data_space_memberships");

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS identity.ux_users_normalized_email;
                """);

            migrationBuilder.CreateIndex(
                name: "ux_users_email_normalized",
                schema: "identity",
                table: "users",
                column: "email",
                unique: true);
        }
    }
}
