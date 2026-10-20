namespace StudyAI.Domain.Workspace;

public sealed class ContentSpace
{
    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public string Kind { get; set; } = "library";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
}
