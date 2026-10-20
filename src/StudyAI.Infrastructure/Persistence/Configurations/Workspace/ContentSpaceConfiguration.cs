using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Ownership;
using StudyAI.Domain.Workspace;

namespace StudyAI.Infrastructure.Persistence.Configurations.Workspace;

public sealed class ContentSpaceConfiguration : IEntityTypeConfiguration<ContentSpace>
{
    public void Configure(EntityTypeBuilder<ContentSpace> builder)
    {
        builder.ToTable("content_spaces", "workspace");

        builder.HasKey(x => x.Id);

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

        builder.Property(x => x.Position)
            .HasColumnName("position")
            .HasDefaultValue(0)
            .IsRequired();

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

        builder.HasIndex(x => x.DataSpaceId)
            .HasDatabaseName("ux_library_per_owner")
            .HasFilter("kind = 'library'")
            .IsUnique();

        builder.HasIndex(x => new { x.DataSpaceId, x.Kind, x.Position })
            .HasDatabaseName("ix_content_spaces_owner")
            .HasFilter("deleted_at IS NULL");
    }
}
