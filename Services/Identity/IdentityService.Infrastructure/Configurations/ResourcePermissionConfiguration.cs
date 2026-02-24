using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations;

public class ResourcePermissionConfiguration : IEntityTypeConfiguration<ResourcePermission>
{
    public void Configure(EntityTypeBuilder<ResourcePermission> builder)
    {
        builder.ToTable("resource_permissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(rp => rp.ServiceName)
            .HasColumnName("service_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rp => rp.ResourceName)
            .HasColumnName("resource_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rp => rp.ResourceKey)
            .HasColumnName("resource_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rp => rp.Permission)
            .HasColumnName("permission")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rp => rp.PermissionKey)
            .HasColumnName("permission_key")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(rp => rp.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(rp => rp.RegisteredAt)
            .HasColumnName("registered_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(rp => rp.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(rp => rp.PermissionKey)
            .HasDatabaseName("idx_resource_permissions_key")
            .IsUnique();

        builder.HasIndex(rp => rp.ServiceName)
            .HasDatabaseName("idx_resource_permissions_service");

        builder.HasIndex(rp => rp.IsActive)
            .HasDatabaseName("idx_resource_permissions_active");
    }
}