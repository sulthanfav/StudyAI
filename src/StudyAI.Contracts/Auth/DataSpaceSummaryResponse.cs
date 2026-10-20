namespace StudyAI.Contracts.Auth;

/// <summary>
/// Summary of an authorized Data Space accessible by the current actor.
/// </summary>
public sealed record DataSpaceSummaryResponse(
    Guid Id,
    string Name,
    string Kind,
    string Role);
