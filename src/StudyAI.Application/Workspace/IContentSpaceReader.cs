namespace StudyAI.Application.Workspace;

public interface IContentSpaceReader
{
    Task<IReadOnlyList<ContentSpaceSummary>> ListAsync(
        Guid dataSpaceId,
        CancellationToken cancellationToken = default);
}
