namespace StudyAI.Application.Auth.Models;

public sealed record RegisterResult(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid PersonalDataSpaceId,
    Guid LibraryContentSpaceId);
