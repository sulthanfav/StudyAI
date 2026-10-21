using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyAI.Infrastructure.Persistence;

namespace StudyAI.Infrastructure.Identity;

public sealed class SessionCookieValidator(
    StudyAIDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<AuthenticationSessionOptions> sessionOptions)
{
    public async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var identifier = context.Principal?.FindFirstValue(SessionIdentifier.ClaimType);
        if (string.IsNullOrWhiteSpace(identifier))
        {
            await RejectAsync(context);
            return;
        }

        var identifierHash = SessionIdentifier.Hash(identifier);
        var state = await (
            from session in dbContext.UserSessions.AsNoTracking()
            join user in dbContext.Users.AsNoTracking() on session.UserId equals user.Id
            where session.SessionIdentifierHash == identifierHash
            select new
            {
                session.Id,
                session.RevokedAt,
                session.AbsoluteExpiresAt,
                session.LastSeenAt,
                session.SecurityStampSnapshot,
                user.SecurityStamp,
                user.Status,
                user.DeletedAt
            })
            .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

        var now = timeProvider.GetUtcNow();
        var idleCutoff = now.AddSeconds(-sessionOptions.Value.IdleLifetimeSeconds);
        var isEligible = state is not null &&
            state.RevokedAt is null &&
            state.AbsoluteExpiresAt > now &&
            state.LastSeenAt > idleCutoff &&
            string.Equals(state.SecurityStampSnapshot, state.SecurityStamp, StringComparison.Ordinal) &&
            string.Equals(state.Status, "active", StringComparison.Ordinal) &&
            state.DeletedAt is null;

        if (!isEligible)
        {
            await RejectAsync(context);
            return;
        }

        await dbContext.UserSessions
            .Where(session => session.Id == state!.Id && session.RevokedAt == null)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(session => session.LastSeenAt, now),
                context.HttpContext.RequestAborted);
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}
