namespace StudyAI.Application.Common.Interfaces;

/// <summary>
/// Authoritative server-side authorization check for Data Space access.
/// </summary>
public interface IDataSpaceAuthorizationService
{
    /// <summary>
    /// Validates whether the specified actor has permission to access the Data Space.
    /// </summary>
    Task<bool> CanAccessDataSpaceAsync(Guid actorId, Guid dataSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the role of the actor within the specified Data Space, or null if no membership exists.
    /// </summary>
    Task<string?> GetMemberRoleAsync(Guid actorId, Guid dataSpaceId, CancellationToken cancellationToken = default);
}
