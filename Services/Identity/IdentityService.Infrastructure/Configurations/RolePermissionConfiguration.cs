using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(rp => rp.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rp => rp.PermissionKey)
            .HasColumnName("permission_key")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(rp => rp.GrantedAt)
            .HasColumnName("granted_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(rp => rp.GrantedBy)
            .HasColumnName("granted_by");

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionKey })
            .HasDatabaseName("idx_role_permissions_role_permission")
            .IsUnique();

        builder.HasIndex(rp => rp.RoleId)
            .HasDatabaseName("idx_role_permissions_role_id");

        builder.HasIndex(rp => rp.PermissionKey)
            .HasDatabaseName("idx_role_permissions_permission_key");
    }
}