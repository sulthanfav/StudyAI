using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Knowledge;
using StudyAI.Domain.Ownership;

namespace StudyAI.Infrastructure.Persistence.Configurations.Knowledge;

public sealed class KnowledgeItemConfiguration : IEntityTypeConfiguration<KnowledgeItem>
{
    public void Configure(EntityTypeBuilder<KnowledgeItem> builder)
    {
        builder.ToTable("knowledge_items", "knowledge");

        builder.HasKey(x => x.Id);
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasOne<DataSpace>()
            .WithMany()
            .HasForeignKey(x => x.DataSpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Id, x.DataSpaceId })
            .IsUnique();

        builder.HasIndex(x => new { x.Id, x.DataSpaceId, x.Kind })
            .IsUnique();

        builder.HasIndex(x => new { x.DataSpaceId, x.UpdatedAt, x.Id })
            .HasDatabaseName("ix_knowledge_owner_recent")
            .HasFilter("deleted_at IS NULL");
    }
}
