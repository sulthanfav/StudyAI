using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Documents;

namespace StudyAI.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_versions", "documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .IsRequired();

        builder.Property(x => x.VersionNo)
            .HasColumnName("version_no")
            .IsRequired();

        builder.Property(x => x.StorageObjectId)
            .HasColumnName("storage_object_id")
            .IsRequired();

        builder.Property(x => x.PageCount)
            .HasColumnName("page_count");

        builder.Property(x => x.ProcessingStatus)
            .HasColumnName("processing_status")
            .HasDefaultValue("pending")
            .IsRequired();

        builder.Property(x => x.ProcessingVersion)
            .HasColumnName("processing_version");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StorageObject>()
            .WithMany()
            .HasForeignKey(x => x.StorageObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Id, x.DataSpaceId })
            .IsUnique();

        builder.HasIndex(x => new { x.Id, x.DataSpaceId, x.DocumentId })
            .IsUnique();

        builder.HasIndex(x => new { x.DocumentId, x.VersionNo })
            .IsUnique();
    }
}
