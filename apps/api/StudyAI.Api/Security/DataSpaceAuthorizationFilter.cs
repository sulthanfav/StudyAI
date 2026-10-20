using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StudyAI.Application.Common.Interfaces;

namespace StudyAI.Api.Security;

/// <summary>
/// Endpoint filter enforcing server-side Data Space membership verification and transaction-local RLS setup.
/// </summary>
public sealed class DataSpaceAuthorizationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var httpContext = context.HttpContext;
        var user = httpContext.User;

        var actorIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actorIdClaim) || !Guid.TryParse(actorIdClaim, out var actorId))
        {
            return TypedResults.Unauthorized();
        }

        if (!TryResolveDataSpaceId(httpContext, out var dataSpaceId))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "A valid Data Space ID must be provided in the route parameter or X-Data-Space-Id header.");
        }

        var authService = httpContext.RequestServices.GetRequiredService<IDataSpaceAuthorizationService>();
        var actorAccessor = httpContext.RequestServices.GetRequiredService<ICurrentActorAccessor>();
        actorAccessor.CurrentActorId = actorId;
        var canAccess = await authService.CanAccessDataSpaceAsync(actorId, dataSpaceId, httpContext.RequestAborted);
        if (!canAccess)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: "The authenticated actor is not authorized to access this Data Space.");
        }

        var accessor = httpContext.RequestServices.GetRequiredService<ICurrentDataSpaceAccessor>();
        accessor.CurrentDataSpaceId = dataSpaceId;

        return await next(context);
    }

    private static bool TryResolveDataSpaceId(
        HttpContext httpContext,
        out Guid dataSpaceId)
    {
        if (httpContext.Request.RouteValues.TryGetValue("dataSpaceId", out var dsObj) &&
            Guid.TryParse(dsObj?.ToString(), out dataSpaceId))
        {
            return true;
        }

        if (httpContext.Request.RouteValues.TryGetValue("id", out var idObj) &&
            Guid.TryParse(idObj?.ToString(), out dataSpaceId))
        {
            return true;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Data-Space-Id", out var headerVal) &&
            Guid.TryParse(headerVal.ToString(), out dataSpaceId))
        {
            return true;
        }

        dataSpaceId = Guid.Empty;
        return false;
    }
}
