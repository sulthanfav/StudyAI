using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyAI.Domain.Identity;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class StudyAIUserStore :
    IUserStore<User>,
    IUserPasswordStore<User>,
    IUserEmailStore<User>,
    IUserSecurityStampStore<User>,
    IUserLockoutStore<User>
{
    private static readonly IdentityErrorDescriber ErrorDescriber = new();

    private readonly StudyAIDbContext _context;
    private readonly TimeProvider _timeProvider;

    public StudyAIUserStore(StudyAIDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    // -------------------------------------------------------------------------
    // IUserStore<User>
    // -------------------------------------------------------------------------

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult<string?>(user.UserName);
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.UserName = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult<string?>(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.CreateVersion7();
        }

        if (string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            user.SecurityStamp = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(user.ConcurrencyStamp))
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }

        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var persistedState = await _context.Users
            .AsNoTracking()
            .Where(candidate => candidate.Id == user.Id)
            .Select(candidate => new
            {
                candidate.PasswordHash,
                candidate.SecurityStamp,
                candidate.Status,
                candidate.DeletedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        var revokeAllSessions = persistedState is not null &&
            (!string.Equals(persistedState.PasswordHash, user.PasswordHash, StringComparison.Ordinal) ||
             !string.Equals(persistedState.SecurityStamp, user.SecurityStamp, StringComparison.Ordinal) ||
             !string.Equals(persistedState.Status, user.Status, StringComparison.Ordinal) ||
             persistedState.DeletedAt != user.DeletedAt);

        user.UpdatedAt = _timeProvider.GetUtcNow();
        user.ConcurrencyStamp = Guid.NewGuid().ToString("N");

        _context.Users.Update(user);
        if (revokeAllSessions)
        {
            var revokedAt = _timeProvider.GetUtcNow();
            var activeSessions = await _context.UserSessions
                .Where(session => session.UserId == user.Id && session.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var session in activeSessions)
            {
                session.RevokedAt = revokedAt;
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        _context.Users.Remove(user);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return null;
        }

        return await _context.Users.FirstOrDefaultAsync(
            u => u.NormalizedUserName == normalizedUserName || u.NormalizedEmail == normalizedUserName,
            cancellationToken);
    }

    // -------------------------------------------------------------------------
    // IUserPasswordStore<User>
    // -------------------------------------------------------------------------

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    // -------------------------------------------------------------------------
    // IUserEmailStore<User>
    // -------------------------------------------------------------------------

    public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.Email = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return null;
        }

        var normalized = normalizedEmail.ToUpperInvariant();
        var normalizedLower = normalizedEmail.ToLowerInvariant();

        return await _context.Users.FirstOrDefaultAsync(
            u => u.NormalizedEmail == normalized || u.NormalizedEmail == normalizedLower || u.Email == normalizedLower,
            cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult<string?>(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.NormalizedEmail = normalizedEmail ?? string.Empty;
        return Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // IUserSecurityStampStore<User>
    // -------------------------------------------------------------------------

    public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult<string?>(user.SecurityStamp);
    }

    // -------------------------------------------------------------------------
    // IUserLockoutStore<User>
    // -------------------------------------------------------------------------

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Managed by DI lifetime
    }
}
