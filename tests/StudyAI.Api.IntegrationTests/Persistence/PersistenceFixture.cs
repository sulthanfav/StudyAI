using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using StudyAI.Domain.Ai;
using StudyAI.Domain.Documents;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Knowledge;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;
using StudyAI.Infrastructure.Persistence;
using StudyAI.Infrastructure.Persistence.Interceptors;

namespace StudyAI.Api.IntegrationTests.Persistence;

public static class PersistenceFixture
{
    public static string AdminConnectionString => GetRequiredConnectionString("STUDYAI_TEST_ADMIN_CONNECTION_STRING");

    public static string AppConnectionString => GetRequiredConnectionString("STUDYAI_TEST_APP_CONNECTION_STRING");

    public static readonly Guid UserAId = Guid.Parse("0192a000-0000-7000-8000-000000000001");
    public static readonly Guid UserBId = Guid.Parse("0192a000-0000-7000-8000-000000000002");

    public static readonly Guid SpaceAId = Guid.Parse("0192a000-0000-7000-8000-0000000000aa");
    public static readonly Guid SpaceBId = Guid.Parse("0192a000-0000-7000-8000-0000000000bb");

    public static readonly Guid ContentSpaceAId = Guid.Parse("0192a000-0000-7000-8000-000000000011");
    public static readonly Guid ContentSpaceBId = Guid.Parse("0192a000-0000-7000-8000-000000000022");

    public static readonly Guid EmbeddingModelId = Guid.Parse("0192a000-0000-7000-8000-000000000099");

    public static readonly Guid KnowledgeItemAId = Guid.Parse("0192a000-0000-7000-8000-0000000000a1");
    public static readonly Guid KnowledgeItemBId = Guid.Parse("0192a000-0000-7000-8000-0000000000b1");

    public static readonly Guid StorageAId = Guid.Parse("0192a000-0000-7000-8000-0000000000a2");
    public static readonly Guid StorageBId = Guid.Parse("0192a000-0000-7000-8000-0000000000b2");

    public static readonly Guid VersionAId = Guid.Parse("0192a000-0000-7000-8000-0000000000a3");
    public static readonly Guid VersionBId = Guid.Parse("0192a000-0000-7000-8000-0000000000b3");

    public static readonly Guid RunAId = Guid.Parse("0192a000-0000-7000-8000-0000000000a4");
    public static readonly Guid RunBId = Guid.Parse("0192a000-0000-7000-8000-0000000000b4");

    public static readonly Guid ChunkAId = Guid.Parse("0192a000-0000-7000-8000-0000000000a5");
    public static readonly Guid ChunkBId = Guid.Parse("0192a000-0000-7000-8000-0000000000b5");

    private static readonly SemaphoreSlim InitializationGate = new(1, 1);
    private static bool _initialized;

    private static string GetRequiredConnectionString(string variableName) =>
        Environment.GetEnvironmentVariable(variableName)
        ?? throw new InvalidOperationException($"{variableName} is required to run PostgreSQL integration tests.");

    public static StudyAIDbContext CreateAdminContext()
    {
        var options = new DbContextOptionsBuilder<StudyAIDbContext>()
            .UseNpgsql(AdminConnectionString, o =>
            {
                o.UseVector();
                o.MigrationsHistoryTable("__ef_migrations_history", "ops");
            })
            .Options;

        return new StudyAIDbContext(options);
    }

    public static StudyAIDbContext CreateAppContext(CurrentDataSpaceAccessor? accessor = null)
    {
        var contextAccessor = accessor ?? new CurrentDataSpaceAccessor();
        var interceptor = new TenantSessionInterceptor(contextAccessor, contextAccessor);
        var options = new DbContextOptionsBuilder<StudyAIDbContext>()
            .UseNpgsql(AppConnectionString, o =>
            {
                o.UseVector();
                o.MigrationsHistoryTable("__ef_migrations_history", "ops");
            })
            .AddInterceptors(interceptor)
            .Options;

        return new StudyAIDbContext(options);
    }

    public static async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await InitializationGate.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            await using var adminContext = CreateAdminContext();

