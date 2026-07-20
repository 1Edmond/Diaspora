using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Configurations;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(n => n.Id); // Guid — clé métier héritée d'AggregateRoot

        builder.Property(n => n.Sequence)
            .UseIdentityAlwaysColumn(); // Npgsql : bigserial contrôlé par la DB, jamais par l'app

        builder.HasIndex(n => n.Sequence).IsUnique();
        builder.HasIndex(n => new { n.UserId, n.Sequence });
        builder.HasIndex(n => new { n.UserId, n.IsRead });

        builder.Property(n => n.EventType).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Payload).HasColumnType("jsonb");
    }
}