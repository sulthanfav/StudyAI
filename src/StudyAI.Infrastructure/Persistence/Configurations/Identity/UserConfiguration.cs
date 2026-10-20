using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Identity;

namespace StudyAI.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");

        builder.HasKey(x => x.Id);
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasColumnName("normalized_email")
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasColumnName("user_name")
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(x => x.NormalizedUserName)
            .HasColumnName("normalized_user_name")
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(x => x.SecurityStamp)
            .HasColumnName("security_stamp")
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(x => x.ConcurrencyStamp)
            .HasColumnName("concurrency_stamp")
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(x => x.EmailConfirmed)
            .HasColumnName("email_confirmed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.LockoutEnd)
            .HasColumnName("lockout_end");

        builder.Property(x => x.LockoutEnabled)
            .HasColumnName("lockout_enabled")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.AccessFailedCount)
            .HasColumnName("access_failed_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasDefaultValue("active")
            .IsRequired();

        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasDefaultValue("id-ID")
            .IsRequired();

        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasDefaultValue("Asia/Jakarta")
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

        builder.HasIndex(x => x.Email)
            .HasDatabaseName("ux_users_email_normalized")
            .IsUnique();
    }
}
