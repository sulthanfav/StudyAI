namespace StudyAI.Domain.Documents;

public sealed class StorageObject
{
    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string ChecksumSha256 { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string ChargeClass { get; set; } = "billable";
    public string State { get; set; } = "pending";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
}
