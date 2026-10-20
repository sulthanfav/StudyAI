using Microsoft.AspNetCore.Http.HttpResults;
using StudyAI.Api.Security;
using StudyAI.Application.Workspace;
using StudyAI.Contracts.Workspace;

namespace StudyAI.Api.Endpoints;

public static class DataSpaceEndpoints
{
    public static IEndpointRouteBuilder MapDataSpaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/data-spaces")
            .WithTags("Data Spaces")
            .RequireAuthorization();

        group.MapGet("/{id:guid}/content-spaces", GetContentSpacesAsync)
            .WithName("GetDataSpaceContentSpaces")
            .WithSummary("Get content spaces within an authorized Data Space")
            .WithDescription("Returns all content spaces (libraries, courses, projects) belonging to the specified Data Space after verifying membership.")
            .Produces<List<ContentSpaceResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .AddEndpointFilter<DataSpaceAuthorizationFilter>();

        return app;
    }

    public static async Task<Ok<List<ContentSpaceResponse>>> GetContentSpacesAsync(
        Guid id,
        IContentSpaceReader contentSpaceReader,
        CancellationToken cancellationToken)
    {
        var contentSpaces = await contentSpaceReader.ListAsync(id, cancellationToken);
        var response = contentSpaces
            .Select(c => new ContentSpaceResponse(
                c.Id,
                c.DataSpaceId,
                c.Kind,
                c.Title,
                c.Description,
                c.Position))
            .ToList();

        return TypedResults.Ok(response);
    }
}
