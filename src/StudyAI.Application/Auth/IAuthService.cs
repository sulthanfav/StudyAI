using StudyAI.Application.Auth.Models;

namespace StudyAI.Application.Auth;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AuthUserSummary?> GetCurrentUserSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
}
