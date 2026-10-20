using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pgvector;
using StudyAI.Domain.Ai;
using StudyAI.Domain.Documents;

namespace StudyAI.Infrastructure.Persistence.Configurations.Documents;

public sealed class ChunkEmbeddingConfiguration : IEntityTypeConfiguration<ChunkEmbedding>
{
    public void Configure(EntityTypeBuilder<ChunkEmbedding> builder)
    {
        builder.ToTable("chunk_embeddings", "documents");

        builder.HasKey(x => new { x.ChunkId, x.EmbeddingModelId });

        builder.Property(x => x.ChunkId)
            .HasColumnName("chunk_id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.EmbeddingModelId)
            .HasColumnName("embedding_model_id");

        var vectorConverter = new ValueConverter<float[], Vector>(
            floats => new Vector(floats),
            vec => vec.ToArray());

        builder.Property(x => x.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector")
            .HasConversion(vectorConverter)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<DocumentChunk>()
            .WithMany()
            .HasForeignKey(x => x.ChunkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EmbeddingModel>()
            .WithMany()
            .HasForeignKey(x => x.EmbeddingModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.DataSpaceId, x.EmbeddingModelId })
            .HasDatabaseName("ix_embeddings_tenant");
    }
}
