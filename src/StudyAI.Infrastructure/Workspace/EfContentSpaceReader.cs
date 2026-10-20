using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Workspace;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Workspace;

public sealed class EfContentSpaceReader(StudyAIDbContext dbContext) : IContentSpaceReader
{
    public async Task<IReadOnlyList<ContentSpaceSummary>> ListAsync(
        Guid dataSpaceId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var results = await dbContext.ContentSpaces
            .AsNoTracking()
            .Where(item => item.DataSpaceId == dataSpaceId && item.DeletedAt == null)
            .OrderBy(item => item.Position)
            .Select(item => new ContentSpaceSummary(
                item.Id,
                item.DataSpaceId,
                item.Kind,
                item.Title,
                item.Description,
                item.Position))
            .ToListAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return results;
    }
}
