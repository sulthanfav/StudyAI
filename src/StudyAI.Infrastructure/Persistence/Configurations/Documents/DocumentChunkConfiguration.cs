using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Documents;

namespace StudyAI.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks", "documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.DocumentVersionId)
            .HasColumnName("document_version_id")
            .IsRequired();

        builder.Property(x => x.ProcessingRunId)
            .HasColumnName("processing_run_id")
            .IsRequired();

        builder.Property(x => x.ChunkIndex)
            .HasColumnName("chunk_index")
            .IsRequired();

        builder.Property(x => x.ContentText)
            .HasColumnName("content_text")
            .IsRequired();

        builder.Property(x => x.LocatorJson)
            .HasColumnName("locator")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(x => x.TokenCount)
            .HasColumnName("token_count");

        builder.HasOne<DocumentVersion>()
            .WithMany()
            .HasForeignKey(x => x.DocumentVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProcessingRun>()
            .WithMany()
            .HasForeignKey(x => x.ProcessingRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Id, x.DataSpaceId })
            .IsUnique();

        builder.HasIndex(x => new { x.DocumentVersionId, x.ProcessingRunId, x.ChunkIndex })
            .IsUnique();
    }
}
