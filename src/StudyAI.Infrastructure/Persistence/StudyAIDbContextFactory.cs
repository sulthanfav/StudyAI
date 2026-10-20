using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudyAI.Infrastructure.Persistence;

public sealed class StudyAIDbContextFactory : IDesignTimeDbContextFactory<StudyAIDbContext>
{
    public StudyAIDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("STUDYAI_POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=55432;Database=studyai_persistence_test;Username=studyai_admin;Password=B598081A714FAEE7A8A6A4B786B5C26F46A86532136C43B87197C7ABDAA33315";

        var optionsBuilder = new DbContextOptionsBuilder<StudyAIDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.UseVector();
            o.MigrationsHistoryTable("__ef_migrations_history", "ops");
        });

        return new StudyAIDbContext(optionsBuilder.Options);
    }
}
