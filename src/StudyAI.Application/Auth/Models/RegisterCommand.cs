namespace StudyAI.Application.Auth.Models;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string DisplayName);
