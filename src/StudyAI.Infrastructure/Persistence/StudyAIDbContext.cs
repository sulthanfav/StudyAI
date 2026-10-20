using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Common.Interfaces;
using StudyAI.Domain.Ai;
using StudyAI.Domain.Documents;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Knowledge;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;

namespace StudyAI.Infrastructure.Persistence;

public class StudyAIDbContext : DbContext, IApplicationDbContext
{
    public StudyAIDbContext(DbContextOptions<StudyAIDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DataSpace> DataSpaces => Set<DataSpace>();
    public DbSet<DataSpaceMembership> DataSpaceMemberships => Set<DataSpaceMembership>();
    public DbSet<ContentSpace> ContentSpaces => Set<ContentSpace>();
    public DbSet<KnowledgeItem> KnowledgeItems => Set<KnowledgeItem>();
    public DbSet<StorageObject> StorageObjects => Set<StorageObject>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<ProcessingRun> ProcessingRuns => Set<ProcessingRun>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<ChunkEmbedding> ChunkEmbeddings => Set<ChunkEmbedding>();
    public DbSet<EmbeddingModel> EmbeddingModels => Set<EmbeddingModel>();

    IQueryable<User> IApplicationDbContext.Users => Users;
    IQueryable<DataSpace> IApplicationDbContext.DataSpaces => DataSpaces;
    IQueryable<DataSpaceMembership> IApplicationDbContext.DataSpaceMemberships => DataSpaceMemberships;
    IQueryable<ContentSpace> IApplicationDbContext.ContentSpaces => ContentSpaces;
    IQueryable<KnowledgeItem> IApplicationDbContext.KnowledgeItems => KnowledgeItems;
    IQueryable<StorageObject> IApplicationDbContext.StorageObjects => StorageObjects;
    IQueryable<Document> IApplicationDbContext.Documents => Documents;
    IQueryable<DocumentVersion> IApplicationDbContext.DocumentVersions => DocumentVersions;
    IQueryable<ProcessingRun> IApplicationDbContext.ProcessingRuns => ProcessingRuns;
    IQueryable<DocumentChunk> IApplicationDbContext.DocumentChunks => DocumentChunks;
    IQueryable<ChunkEmbedding> IApplicationDbContext.ChunkEmbeddings => ChunkEmbeddings;
    IQueryable<EmbeddingModel> IApplicationDbContext.EmbeddingModels => EmbeddingModels;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyAIDbContext).Assembly);
    }

    public async Task SetTenantSessionContextAsync(Guid dataSpaceId, CancellationToken cancellationToken = default)
    {
        await Database.ExecuteSqlRawAsync(
            "SELECT set_config('app.current_data_space_id', {0}, true);",
            [dataSpaceId.ToString()],
            cancellationToken);
    }
}
