using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StudyAI.Application.Common.Interfaces;
using StudyAI.Domain.Ownership;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class AuthorizedDataSpaceScopeFactory(
    StudyAIDbContext dbContext,
    ICurrentActorAccessor actorAccessor,
    ICurrentDataSpaceAccessor dataSpaceAccessor) : IAuthorizedDataSpaceScopeFactory
{
    public async Task<IAuthorizedDataSpaceScope?> OpenAsync(
        Guid actorId,
        Guid dataSpaceId,
        bool requiresMutationAccess,
        CancellationToken cancellationToken = default)
    {
        if (actorId == Guid.Empty || dataSpaceId == Guid.Empty)
        {
            return null;
        }

        actorAccessor.CurrentActorId = actorId;
        dataSpaceAccessor.CurrentDataSpaceId = null;
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var role = await dbContext.DataSpaceMemberships
                .AsNoTracking()
                .Where(membership => membership.UserId == actorId && membership.DataSpaceId == dataSpaceId)
                .Select(membership => membership.Role)
                .SingleOrDefaultAsync(cancellationToken);

            if (role is null ||
                (requiresMutationAccess &&
                 string.Equals(role, DataSpaceMembership.RoleViewer, StringComparison.OrdinalIgnoreCase)))
            {
                await transaction.RollbackAsync(cancellationToken);
                await transaction.DisposeAsync();
                actorAccessor.CurrentActorId = null;
                return null;
            }

            dataSpaceAccessor.CurrentDataSpaceId = dataSpaceId;
            await dbContext.Database.ExecuteSqlRawAsync(
                "SELECT set_config('app.current_data_space_id', {0}, true);",
                [dataSpaceId.ToString()],
                cancellationToken);

            return new AuthorizedDataSpaceScope(transaction, role, actorAccessor, dataSpaceAccessor);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            await transaction.DisposeAsync();
            actorAccessor.CurrentActorId = null;
            dataSpaceAccessor.CurrentDataSpaceId = null;
            throw;
        }
    }

    private sealed class AuthorizedDataSpaceScope(
        IDbContextTransaction transaction,
        string role,
        ICurrentActorAccessor actorAccessor,
        ICurrentDataSpaceAccessor dataSpaceAccessor) : IAuthorizedDataSpaceScope
    {
        private bool _completed;
        private bool _disposed;

        public string Role { get; } = role;

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            await transaction.CommitAsync(cancellationToken);
            _completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (!_completed)
                {
                    await transaction.RollbackAsync();
                }
            }
            finally
            {
                await transaction.DisposeAsync();
                actorAccessor.CurrentActorId = null;
                dataSpaceAccessor.CurrentDataSpaceId = null;
                _disposed = true;
            }
        }
    }
}
