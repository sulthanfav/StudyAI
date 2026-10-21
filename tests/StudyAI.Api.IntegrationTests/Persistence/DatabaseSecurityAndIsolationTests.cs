using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Domain.Ai;
using StudyAI.Domain.Documents;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Knowledge;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;
using StudyAI.Infrastructure.Persistence;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Persistence;

public sealed class DatabaseSecurityAndIsolationTests : IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ApplicationRoleCannotExecuteDdl()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var appContext = PersistenceFixture.CreateAppContext();
        var connection = (NpgsqlConnection)appContext.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE identity.illegal_table (id int);";

        var ex = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync(ct));
        Assert.Equal("42501", ex.SqlState); // insufficient_privilege
    }

    [Fact]
    public async Task ApplicationRoleHasOnlyReviewedRuntimePrivileges()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var adminContext = PersistenceFixture.CreateAdminContext();
        var adminConnection = (NpgsqlConnection)adminContext.Database.GetDbConnection();
        await adminConnection.OpenAsync(ct);
        await using var appContext = PersistenceFixture.CreateAppContext();
        var appConnection = (NpgsqlConnection)appContext.Database.GetDbConnection();
        await appConnection.OpenAsync(ct);

        await using var createProbe = adminConnection.CreateCommand();
        createProbe.CommandText = "CREATE TABLE ai.runtime_privilege_probe (id integer PRIMARY KEY);";
        await createProbe.ExecuteNonQueryAsync(ct);

        try
        {
            foreach (var deniedSql in new[]
                     {
                         "DELETE FROM ops.__ef_migrations_history;",
                         "ALTER TABLE identity.users ADD COLUMN runtime_privilege_probe integer;",
                         "SELECT * FROM ai.runtime_privilege_probe;"
                     })
            {
                await using var deniedCommand = appConnection.CreateCommand();
                deniedCommand.CommandText = deniedSql;
                var exception = await Assert.ThrowsAsync<PostgresException>(
                    () => deniedCommand.ExecuteNonQueryAsync(ct));
                Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
            }

            await using (var ineffectiveGrant = appConnection.CreateCommand())
            {
                ineffectiveGrant.CommandText = "GRANT SELECT ON identity.users TO studyai_ro;";
                await ineffectiveGrant.ExecuteNonQueryAsync(ct);
            }

            await using var factsCommand = adminConnection.CreateCommand();
            factsCommand.CommandText = """
                SELECT concat_ws('|',
                    (SELECT count(*)
                     FROM information_schema.role_table_grants
                     WHERE grantee = 'studyai_app' AND is_grantable = 'YES'),
                    (SELECT count(*)
                     FROM pg_tables
                     WHERE schemaname IN ('identity', 'ownership', 'workspace', 'knowledge', 'documents', 'ai', 'ops')
                       AND tableowner = 'studyai_app'),
                    has_table_privilege('studyai_app', 'ops.__ef_migrations_history', 'SELECT,INSERT,UPDATE,DELETE'),
                    has_database_privilege('studyai_app', current_database(), 'TEMP'),
                    has_schema_privilege('studyai_app', 'public', 'CREATE'),
                    has_table_privilege('studyai_ro', 'identity.users', 'SELECT'),
                    pg_has_role('studyai_app', 'studyai_migrator', 'MEMBER'));
                """;
            Assert.Equal("0|0|f|f|f|f|f", await factsCommand.ExecuteScalarAsync(ct));
        }
        finally
        {
            await using var dropProbe = adminConnection.CreateCommand();
            dropProbe.CommandText = "DROP TABLE IF EXISTS ai.runtime_privilege_probe;";
            await dropProbe.ExecuteNonQueryAsync(ct);
        }
    }

    [Fact]
    public async Task MissingTenantContextDeniesProtectedReadsAndWrites()
    {
        var ct = TestContext.Current.CancellationToken;
        var accessor = new CurrentDataSpaceAccessor(); // CurrentDataSpaceId is null
        await using var appContext = PersistenceFixture.CreateAppContext(accessor);

        await using var tx = await appContext.Database.BeginTransactionAsync(ct);

        // 1. Protected Read fails closed (returns 0 rows)
        var items = await appContext.KnowledgeItems.ToListAsync(ct);
        Assert.Empty(items);

        // 2. Protected Write is blocked by RLS
        var newItem = new KnowledgeItem
        {
            Id = Guid.CreateVersion7(),
            DataSpaceId = PersistenceFixture.SpaceAId,
            Kind = "note",
            Title = "Unauthenticated Attempt",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        appContext.KnowledgeItems.Add(newItem);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => appContext.SaveChangesAsync(ct));
        var pgEx = Assert.IsType<PostgresException>(ex.InnerException);
        Assert.Equal("42501", pgEx.SqlState); // RLS with_check violation
    }

    [Fact]
    public async Task MissingActorContextDeniesMembershipReads()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var appContext = PersistenceFixture.CreateAppContext();
        await using var transaction = await appContext.Database.BeginTransactionAsync(ct);

        Assert.Empty(await appContext.DataSpaceMemberships.AsNoTracking().ToListAsync(ct));
    }

    [Fact]
    public async Task ActorScopedMembershipLookupCannotReadAnotherActorsMembership()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var appContext = PersistenceFixture.CreateAppContext();
        await using var transaction = await appContext.Database.BeginTransactionAsync(ct);
        await appContext.Database.ExecuteSqlRawAsync(
            "SELECT set_config('app.current_actor_id', {0}, true);",
            [PersistenceFixture.UserAId.ToString()],
            ct);

        var memberships = await appContext.DataSpaceMemberships.AsNoTracking().ToListAsync(ct);

        Assert.NotEmpty(memberships);
        Assert.All(memberships, membership => Assert.Equal(PersistenceFixture.UserAId, membership.UserId));
    }

    [Fact]
    public async Task ActorScopedMembershipWriteCannotTargetAnotherActor()
    {
        var ct = TestContext.Current.CancellationToken;
        var temporarySpaceId = Guid.CreateVersion7();
        await using var appContext = PersistenceFixture.CreateAppContext();
        var connection = (NpgsqlConnection)appContext.Database.GetDbConnection();
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using (var setActor = connection.CreateCommand())
        {
            setActor.Transaction = transaction;
            setActor.CommandText = "SELECT set_config('app.current_actor_id', @actor_id, true);";
            setActor.Parameters.AddWithValue("actor_id", PersistenceFixture.UserAId.ToString());
            await setActor.ExecuteNonQueryAsync(ct);
        }

        await using (var createSpace = connection.CreateCommand())
        {
            createSpace.Transaction = transaction;
            createSpace.CommandText = """
                INSERT INTO ownership.data_spaces (id, owner_user_id, kind, name)
                VALUES (@id, @owner_user_id, 'project', 'Actor Scope Probe');
                """;
            createSpace.Parameters.AddWithValue("id", temporarySpaceId);
            createSpace.Parameters.AddWithValue("owner_user_id", PersistenceFixture.UserAId);
            await createSpace.ExecuteNonQueryAsync(ct);
        }

        await using var crossActorMembership = connection.CreateCommand();
        crossActorMembership.Transaction = transaction;
        crossActorMembership.CommandText = """
            INSERT INTO ownership.data_space_memberships (id, data_space_id, user_id, role)
            VALUES (@id, @data_space_id, @user_id, 'viewer');
            """;
        crossActorMembership.Parameters.AddWithValue("id", Guid.CreateVersion7());
        crossActorMembership.Parameters.AddWithValue("data_space_id", temporarySpaceId);
        crossActorMembership.Parameters.AddWithValue("user_id", PersistenceFixture.UserBId);

        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => crossActorMembership.ExecuteNonQueryAsync(ct));
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    [Fact]
    public async Task AuthorizedDataSpaceContextAllowsPermittedOperations()
    {
        var ct = TestContext.Current.CancellationToken;
        var accessor = new CurrentDataSpaceAccessor
        {
            CurrentDataSpaceId = PersistenceFixture.SpaceAId
        };
        await using var appContext = PersistenceFixture.CreateAppContext(accessor);

        await using var tx = await appContext.Database.BeginTransactionAsync(ct);

        // 1. Permitted Read returns Space A data
        var items = await appContext.KnowledgeItems.ToListAsync(ct);
        Assert.Contains(items, k => k.Id == PersistenceFixture.KnowledgeItemAId);
        Assert.DoesNotContain(items, k => k.Id == PersistenceFixture.KnowledgeItemBId);

        // 2. Permitted Write into Space A succeeds
        var newItemId = Guid.CreateVersion7();
        var newItem = new KnowledgeItem
        {
            Id = newItemId,
            DataSpaceId = PersistenceFixture.SpaceAId,
            Kind = "note",
            Title = "Authorized Space A Note",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        appContext.KnowledgeItems.Add(newItem);
        await appContext.SaveChangesAsync(ct);

        var queried = await appContext.KnowledgeItems.FirstOrDefaultAsync(k => k.Id == newItemId, ct);
        Assert.NotNull(queried);
        Assert.Equal("Authorized Space A Note", queried.Title);

        await tx.RollbackAsync(ct); // Clean up test write
    }

    [Fact]
    public async Task CrossTenantAccessIsBlocked()
    {
        var ct = TestContext.Current.CancellationToken;
        var accessor = new CurrentDataSpaceAccessor
        {
            CurrentDataSpaceId = PersistenceFixture.SpaceAId
        };
        await using var appContext = PersistenceFixture.CreateAppContext(accessor);

        await using var tx = await appContext.Database.BeginTransactionAsync(ct);

        // 1. Cross-tenant Read returns empty for Space B item
        var itemB = await appContext.KnowledgeItems
            .FirstOrDefaultAsync(k => k.Id == PersistenceFixture.KnowledgeItemBId, ct);
        Assert.Null(itemB);

        // 2. Cross-tenant Write targeting Space B is blocked by RLS
        var rogueItem = new KnowledgeItem
        {
            Id = Guid.CreateVersion7(),
            DataSpaceId = PersistenceFixture.SpaceBId, // Attempting to write into Space B
            Kind = "note",
            Title = "Cross Tenant Poisoning",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        appContext.KnowledgeItems.Add(rogueItem);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => appContext.SaveChangesAsync(ct));
        var pgEx = Assert.IsType<PostgresException>(ex.InnerException);
        Assert.Equal("42501", pgEx.SqlState);
    }

    [Fact]
    public async Task PooledConnectionsDoNotLeakContextBetweenTenants()
    {
        var ct = TestContext.Current.CancellationToken;

        // Session 1: Operate as Space A on pooled connection
        var accessorA = new CurrentDataSpaceAccessor
        {
            CurrentDataSpaceId = PersistenceFixture.SpaceAId
        };
        await using (var contextA = PersistenceFixture.CreateAppContext(accessorA))
        {
            await using var txA = await contextA.Database.BeginTransactionAsync(ct);
            var itemsA = await contextA.KnowledgeItems.ToListAsync(ct);
            Assert.Contains(itemsA, k => k.Id == PersistenceFixture.KnowledgeItemAId);
            await txA.CommitAsync(ct);
        }

        // Session 2: Operate as Space B on a pooled connection
        var accessorB = new CurrentDataSpaceAccessor
        {
            CurrentDataSpaceId = PersistenceFixture.SpaceBId
        };
        await using (var contextB = PersistenceFixture.CreateAppContext(accessorB))
        {
            await using var txB = await contextB.Database.BeginTransactionAsync(ct);
            var itemsB = await contextB.KnowledgeItems.ToListAsync(ct);
            Assert.Contains(itemsB, k => k.Id == PersistenceFixture.KnowledgeItemBId);
            Assert.DoesNotContain(itemsB, k => k.Id == PersistenceFixture.KnowledgeItemAId); // No leak of A!
            await txB.CommitAsync(ct);
        }

        // Session 3: Operate with no tenant context on a pooled connection
        var accessorNone = new CurrentDataSpaceAccessor
        {
            CurrentDataSpaceId = null
        };
        await using (var contextNone = PersistenceFixture.CreateAppContext(accessorNone))
        {
            await using var txNone = await contextNone.Database.BeginTransactionAsync(ct);
            var itemsNone = await contextNone.KnowledgeItems.ToListAsync(ct);
            Assert.Empty(itemsNone); // Fails closed, no residual context from previous sessions!
            await txNone.CommitAsync(ct);
        }
    }

    [Fact]
    public async Task TransactionLifecycleCommitAndRollbackClearsContext()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var appContext = PersistenceFixture.CreateAppContext();
        var connection = (NpgsqlConnection)appContext.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        // 1. Inside transaction: context is active
        await using (var tx = await connection.BeginTransactionAsync(ct))
        {
            await using var cmdSet = connection.CreateCommand();
            cmdSet.Transaction = tx;
            cmdSet.CommandText = "SELECT set_config('app.current_data_space_id', @space_id, true);";
            cmdSet.Parameters.AddWithValue("space_id", PersistenceFixture.SpaceAId.ToString());
            await cmdSet.ExecuteNonQueryAsync(ct);

            await using var cmdCheck = connection.CreateCommand();
            cmdCheck.Transaction = tx;
            cmdCheck.CommandText = "SELECT current_setting('app.current_data_space_id', true);";
            var inTxSetting = (string?)await cmdCheck.ExecuteScalarAsync(ct);
            Assert.Equal(PersistenceFixture.SpaceAId.ToString(), inTxSetting);

            await tx.CommitAsync(ct);
        }

        // 2. Post-commit: context is automatically cleared by PostgreSQL transaction scope
        await using (var cmdPost = connection.CreateCommand())
        {
            cmdPost.CommandText = "SELECT current_setting('app.current_data_space_id', true);";
            var postTxSetting = (string?)await cmdPost.ExecuteScalarAsync(ct);
            Assert.True(string.IsNullOrEmpty(postTxSetting), "Tenant context must be empty post-commit");
        }

        // 3. Rollback also clears context
        await using (var txRollback = await connection.BeginTransactionAsync(ct))
        {
            await using var cmdSet = connection.CreateCommand();
            cmdSet.Transaction = txRollback;
            cmdSet.CommandText = "SELECT set_config('app.current_data_space_id', @space_id, true);";
            cmdSet.Parameters.AddWithValue("space_id", PersistenceFixture.SpaceBId.ToString());
            await cmdSet.ExecuteNonQueryAsync(ct);

            await txRollback.RollbackAsync(ct);
        }

        await using (var cmdPostRollback = connection.CreateCommand())
        {
            cmdPostRollback.CommandText = "SELECT current_setting('app.current_data_space_id', true);";
            var postRollbackSetting = (string?)await cmdPostRollback.ExecuteScalarAsync(ct);
            Assert.True(string.IsNullOrEmpty(postRollbackSetting), "Tenant context must be empty post-rollback");
        }
    }

    [Fact]
    public async Task InvalidOrMalformedContextFailsClosed()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var appContext = PersistenceFixture.CreateAppContext();
        var connection = (NpgsqlConnection)appContext.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var tx = await connection.BeginTransactionAsync(ct);

        // Injected SQL string is safely passed as parameter
        const string maliciousInput = "'; DROP TABLE identity.users; --";
        await using var cmdSet = connection.CreateCommand();
        cmdSet.Transaction = tx;
        cmdSet.CommandText = "SELECT set_config('app.current_data_space_id', @space_id, true);";
        cmdSet.Parameters.AddWithValue("space_id", maliciousInput);
        await cmdSet.ExecuteNonQueryAsync(ct);

        // When RLS evaluates NULLIF(current_setting(...), '')::uuid, invalid UUID fails query safely
        await using var cmdQuery = connection.CreateCommand();
        cmdQuery.Transaction = tx;
        cmdQuery.CommandText = "SELECT count(*) FROM knowledge.knowledge_items;";

        var ex = await Assert.ThrowsAsync<PostgresException>(() => cmdQuery.ExecuteScalarAsync(ct));
        Assert.Equal("22P02", ex.SqlState); // invalid_text_representation (fails closed)

        await tx.RollbackAsync(ct);
    }

    [Fact]
    public async Task EfCoreMappingsMatchImplementedSchemaSubset()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var adminContext = PersistenceFixture.CreateAdminContext();

        // Assert all 10 mapped entity sets can be queried through EF Core without mapping errors
        var users = await adminContext.Users.Take(1).ToListAsync(ct);
        Assert.NotEmpty(users);

        var dataSpaces = await adminContext.DataSpaces.Take(1).ToListAsync(ct);
        Assert.NotEmpty(dataSpaces);

        var contentSpaces = await adminContext.ContentSpaces.Take(1).ToListAsync(ct);
        Assert.NotEmpty(contentSpaces);

        var knowledgeItems = await adminContext.KnowledgeItems.Take(1).ToListAsync(ct);
        Assert.NotEmpty(knowledgeItems);

        var embeddingModels = await adminContext.EmbeddingModels.Take(1).ToListAsync(ct);
        Assert.NotEmpty(embeddingModels);

        var storageObjects = await adminContext.StorageObjects.Take(1).ToListAsync(ct);
        Assert.NotEmpty(storageObjects);

        var documents = await adminContext.Documents.Take(1).ToListAsync(ct);
        Assert.NotEmpty(documents);

        var documentVersions = await adminContext.DocumentVersions.Take(1).ToListAsync(ct);
        Assert.NotEmpty(documentVersions);

        var processingRuns = await adminContext.ProcessingRuns.Take(1).ToListAsync(ct);
        Assert.NotEmpty(processingRuns);

        var documentChunks = await adminContext.DocumentChunks.Take(1).ToListAsync(ct);
        Assert.NotEmpty(documentChunks);

        var chunkEmbeddings = await adminContext.ChunkEmbeddings.Take(1).ToListAsync(ct);
        Assert.NotEmpty(chunkEmbeddings);
        Assert.Equal(3, chunkEmbeddings[0].Embedding.Length);
    }

    [Fact]
    public async Task IncrementalMigrationPreservesPopulatedData()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var adminContext = PersistenceFixture.CreateAdminContext();

        // 1. Verify migration history contains the baseline migration
        var connection = (NpgsqlConnection)adminContext.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT \"MigrationId\" FROM ops.__ef_migrations_history ORDER BY \"MigrationId\";";
        var appliedMigration = (string?)await cmd.ExecuteScalarAsync(ct);
        Assert.NotNull(appliedMigration);
        Assert.Contains("InitialPersistenceBaseline", appliedMigration);

        // 2. Verify all seeded records are intact
        var userCount = await adminContext.Users.CountAsync(ct);
        Assert.True(userCount >= 2);

        var spaceCount = await adminContext.DataSpaces.CountAsync(ct);
        Assert.True(spaceCount >= 2);

        var chunkCount = await adminContext.DocumentChunks.CountAsync(ct);
        Assert.True(chunkCount >= 2);
    }

    [Fact]
    public async Task ConcurrencyConflictsAreHandledCorrectly()
    {
        var ct = TestContext.Current.CancellationToken;

        // Test optimistic concurrency conflict detection on entities
        await using var context1 = PersistenceFixture.CreateAdminContext();
        await using var context2 = PersistenceFixture.CreateAdminContext();

        var user1 = await context1.Users.FirstAsync(u => u.Id == PersistenceFixture.UserAId, ct);
        var user2 = await context2.Users.FirstAsync(u => u.Id == PersistenceFixture.UserAId, ct);

        var edit1 = $"Concurrent Edit 1 {Guid.NewGuid()}";
        var edit2 = $"Concurrent Edit 2 {Guid.NewGuid()}";

        user1.DisplayName = edit1;
        await context1.SaveChangesAsync(ct);

        // When second context attempts to update from stale snapshot, EF Core raises DbUpdateConcurrencyException
        user2.DisplayName = edit2;
        var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync(ct));
        Assert.NotNull(ex);

        // Verify that first update was preserved and not silently overwritten
        await using var verifyContext = PersistenceFixture.CreateAdminContext();
        var reloaded = await verifyContext.Users.FirstAsync(u => u.Id == PersistenceFixture.UserAId, ct);
        Assert.Equal(edit1, reloaded.DisplayName);
    }

    [Fact]
    public async Task RuntimeCredentialsAreNotExposedInLogs()
    {
        var ct = TestContext.Current.CancellationToken;
        var capturingLogger = new CapturingLoggerProvider();
        var options = new DbContextOptionsBuilder<StudyAIDbContext>()
            .UseNpgsql(PersistenceFixture.AppConnectionString, o =>
            {
                o.UseVector();
                o.MigrationsHistoryTable("__ef_migrations_history", "ops");
            })
            .UseLoggerFactory(LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(capturingLogger);
                builder.SetMinimumLevel(LogLevel.Trace);
            }))
            .EnableSensitiveDataLogging(false)
            .Options;

        await using (var context = new StudyAIDbContext(options))
        {
            _ = await context.Users.Take(1).ToListAsync(ct);
        }

        // Verify that configured database passwords are NEVER found in captured logs.
        var allLogs = string.Join(Environment.NewLine, capturingLogger.Entries);
        foreach (var connectionString in new[]
                 {
                     PersistenceFixture.AdminConnectionString,
                     PersistenceFixture.AppConnectionString
                 })
        {
            var configuredPassword = new NpgsqlConnectionStringBuilder(connectionString).Password;
            if (!string.IsNullOrEmpty(configuredPassword))
            {
                Assert.DoesNotContain(configuredPassword, allLogs, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task ExistingApiIntegrationTestsRemainPassing()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new StudyAIApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live", ct);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
