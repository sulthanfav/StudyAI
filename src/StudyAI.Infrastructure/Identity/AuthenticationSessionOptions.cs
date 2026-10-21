namespace StudyAI.Infrastructure.Identity;

public sealed class AuthenticationSessionOptions
{
    public int IdleLifetimeSeconds { get; init; } = 28_800;
    public int StandardAbsoluteLifetimeSeconds { get; init; } = 28_800;
    public int RememberMeAbsoluteLifetimeSeconds { get; init; } = 2_592_000;
}
