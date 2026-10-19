using System.Xml.Linq;
using Xunit;

namespace StudyAI.ArchitectureTests;

public sealed class ProjectReferenceRulesTests
{
    public static TheoryData<string, string[]> ApprovedReferenceGraph => new()
    {
        {
            "src/StudyAI.Domain/StudyAI.Domain.csproj",
            []
        },
        {
            "src/StudyAI.Contracts/StudyAI.Contracts.csproj",
            []
        },
        {
            "src/StudyAI.Application/StudyAI.Application.csproj",
            ["src/StudyAI.Domain/StudyAI.Domain.csproj"]
        },
        {
            "src/StudyAI.Infrastructure/StudyAI.Infrastructure.csproj",
            [
                "src/StudyAI.Application/StudyAI.Application.csproj",
                "src/StudyAI.Domain/StudyAI.Domain.csproj"
            ]
        },
        {
            "apps/api/StudyAI.Api/StudyAI.Api.csproj",
            [
                "src/StudyAI.Application/StudyAI.Application.csproj",
                "src/StudyAI.Contracts/StudyAI.Contracts.csproj",
                "src/StudyAI.Domain/StudyAI.Domain.csproj",
                "src/StudyAI.Infrastructure/StudyAI.Infrastructure.csproj"
            ]
        }
    };

    [Theory]
    [MemberData(nameof(ApprovedReferenceGraph))]
    public void ProductionProjectReferencesMatchApprovedDirection(
        string projectPath,
        string[] expectedReferences)
    {
        var repositoryRoot = FindRepositoryRoot();
        var absoluteProjectPath = Path.Combine(repositoryRoot, projectPath);
        var projectDirectory = Path.GetDirectoryName(absoluteProjectPath)!;
        var document = XDocument.Load(absoluteProjectPath);
        var actualReferences = document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference => Path.GetFullPath(Path.Combine(projectDirectory, reference!)))
            .Select(reference => Normalize(Path.GetRelativePath(repositoryRoot, reference)))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedReferences.Order(StringComparer.Ordinal), actualReferences);
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

        throw new DirectoryNotFoundException("Could not locate the StudyAI repository root.");
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}
