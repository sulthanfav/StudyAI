namespace StudyAI.Domain.Ownership;

public sealed class DataSpace
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Kind { get; set; } = "personal";
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
