namespace StudyAI.Contracts.Workspace;

/// <summary>
/// Represents a content space (library, course, or project container) within a Data Space.
/// </summary>
public sealed record ContentSpaceResponse(
    Guid Id,
    Guid DataSpaceId,
    string Kind,
    string Title,
    string? Description,
    int Position);
