using System.ComponentModel.DataAnnotations;

namespace StudyAI.Contracts.Auth;

/// <summary>
/// Payload for user login.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>
    /// Account password.
    /// </summary>
    [Required]
    public required string Password { get; init; }

    /// <summary>
    /// Whether to persist the cookie session across browser restarts.
    /// </summary>
    public bool RememberMe { get; init; }
}
