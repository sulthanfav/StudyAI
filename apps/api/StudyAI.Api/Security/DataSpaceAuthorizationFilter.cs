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

        var requiresMutationAccess = HttpMethods.IsPost(httpContext.Request.Method) ||
                                     HttpMethods.IsPut(httpContext.Request.Method) ||
                                     HttpMethods.IsPatch(httpContext.Request.Method) ||
                                     HttpMethods.IsDelete(httpContext.Request.Method);
        var scopeFactory = httpContext.RequestServices.GetRequiredService<IAuthorizedDataSpaceScopeFactory>();
        await using var authorizedScope = await scopeFactory.OpenAsync(
            actorId,
            dataSpaceId,
            requiresMutationAccess,
            httpContext.RequestAborted);
        if (authorizedScope is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: "The authenticated actor is not authorized to access this Data Space.");
        }

        var result = await next(context);
        if (result is not IStatusCodeHttpResult { StatusCode: >= 400 })
        {
            await authorizedScope.CompleteAsync(httpContext.RequestAborted);
        }

        return result;
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
