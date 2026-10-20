using System.ComponentModel.DataAnnotations;

namespace StudyAI.Contracts.Auth;

/// <summary>
/// Payload for registering a new StudyAI Campus user account.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    /// <summary>
    /// Account password meeting security requirements (min 8 chars, mixed case, digit, symbol).
    /// </summary>
    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public required string Password { get; init; }

    /// <summary>
    /// User display name.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public required string DisplayName { get; init; }
}
