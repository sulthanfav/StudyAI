using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Documents;

namespace StudyAI.Infrastructure.Persistence.Configurations.Documents;

public sealed class ProcessingRunConfiguration : IEntityTypeConfiguration<ProcessingRun>
{
    public void Configure(EntityTypeBuilder<ProcessingRun> builder)
    {
        builder.ToTable("processing_runs", "documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.DocumentVersionId)
            .HasColumnName("document_version_id")
            .IsRequired();

        builder.Property(x => x.ProcessorVersion)
            .HasColumnName("processor_version")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ErrorCode)
            .HasColumnName("error_code");

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at");

        builder.Property(x => x.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<DocumentVersion>()
            .WithMany()
            .HasForeignKey(x => x.DocumentVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Id, x.DataSpaceId, x.DocumentVersionId })
            .IsUnique();

        builder.HasIndex(x => x.DocumentVersionId)
            .HasDatabaseName("ux_processing_active_run")
            .HasFilter("is_active = true")
            .IsUnique();
    }
}
