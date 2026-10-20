namespace StudyAI.Domain.Ai;

public sealed class EmbeddingModel
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ModelKey { get; set; } = string.Empty;
    public int Dimensions { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
