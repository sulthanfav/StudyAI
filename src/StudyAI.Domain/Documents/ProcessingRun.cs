namespace StudyAI.Domain.Documents;

public sealed class ProcessingRun
{
    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public string ProcessorVersion { get; set; } = "v1";
    public string Status { get; set; } = "queued";
    public bool IsActive { get; set; }
    public string? ErrorCode { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
