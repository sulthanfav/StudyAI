namespace StudyAI.Application.Auth.Models;

public sealed record AuthUserSummary(
    Guid UserId,
    string Email,
    string DisplayName,
    string Status,
    Guid PersonalDataSpaceId);

public sealed record LoginResult(
    bool Succeeded,
    bool IsLockedOut,
    bool RequiresTwoFactor,
    AuthUserSummary? User,
    string? ErrorMessage);
