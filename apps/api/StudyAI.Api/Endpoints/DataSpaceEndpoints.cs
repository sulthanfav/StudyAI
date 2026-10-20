using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StudyAI.Api.Security;
using StudyAI.Contracts.Workspace;
using StudyAI.Infrastructure.Persistence;

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
        StudyAIDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var contentSpaces = await dbContext.ContentSpaces
            .AsNoTracking()
            .Where(c => c.DataSpaceId == id && c.DeletedAt == null)
            .OrderBy(c => c.Position)
            .Select(c => new ContentSpaceResponse(
                c.Id,
                c.DataSpaceId,
                c.Kind,
                c.Title,
                c.Description,
                c.Position))
            .ToListAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return TypedResults.Ok(contentSpaces);
    }
}
