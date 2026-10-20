namespace StudyAI.Domain.Documents;

public sealed class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNo { get; set; }
    public Guid StorageObjectId { get; set; }
    public int? PageCount { get; set; }
    public string ProcessingStatus { get; set; } = "pending";
    public string? ProcessingVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
