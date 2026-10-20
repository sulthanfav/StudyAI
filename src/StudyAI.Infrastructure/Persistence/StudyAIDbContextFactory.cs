using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudyAI.Infrastructure.Persistence;

public sealed class StudyAIDbContextFactory : IDesignTimeDbContextFactory<StudyAIDbContext>
{
    public StudyAIDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("STUDYAI_POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "STUDYAI_POSTGRES_CONNECTION_STRING is required for design-time database operations.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<StudyAIDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.UseVector();
            o.MigrationsHistoryTable("__ef_migrations_history", "ops");
        });

        return new StudyAIDbContext(optionsBuilder.Options);
    }
}
