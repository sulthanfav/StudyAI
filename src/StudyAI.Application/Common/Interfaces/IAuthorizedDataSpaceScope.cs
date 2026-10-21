namespace StudyAI.Application.Common.Interfaces;

public interface IAuthorizedDataSpaceScope : IAsyncDisposable
{
    string Role { get; }

    Task CompleteAsync(CancellationToken cancellationToken = default);
}

public interface IAuthorizedDataSpaceScopeFactory
{
    Task<IAuthorizedDataSpaceScope?> OpenAsync(
        Guid actorId,
        Guid dataSpaceId,
        bool requiresMutationAccess,
        CancellationToken cancellationToken = default);
}
