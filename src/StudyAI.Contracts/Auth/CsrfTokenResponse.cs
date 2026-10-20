namespace StudyAI.Contracts.Auth;

/// <summary>
/// Antiforgery / CSRF request token.
/// </summary>
public sealed record CsrfTokenResponse(string Token);
