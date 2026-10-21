using Xunit;

namespace StudyAI.ArchitectureTests;

public sealed class EndpointArchitectureTests
{
    [Fact]
    public void ApiEndpointsAndFiltersDoNotDependOnPersistenceImplementationDetails()
    {
        var repositoryRoot = FindRepositoryRoot();
        var apiRoot = Path.Combine(repositoryRoot, "apps", "api", "StudyAI.Api");
        var files = new[]
        {
            Path.Combine(apiRoot, "Endpoints"),
            Path.Combine(apiRoot, "Filters"),
            Path.Combine(apiRoot, "Security")
        }
        .Where(Directory.Exists)
        .SelectMany(directory => Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories));

        var violations = new List<string>();
        var forbiddenTokens = new[]
        {
            "StudyAI.Infrastructure",
            "Microsoft.EntityFrameworkCore",
            "StudyAIDbContext",
            "IApplicationDbContext",
            ".AsNoTracking(",
            ".ToListAsync("
        };

        foreach (var file in files)
        {
            var fileName = Path.GetRelativePath(repositoryRoot, file);
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!line.TrimStart().StartsWith("//", StringComparison.Ordinal) &&
                    forbiddenTokens.Any(token => line.Contains(token, StringComparison.Ordinal)))
                {
                    violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                }
            }
        }

        Assert.Empty(violations);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "StudyAI.slnx")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate StudyAI repository root.");
    }
}
