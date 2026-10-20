namespace StudyAI.Application.Auth.Models;

public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = false);
