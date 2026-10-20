namespace StudyAI.Domain.Documents;

public sealed class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public Guid ProcessingRunId { get; set; }
    public int ChunkIndex { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string LocatorJson { get; set; } = "{}";
    public int? TokenCount { get; set; }
}
