using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using StudyAI.Api.IntegrationTests.Persistence;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Persistence;

public sealed class SessionModelMetadataTests
{
    [Fact]
    public void SessionModelStoresOnlyHashedIdentifiersAndSupportsRevocationLookups()
    {
        using var context = PersistenceFixture.CreateAdminContext();
        var session = context.Model.FindEntityType("StudyAI.Domain.Identity.UserSession");

        Assert.NotNull(session);
        Assert.Equal("user_sessions", session.GetTableName());
        Assert.Equal("identity", session.GetSchema());

        var hash = session.FindProperty("SessionIdentifierHash");
        Assert.NotNull(hash);
        Assert.Equal(typeof(byte[]), hash.ClrType);
        Assert.False(hash.IsNullable);
        Assert.Equal(32, hash.GetMaxLength());
        Assert.Null(session.FindProperty("SessionIdentifier"));
        Assert.Null(session.FindProperty("Token"));

        Assert.NotNull(session.FindProperty("SecurityStampSnapshot"));
        Assert.NotNull(session.FindProperty("IssuedAt"));
        Assert.NotNull(session.FindProperty("AbsoluteExpiresAt"));
        Assert.NotNull(session.FindProperty("RevokedAt"));
        Assert.NotNull(session.FindProperty("LastSeenAt"));

        var indexes = session.GetIndexes().ToArray();
        Assert.Contains(indexes, index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["SessionIdentifierHash"]));
        Assert.Contains(indexes, index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["UserId", "RevokedAt", "AbsoluteExpiresAt"]));

        var userForeignKey = Assert.Single(session.GetForeignKeys());
        Assert.Equal("UserId", Assert.Single(userForeignKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, userForeignKey.DeleteBehavior);
    }

    [Fact]
    public void MembershipModelRestrictsRolesToTheApprovedVocabulary()
    {
        using var context = PersistenceFixture.CreateAdminContext();
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var membership = designTimeModel.FindEntityType("StudyAI.Domain.Ownership.DataSpaceMembership");

        Assert.NotNull(membership);
        var roleConstraint = Assert.Single(
            membership.GetCheckConstraints(),
            constraint => constraint.Name == "ck_data_space_memberships_role");
        Assert.Equal("role IN ('owner', 'member', 'viewer')", roleConstraint.Sql);
    }

    [Fact]
    public async Task ForwardMigrationCreatesOnlyHashedSessionIdentifierStorage()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = PersistenceFixture.CreateAdminContext();
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var columnsCommand = connection.CreateCommand();
        columnsCommand.CommandText = """
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'identity' AND table_name = 'user_sessions'
            ORDER BY ordinal_position;
            """;
        var columns = new List<string>();
        await using (var reader = await columnsCommand.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                columns.Add(reader.GetString(0));
            }
        }

        Assert.Contains("session_identifier_hash", columns);
        Assert.Contains("revoked_at", columns);
        Assert.Contains("absolute_expires_at", columns);
        Assert.DoesNotContain("session_identifier", columns);
        Assert.DoesNotContain("token", columns);

        await using var indexesCommand = connection.CreateCommand();
        indexesCommand.CommandText = """
            SELECT indexname
            FROM pg_indexes
            WHERE schemaname = 'identity' AND tablename = 'user_sessions';
            """;
        var indexes = new List<string>();
        await using (var reader = await indexesCommand.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                indexes.Add(reader.GetString(0));
            }
        }

        Assert.Contains("ux_user_sessions_identifier_hash", indexes);
        Assert.Contains("ix_user_sessions_user_active_expiry", indexes);
    }

    [Fact]
    public async Task ForwardMigrationCreatesAuthoritativeNormalizedEmailUniqueIndex()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = PersistenceFixture.CreateAdminContext();
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT indexdef
            FROM pg_indexes
            WHERE schemaname = 'identity'
              AND tablename = 'users'
              AND indexname = 'ux_users_normalized_email';
            """;

        var indexDefinition = Assert.IsType<string>(await command.ExecuteScalarAsync(ct));

        Assert.Contains("CREATE UNIQUE INDEX", indexDefinition, StringComparison.Ordinal);
        Assert.Contains("upper(btrim(email))", indexDefinition, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ForwardMigrationCreatesValidatedMembershipRoleConstraint()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = PersistenceFixture.CreateAdminContext();
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT convalidated
            FROM pg_constraint
            WHERE conname = 'ck_data_space_memberships_role'
              AND conrelid = 'ownership.data_space_memberships'::regclass;
            """;

        Assert.True(Assert.IsType<bool>(await command.ExecuteScalarAsync(ct)));
    }
}
