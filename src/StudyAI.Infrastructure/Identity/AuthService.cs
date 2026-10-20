using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Auth;
using StudyAI.Application.Auth.Models;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly StudyAIDbContext _dbContext;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        StudyAIDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
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
            throw new InvalidOperationException("Email is already registered.");
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

            await _dbContext.SetTenantSessionContextAsync(dataSpace.Id, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new RegisterResult(
                user.Id,
                user.Email,
                user.DisplayName,
                dataSpace.Id,
                library.Id);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            throw new InvalidOperationException("Email is already registered.", ex);
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

        if (await _userManager.IsLockedOutAsync(user))
        {
            return new LoginResult(false, true, false, null, "Account is temporarily locked out due to multiple failed attempts.");
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            command.Password,
            isPersistent: command.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
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
