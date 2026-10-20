using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Ownership;

namespace StudyAI.Infrastructure.Persistence.Configurations.Ownership;

public sealed class DataSpaceConfiguration : IEntityTypeConfiguration<DataSpace>
{
    public void Configure(EntityTypeBuilder<DataSpace> builder)
    {
        builder.ToTable("data_spaces", "ownership");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasDefaultValue("personal")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Id, x.OwnerUserId })
            .IsUnique();

        builder.HasIndex(x => x.OwnerUserId)
            .HasDatabaseName("ux_one_personal_space")
            .HasFilter("kind = 'personal'")
            .IsUnique();
    }
}
