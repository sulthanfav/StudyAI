namespace StudyAI.Contracts.Auth;

/// <summary>
/// Response returned upon successful user registration and atomic personal workspace provisioning.
/// </summary>
public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid PersonalDataSpaceId,
    Guid LibraryContentSpaceId);
