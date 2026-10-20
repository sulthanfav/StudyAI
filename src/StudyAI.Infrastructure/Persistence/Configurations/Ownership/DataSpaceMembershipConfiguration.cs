using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Identity;
using StudyAI.Domain.Ownership;

namespace StudyAI.Infrastructure.Persistence.Configurations.Ownership;

public sealed class DataSpaceMembershipConfiguration : IEntityTypeConfiguration<DataSpaceMembership>
{
    public void Configure(EntityTypeBuilder<DataSpaceMembership> builder)
    {
        builder.ToTable("data_space_memberships", "ownership");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DataSpaceId)
            .HasColumnName("data_space_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasDefaultValue(DataSpaceMembership.RoleOwner)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<DataSpace>()
            .WithMany()
            .HasForeignKey(x => x.DataSpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.DataSpaceId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_data_space_memberships_user");
    }
}
