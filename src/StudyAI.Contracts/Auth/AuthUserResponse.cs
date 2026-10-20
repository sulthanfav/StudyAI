namespace StudyAI.Contracts.Auth;

/// <summary>
/// Authenticated user profile and active personal workspace summary.
/// </summary>
public sealed record AuthUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Status,
    Guid PersonalDataSpaceId);
