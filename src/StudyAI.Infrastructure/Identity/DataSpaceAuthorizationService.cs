using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Common.Interfaces;
using StudyAI.Domain.Ownership;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class DataSpaceAuthorizationService : IDataSpaceAuthorizationService
{
    private readonly StudyAIDbContext _dbContext;
    private readonly ICurrentActorAccessor _actorAccessor;

    public DataSpaceAuthorizationService(StudyAIDbContext dbContext, ICurrentActorAccessor actorAccessor)
    {
        _dbContext = dbContext;
        _actorAccessor = actorAccessor;
    }

    public async Task<bool> CanAccessDataSpaceAsync(
        Guid actorId,
        Guid dataSpaceId,
        CancellationToken cancellationToken = default)
    {
        if (actorId == Guid.Empty || dataSpaceId == Guid.Empty)
        {
            return false;
        }

        _actorAccessor.CurrentActorId = actorId;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var hasMembership = await _dbContext.DataSpaceMemberships
            .AsNoTracking()
            .AnyAsync(m => m.UserId == actorId && m.DataSpaceId == dataSpaceId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return hasMembership;
    }

    public async Task<string?> GetMemberRoleAsync(
        Guid actorId,
        Guid dataSpaceId,
        CancellationToken cancellationToken = default)
    {
        if (actorId == Guid.Empty || dataSpaceId == Guid.Empty)
        {
            return null;
        }

        _actorAccessor.CurrentActorId = actorId;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var membership = await _dbContext.DataSpaceMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == actorId && m.DataSpaceId == dataSpaceId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return membership?.Role;
    }
}
