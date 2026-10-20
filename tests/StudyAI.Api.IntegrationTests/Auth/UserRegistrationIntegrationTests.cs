using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class UserRegistrationIntegrationTests : IAsyncLifetime
{
    private StudyAIApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);

        _factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task SuccessfulRegistrationProvisionsAllEntitiesAtomically()
    {
        var ct = TestContext.Current.CancellationToken;
        var uniqueEmail = $"newstudent.{Guid.NewGuid():N}@studyai.test";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            Password = "Password123!",
            DisplayName = "New Student"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request, ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var payload = await response.Content.ReadFromJsonAsync<RegisterResponse>(ct);
        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.UserId);
        Assert.Equal(uniqueEmail, payload.Email);
        Assert.Equal("New Student", payload.DisplayName);
        Assert.NotEqual(Guid.Empty, payload.PersonalDataSpaceId);
        Assert.NotEqual(Guid.Empty, payload.LibraryContentSpaceId);

        // Verify database state using admin context
        await using var adminContext = PersistenceFixture.CreateAdminContext();

        var dbUser = await adminContext.Users.FirstOrDefaultAsync(u => u.Id == payload.UserId, ct);
        Assert.NotNull(dbUser);
        Assert.Equal("active", dbUser.Status);
        Assert.False(string.IsNullOrWhiteSpace(dbUser.PasswordHash));
        Assert.False(string.IsNullOrWhiteSpace(dbUser.SecurityStamp));

        var dbSpace = await adminContext.DataSpaces.FirstOrDefaultAsync(s => s.Id == payload.PersonalDataSpaceId, ct);
        Assert.NotNull(dbSpace);
        Assert.Equal("personal", dbSpace.Kind);
        Assert.Equal(dbUser.Id, dbSpace.OwnerUserId);

        var dbMembership = await adminContext.DataSpaceMemberships.FirstOrDefaultAsync(
            m => m.DataSpaceId == payload.PersonalDataSpaceId && m.UserId == dbUser.Id, ct);
        Assert.NotNull(dbMembership);
        Assert.Equal("owner", dbMembership.Role);

        var dbLibrary = await adminContext.ContentSpaces.FirstOrDefaultAsync(c => c.Id == payload.LibraryContentSpaceId, ct);
        Assert.NotNull(dbLibrary);
        Assert.Equal("library", dbLibrary.Kind);
        Assert.Equal(dbSpace.Id, dbLibrary.DataSpaceId);
    }

    [Fact]
    public async Task DuplicateRegistrationReturns409Conflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var uniqueEmail = $"duplicate.{Guid.NewGuid():N}@studyai.test";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            Password = "Password123!",
            DisplayName = "First Registration"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", request, ct);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Attempt second registration with same email
        var duplicateResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", request, ct);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var problem = await duplicateResponse.Content.ReadFromJsonAsync<ProblemDetails>(ct);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
    }

    [Fact]
    public async Task RegistrationWithInvalidPasswordReturns400BadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new RegisterRequest
        {
            Email = $"weak.{Guid.NewGuid():N}@studyai.test",
            Password = "weak", // less than 8 chars, no digit, no symbol
            DisplayName = "Weak Password User"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request, ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify that user was NOT created
        await using var adminContext = PersistenceFixture.CreateAdminContext();
        var exists = await adminContext.Users.AnyAsync(u => u.Email == request.Email, ct);
        Assert.False(exists);
    }

    [Fact]
    public async Task ConcurrentRegistrationWithSameEmailEnsuresOnlyOneSucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        var raceEmail = $"race.{Guid.NewGuid():N}@studyai.test";
        var request = new RegisterRequest
        {
            Email = raceEmail,
            Password = "Password123!",
            DisplayName = "Concurrent User"
        };

        using var clientA = _factory.CreateClient();
        using var clientB = _factory.CreateClient();

        var taskA = clientA.PostAsJsonAsync("/api/v1/auth/register", request, ct);
        var taskB = clientB.PostAsJsonAsync("/api/v1/auth/register", request, ct);

        var responses = await Task.WhenAll(taskA, taskB);

        var statusCodes = responses.Select(r => r.StatusCode).ToList();
        Assert.Contains(HttpStatusCode.Created, statusCodes);
        Assert.Contains(HttpStatusCode.Conflict, statusCodes);

        // Verify only one user in DB
        await using var adminContext = PersistenceFixture.CreateAdminContext();
        var count = await adminContext.Users.CountAsync(u => u.Email == raceEmail, ct);
        Assert.Equal(1, count);
    }
}
