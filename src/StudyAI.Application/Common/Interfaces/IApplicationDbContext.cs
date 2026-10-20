using StudyAI.Domain.Ai;
using StudyAI.Domain.Documents;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Knowledge;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;

namespace StudyAI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<DataSpace> DataSpaces { get; }
    IQueryable<DataSpaceMembership> DataSpaceMemberships { get; }
    IQueryable<ContentSpace> ContentSpaces { get; }
    IQueryable<KnowledgeItem> KnowledgeItems { get; }
    IQueryable<StorageObject> StorageObjects { get; }
    IQueryable<Document> Documents { get; }
    IQueryable<DocumentVersion> DocumentVersions { get; }
    IQueryable<ProcessingRun> ProcessingRuns { get; }
    IQueryable<DocumentChunk> DocumentChunks { get; }
    IQueryable<ChunkEmbedding> ChunkEmbeddings { get; }
    IQueryable<EmbeddingModel> EmbeddingModels { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
