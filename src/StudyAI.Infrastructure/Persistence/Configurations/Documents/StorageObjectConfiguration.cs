using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Documents;
using StudyAI.Domain.Ownership;

namespace StudyAI.Infrastructure.Persistence.Configurations.Documents;

public sealed class StorageObjectConfiguration : IEntityTypeConfiguration<StorageObject>
{
    public void Configure(EntityTypeBuilder<StorageObject> builder)
    {
        builder.ToTable("storage_objects", "documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasColumnName("storage_key")
            .IsRequired();

        builder.Property(x => x.ChecksumSha256)
            .HasColumnName("checksum_sha256")
            .IsRequired();

        builder.Property(x => x.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(x => x.MimeType)
            .HasColumnName("mime_type")
            .IsRequired();

        builder.Property(x => x.ChargeClass)
            .HasColumnName("charge_class")
            .HasDefaultValue("billable")
            .IsRequired();

        builder.Property(x => x.State)
            .HasColumnName("state")
            .HasDefaultValue("pending")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasOne<DataSpace>()
            .WithMany()
            .HasForeignKey(x => x.DataSpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.StorageKey)
            .IsUnique();

        builder.HasIndex(x => new { x.Id, x.DataSpaceId })
            .IsUnique();

        builder.HasIndex(x => new { x.DataSpaceId, x.ChecksumSha256 })
            .HasDatabaseName("ix_storage_owner_checksum")
            .HasFilter("state = 'verified'");
    }
}
