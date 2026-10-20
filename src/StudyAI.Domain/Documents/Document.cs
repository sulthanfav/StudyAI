namespace StudyAI.Domain.Documents;

public sealed class Document
{
    public Guid KnowledgeItemId { get; set; }
    public Guid DataSpaceId { get; set; }
    public string ItemKind { get; set; } = "document";
    public string OriginalFilename { get; set; } = string.Empty;
    public Guid? CurrentVersionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
