using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("user_permissions");

        builder.HasKey(up => up.Id);

        builder.Property(up => up.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(up => up.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(up => up.PermissionKey)
            .HasColumnName("permission_key")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(up => up.IsGranted)
            .HasColumnName("is_granted")
            .IsRequired();

        builder.Property(up => up.GrantedAt)
            .HasColumnName("granted_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(up => up.GrantedBy)
            .HasColumnName("granted_by");

        builder.Property(up => up.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(up => new { up.UserId, up.PermissionKey })
            .HasDatabaseName("idx_user_permissions_user_permission")
            .IsUnique();

        builder.HasIndex(up => up.UserId)
            .HasDatabaseName("idx_user_permissions_user_id");

        builder.HasIndex(up => up.PermissionKey)
            .HasDatabaseName("idx_user_permissions_permission_key");
    }
}