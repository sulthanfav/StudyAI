using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Ai;

namespace StudyAI.Infrastructure.Persistence.Configurations.Ai;

public sealed class EmbeddingModelConfiguration : IEntityTypeConfiguration<EmbeddingModel>
{
    public void Configure(EntityTypeBuilder<EmbeddingModel> builder)
    {
        builder.ToTable("embedding_models", "ai");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .IsRequired();

        builder.Property(x => x.ModelKey)
            .HasColumnName("model_key")
            .IsRequired();

        builder.Property(x => x.Dimensions)
            .HasColumnName("dimensions")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ModelKey, x.Dimensions })
            .IsUnique();
    }
}
