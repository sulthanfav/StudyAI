namespace StudyAI.Application.Auth.Models;

public abstract record RegisterResult
{
    private RegisterResult()
    {
    }

    public sealed record Success(
        Guid UserId,
        string Email,
        string DisplayName,
        Guid PersonalDataSpaceId,
        Guid LibraryContentSpaceId) : RegisterResult;

    public sealed record DuplicateEmail : RegisterResult;
}
