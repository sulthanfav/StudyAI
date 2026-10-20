using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Documents;
using StudyAI.Domain.Knowledge;

namespace StudyAI.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents", "documents");

        builder.HasKey(x => x.KnowledgeItemId);

        builder.Property(x => x.KnowledgeItemId)
            .HasColumnName("knowledge_item_id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.ItemKind)
            .HasColumnName("item_kind")
            .HasDefaultValue("document")
            .IsRequired();

        builder.Property(x => x.OriginalFilename)
            .HasColumnName("original_filename")
            .IsRequired();

        builder.Property(x => x.CurrentVersionId)
            .HasColumnName("current_version_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<KnowledgeItem>()
            .WithMany()
            .HasForeignKey(x => x.KnowledgeItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.KnowledgeItemId, x.DataSpaceId })
            .IsUnique();
    }
}
