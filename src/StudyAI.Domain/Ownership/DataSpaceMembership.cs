namespace StudyAI.Domain.Ownership;

public sealed class DataSpaceMembership
{
    public const string RoleOwner = "owner";
    public const string RoleMember = "member";
    public const string RoleViewer = "viewer";

    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = RoleOwner;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
