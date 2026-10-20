namespace StudyAI.Domain.Documents;

public sealed class ChunkEmbedding
{
    public Guid ChunkId { get; set; }
    public Guid DataSpaceId { get; set; }
    public Guid EmbeddingModelId { get; set; }
    public float[] Embedding { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
