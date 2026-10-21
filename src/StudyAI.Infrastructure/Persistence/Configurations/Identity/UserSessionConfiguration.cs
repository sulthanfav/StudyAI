using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyAI.Domain.Identity;

namespace StudyAI.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions", "identity");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.Id)
            .HasColumnName("id");

        builder.Property(session => session.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(session => session.SessionIdentifierHash)
            .HasColumnName("session_identifier_hash")
            .HasColumnType("bytea")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(session => session.SecurityStampSnapshot)
            .HasColumnName("security_stamp_snapshot")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(session => session.IssuedAt)
            .HasColumnName("issued_at")
            .IsRequired();

        builder.Property(session => session.AbsoluteExpiresAt)
            .HasColumnName("absolute_expires_at")
            .IsRequired();

        builder.Property(session => session.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(session => session.LastSeenAt)
            .HasColumnName("last_seen_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(session => session.SessionIdentifierHash)
            .HasDatabaseName("ux_user_sessions_identifier_hash")
            .IsUnique();

        builder.HasIndex(session => new
            {
                session.UserId,
                session.RevokedAt,
                session.AbsoluteExpiresAt
            })
            .HasDatabaseName("ix_user_sessions_user_active_expiry");
    }
}
