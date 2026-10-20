namespace StudyAI.Application.Workspace;

public sealed record ContentSpaceSummary(
    Guid Id,
    Guid DataSpaceId,
    string Kind,
    string Title,
    string? Description,
    int Position);
