using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Api.Security;
using StudyAI.Application.Auth;
using StudyAI.Application.Auth.Models;
using StudyAI.Contracts.Auth;

namespace StudyAI.Api.Endpoints;

public static class AuthEndpoints
{
    private const string DuplicateEmailErrorCode = "auth_email_already_registered";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("RegisterUser")
            .WithSummary("Register a new user account with default Personal Data Space")
            .WithDescription("Creates the user identity, default personal data space, owner membership, and root personal library atomically.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AddEndpointFilter<AntiforgeryValidationFilter>()
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .WithName("LoginUser")
            .WithSummary("Authenticate with email and password")
            .WithDescription("Authenticates user credentials and issues an encrypted HttpOnly session cookie.")
            .Produces<AuthUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status423Locked)
            .AddEndpointFilter<AntiforgeryValidationFilter>()
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .WithName("LogoutUser")
            .WithSummary("Log out and invalidate current session")
            .WithDescription("Clears the session cookie and invalidates authentication context.")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<AntiforgeryValidationFilter>()
            .RequireAuthorization();

        group.MapGet("/me", GetMeAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get authenticated user profile and active personal workspace")
            .WithDescription("Returns user details and primary personal data space for the current session.")
            .Produces<AuthUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("/csrf", GetCsrfToken)
            .WithName("GetCsrfToken")
            .WithSummary("Issue Antiforgery / CSRF token")
            .WithDescription("Generates and returns an antiforgery token for state-changing requests.")
            .Produces<CsrfTokenResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    public static async Task<Results<Created<RegisterResponse>, Conflict<ProblemDetails>, BadRequest<ProblemDetails>>> RegisterAsync(
        RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RegisterCommand(request.Email, request.Password, request.DisplayName);
            var result = await authService.RegisterAsync(command, cancellationToken);
            if (result is RegisterResult.DuplicateEmail)
            {
                return DuplicateEmailConflict();
            }

            var success = (RegisterResult.Success)result;
            var response = new RegisterResponse(
                success.UserId,
                success.Email,
                success.DisplayName,
                success.PersonalDataSpaceId,
                success.LibraryContentSpaceId);

            return TypedResults.Created("/api/v1/auth/me", response);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message
            });
        }
    }

    private static Conflict<ProblemDetails> DuplicateEmailConflict() =>
        TypedResults.Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Email Already Registered",
            Detail = "An account with this email already exists.",
            Extensions =
            {
                ["code"] = DuplicateEmailErrorCode
            }
        });

    public static async Task<Results<Ok<AuthUserResponse>, ProblemHttpResult, BadRequest<ProblemDetails>>> LoginAsync(
        LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.RememberMe);
        var result = await authService.LoginAsync(command, cancellationToken);

        if (result.Succeeded && result.User is not null)
        {
            var response = new AuthUserResponse(
                result.User.UserId,
                result.User.Email,
                result.User.DisplayName,
                result.User.Status,
                result.User.PersonalDataSpaceId);

            return TypedResults.Ok(response);
        }

        if (result.IsLockedOut)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status423Locked,
                title: "Account Locked",
                detail: result.ErrorMessage ?? "Account is temporarily locked out.");
        }

        return TypedResults.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Unauthorized",
            detail: "Invalid email or password.");
    }

    public static async Task<NoContent> LogoutAsync(
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<AuthUserResponse>, UnauthorizedHttpResult, NotFound<ProblemDetails>>> GetMeAsync(
        ClaimsPrincipal userPrincipal,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var userIdClaim = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var user = await authService.GetCurrentUserSummaryAsync(userId, cancellationToken);
        if (user is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "User profile could not be found."
            });
        }

        return TypedResults.Ok(new AuthUserResponse(
            user.UserId,
            user.Email,
            user.DisplayName,
            user.Status,
            user.PersonalDataSpaceId));
    }

    public static Ok<CsrfTokenResponse> GetCsrfToken(
        IAntiforgery antiforgery,
        HttpContext httpContext)
    {
        var tokenSet = antiforgery.GetAndStoreTokens(httpContext);
        return TypedResults.Ok(new CsrfTokenResponse(tokenSet.RequestToken ?? string.Empty));
    }
}