        // 1. Ensure Users
        if (!await adminContext.Users.AnyAsync(u => u.Id == UserAId, cancellationToken))
        {
            adminContext.Users.Add(new User
            {
                Id = UserAId,
                Email = "user.a@studyai.test",
                DisplayName = "User A",
                Status = "active",
                Locale = "id-ID",
                Timezone = "Asia/Jakarta",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await adminContext.Users.AnyAsync(u => u.Id == UserBId, cancellationToken))
        {
            adminContext.Users.Add(new User
            {
                Id = UserBId,
                Email = "user.b@studyai.test",
                DisplayName = "User B",
                Status = "active",
                Locale = "id-ID",
                Timezone = "Asia/Jakarta",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        await adminContext.SaveChangesAsync(cancellationToken);

        // 2. Ensure Data Spaces
        if (!await adminContext.DataSpaces.AnyAsync(s => s.Id == SpaceAId, cancellationToken))
        {
            adminContext.DataSpaces.Add(new DataSpace
            {
                Id = SpaceAId,
                OwnerUserId = UserAId,
                Kind = "personal",
                Name = "Personal Space A",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await adminContext.DataSpaces.AnyAsync(s => s.Id == SpaceBId, cancellationToken))
        {
            adminContext.DataSpaces.Add(new DataSpace
            {
                Id = SpaceBId,
                OwnerUserId = UserBId,
                Kind = "personal",
                Name = "Personal Space B",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        await adminContext.SaveChangesAsync(cancellationToken);

        // 3. Ensure Content Spaces
        if (!await adminContext.ContentSpaces.AnyAsync(c => c.Id == ContentSpaceAId, cancellationToken))
        {
            adminContext.ContentSpaces.Add(new ContentSpace
            {
                Id = ContentSpaceAId,
                DataSpaceId = SpaceAId,
                Kind = "library",
                Title = "Library Space A",
                Position = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await adminContext.ContentSpaces.AnyAsync(c => c.Id == ContentSpaceBId, cancellationToken))
        {
            adminContext.ContentSpaces.Add(new ContentSpace
            {
                Id = ContentSpaceBId,
                DataSpaceId = SpaceBId,
                Kind = "library",
                Title = "Library Space B",
                Position = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        await adminContext.SaveChangesAsync(cancellationToken);

        // 4. Ensure Embedding Model
        if (!await adminContext.EmbeddingModels.AnyAsync(m => m.Id == EmbeddingModelId, cancellationToken))
        {
            adminContext.EmbeddingModels.Add(new EmbeddingModel
            {
                Id = EmbeddingModelId,
                Provider = "local",
                ModelKey = "test-embedding-v1",
                Dimensions = 3,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await adminContext.SaveChangesAsync(cancellationToken);
        }

        // 5. Ensure Seed Knowledge Items and Documents
        if (!await adminContext.KnowledgeItems.AnyAsync(k => k.Id == KnowledgeItemAId, cancellationToken))
        {
            adminContext.KnowledgeItems.Add(new KnowledgeItem
            {
                Id = KnowledgeItemAId,
                DataSpaceId = SpaceAId,
                Kind = "document",
                Title = "Document Space A",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            adminContext.StorageObjects.Add(new StorageObject
            {
                Id = StorageAId,
                DataSpaceId = SpaceAId,
                StorageKey = "spaces/a/doc_a.pdf",
                ChecksumSha256 = new string('a', 64),
                SizeBytes = 1024,
                MimeType = "application/pdf",
                ChargeClass = "billable",
                State = "verified",
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.Documents.Add(new Document
            {
                KnowledgeItemId = KnowledgeItemAId,
                DataSpaceId = SpaceAId,
                ItemKind = "document",
                OriginalFilename = "doc_a.pdf",
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.DocumentVersions.Add(new DocumentVersion
            {
                Id = VersionAId,
                DataSpaceId = SpaceAId,
                DocumentId = KnowledgeItemAId,
                VersionNo = 1,
                StorageObjectId = StorageAId,
                PageCount = 5,
                ProcessingStatus = "ready",
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.ProcessingRuns.Add(new ProcessingRun
            {
                Id = RunAId,
                DataSpaceId = SpaceAId,
                DocumentVersionId = VersionAId,
                ProcessorVersion = "v1",
                Status = "ready",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.DocumentChunks.Add(new DocumentChunk
            {
                Id = ChunkAId,
                DataSpaceId = SpaceAId,
                DocumentVersionId = VersionAId,
                ProcessingRunId = RunAId,
                ChunkIndex = 0,
                ContentText = "Introduction to StudyAI in Space A",
                LocatorJson = "{}",
                TokenCount = 7
            });

            adminContext.ChunkEmbeddings.Add(new ChunkEmbedding
            {
                ChunkId = ChunkAId,
                DataSpaceId = SpaceAId,
                EmbeddingModelId = EmbeddingModelId,
                Embedding = [0.1f, 0.2f, 0.3f],
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await adminContext.KnowledgeItems.AnyAsync(k => k.Id == KnowledgeItemBId, cancellationToken))
        {
            adminContext.KnowledgeItems.Add(new KnowledgeItem
            {
                Id = KnowledgeItemBId,
                DataSpaceId = SpaceBId,
                Kind = "document",
                Title = "Document Space B",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            adminContext.StorageObjects.Add(new StorageObject
            {
                Id = StorageBId,
                DataSpaceId = SpaceBId,
                StorageKey = "spaces/b/doc_b.pdf",
                ChecksumSha256 = new string('b', 64),
                SizeBytes = 2048,
                MimeType = "application/pdf",
                ChargeClass = "billable",
                State = "verified",
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.Documents.Add(new Document
            {
                KnowledgeItemId = KnowledgeItemBId,
                DataSpaceId = SpaceBId,
                ItemKind = "document",
                OriginalFilename = "doc_b.pdf",
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.DocumentVersions.Add(new DocumentVersion
            {
                Id = VersionBId,
                DataSpaceId = SpaceBId,
                DocumentId = KnowledgeItemBId,
                VersionNo = 1,
                StorageObjectId = StorageBId,
                PageCount = 10,
                ProcessingStatus = "ready",
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.ProcessingRuns.Add(new ProcessingRun
            {
                Id = RunBId,
                DataSpaceId = SpaceBId,
                DocumentVersionId = VersionBId,
                ProcessorVersion = "v1",
                Status = "ready",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

            adminContext.DocumentChunks.Add(new DocumentChunk
            {
                Id = ChunkBId,
                DataSpaceId = SpaceBId,
                DocumentVersionId = VersionBId,
                ProcessingRunId = RunBId,
                ChunkIndex = 0,
                ContentText = "Introduction to StudyAI in Space B",
                LocatorJson = "{}",
                TokenCount = 7
            });

            adminContext.ChunkEmbeddings.Add(new ChunkEmbedding
            {
                ChunkId = ChunkBId,
                DataSpaceId = SpaceBId,
                EmbeddingModelId = EmbeddingModelId,
                Embedding = [0.4f, 0.5f, 0.6f],
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await adminContext.SaveChangesAsync(cancellationToken);

            _initialized = true;
        }
        finally
        {
            InitializationGate.Release();
        }
    }
}
