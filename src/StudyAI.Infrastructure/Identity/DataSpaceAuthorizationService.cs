using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Common.Interfaces;
using StudyAI.Domain.Ownership;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class DataSpaceAuthorizationService : IDataSpaceAuthorizationService
{
    private readonly StudyAIDbContext _dbContext;

    public DataSpaceAuthorizationService(StudyAIDbContext dbContext)
    {
        _dbContext = dbContext;
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

        var hasMembership = await _dbContext.DataSpaceMemberships
            .AsNoTracking()
            .AnyAsync(m => m.UserId == actorId && m.DataSpaceId == dataSpaceId, cancellationToken);

        if (hasMembership)
        {
            return true;
        }

        // Direct personal space ownership check as defense-in-depth
        return await _dbContext.DataSpaces
            .AsNoTracking()
            .AnyAsync(s => s.Id == dataSpaceId && s.OwnerUserId == actorId, cancellationToken);
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

        var membership = await _dbContext.DataSpaceMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == actorId && m.DataSpaceId == dataSpaceId, cancellationToken);

        if (membership is not null)
        {
            return membership.Role;
        }

        var isOwner = await _dbContext.DataSpaces
            .AsNoTracking()
            .AnyAsync(s => s.Id == dataSpaceId && s.OwnerUserId == actorId, cancellationToken);

        return isOwner ? DataSpaceMembership.RoleOwner : null;
    }
}
