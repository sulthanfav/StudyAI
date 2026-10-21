using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyAI.Application.Auth;
using StudyAI.Application.Auth.Models;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private const string NormalizedEmailConstraintName = "ux_users_normalized_email";

    private static readonly HashSet<string> DuplicateIdentityErrorCodes =
        ["DuplicateEmail", "DuplicateUserName"];

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly StudyAIDbContext _dbContext;
    private readonly AuthenticationSessionOptions _sessionOptions;
    private readonly TimeProvider _timeProvider;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        StudyAIDbContext dbContext,
        IOptions<AuthenticationSessionOptions> sessionOptions,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _sessionOptions = sessionOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task<RegisterResult> RegisterAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ArgumentException("Email is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ArgumentException("Password is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(command));
        }

        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return new RegisterResult.DuplicateEmail();
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = new User
            {
                Id = Guid.CreateVersion7(),
                Email = command.Email.Trim(),
                UserName = command.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                NormalizedUserName = normalizedEmail,
                DisplayName = command.DisplayName.Trim(),
                Status = "active",
                Locale = "id-ID",
                Timezone = "Asia/Jakarta",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, command.Password);
            if (!createResult.Succeeded)
            {
                if (createResult.Errors.Any(error => DuplicateIdentityErrorCodes.Contains(error.Code)))
                {
                    return new RegisterResult.DuplicateEmail();
                }

                var errorMessages = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new ArgumentException($"User registration failed: {errorMessages}");
            }

            var dataSpace = new DataSpace
            {
                Id = Guid.CreateVersion7(),
                OwnerUserId = user.Id,
                Kind = "personal",
                Name = $"{user.DisplayName}'s Personal Space",
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.DataSpaces.Add(dataSpace);

            var membership = new DataSpaceMembership
            {
                Id = Guid.CreateVersion7(),
                DataSpaceId = dataSpace.Id,
                UserId = user.Id,
                Role = DataSpaceMembership.RoleOwner,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.DataSpaceMemberships.Add(membership);

            var library = new ContentSpace
            {
                Id = Guid.CreateVersion7(),
                DataSpaceId = dataSpace.Id,
                Kind = "library",
                Title = "Personal Library",
                Description = "Default personal study library",
                Position = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.ContentSpaces.Add(library);

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT set_config('app.current_actor_id', {0}, true), set_config('app.current_data_space_id', {1}, true);",
                [user.Id.ToString(), dataSpace.Id.ToString()],
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new RegisterResult.Success(
                user.Id,
                user.Email,
                user.DisplayName,
                dataSpace.Id,
                library.Id);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Npgsql.PostgresException
            {
                SqlState: Npgsql.PostgresErrorCodes.UniqueViolation,
                ConstraintName: NormalizedEmailConstraintName
            })
        {
            return new RegisterResult.DuplicateEmail();
        }
    }

    public async Task<LoginResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return new LoginResult(false, false, false, null, "Invalid credentials.");
        }

        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return new LoginResult(false, false, false, null, "Invalid credentials.");
        }

        if (!string.Equals(user.Status, "active", StringComparison.Ordinal) || user.DeletedAt is not null)
        {
            return new LoginResult(false, false, false, null, "Invalid credentials.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return new LoginResult(false, true, false, null, "Account is temporarily locked out due to multiple failed attempts.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            command.Password,
            lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            var absoluteLifetimeSeconds = command.RememberMe
                ? _sessionOptions.RememberMeAbsoluteLifetimeSeconds
                : _sessionOptions.StandardAbsoluteLifetimeSeconds;
            if (absoluteLifetimeSeconds <= 0 || _sessionOptions.IdleLifetimeSeconds <= 0)
            {
                throw new InvalidOperationException(
                    "Authentication session lifetimes must be positive numbers of seconds.");
            }

            var now = _timeProvider.GetUtcNow();
            var sessionIdentifier = SessionIdentifier.Create();
            _dbContext.UserSessions.Add(new UserSession
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                SessionIdentifierHash = SessionIdentifier.Hash(sessionIdentifier),
                SecurityStampSnapshot = user.SecurityStamp,
                IssuedAt = now,
                AbsoluteExpiresAt = now.AddSeconds(absoluteLifetimeSeconds),
                LastSeenAt = now
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _signInManager.SignInWithClaimsAsync(
                user,
                new AuthenticationProperties
                {
                    IsPersistent = command.RememberMe,
                    AllowRefresh = true,
                    ExpiresUtc = now.AddSeconds(absoluteLifetimeSeconds)
                },
                [new Claim(SessionIdentifier.ClaimType, sessionIdentifier)]);

            var personalSpace = await _dbContext.DataSpaces
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.OwnerUserId == user.Id && s.Kind == "personal", cancellationToken);

            var userSummary = new AuthUserSummary(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Status,
                personalSpace?.Id ?? Guid.Empty);

            return new LoginResult(true, false, false, userSummary, null);
        }

        if (signInResult.IsLockedOut)
        {
            return new LoginResult(false, true, false, null, "Account is temporarily locked out due to multiple failed attempts.");
        }

        return new LoginResult(false, false, false, null, "Invalid credentials.");
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var identifier = _signInManager.Context.User.FindFirstValue(SessionIdentifier.ClaimType);
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            var identifierHash = SessionIdentifier.Hash(identifier);
            var session = await _dbContext.UserSessions.SingleOrDefaultAsync(
                candidate => candidate.SessionIdentifierHash == identifierHash && candidate.RevokedAt == null,
                cancellationToken);
            if (session is not null)
            {
                session.RevokedAt = _timeProvider.GetUtcNow();
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await _signInManager.SignOutAsync();
    }

    public async Task<AuthUserSummary?> GetCurrentUserSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var personalSpace = await _dbContext.DataSpaces
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OwnerUserId == user.Id && s.Kind == "personal", cancellationToken);

        return new AuthUserSummary(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Status,
            personalSpace?.Id ?? Guid.Empty);
    }
}
