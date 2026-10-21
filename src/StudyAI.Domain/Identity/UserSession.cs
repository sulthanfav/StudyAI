namespace StudyAI.Domain.Identity;

public sealed class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[] SessionIdentifierHash { get; set; } = [];
    public string SecurityStampSnapshot { get; set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset AbsoluteExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
}
