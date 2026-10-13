using Chat.Domain.Entities;
using Chat.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class ChatProfileConfiguration : BaseEntityConfiguration<ChatProfile>
{
    public override void Configure(EntityTypeBuilder<ChatProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("chat_profiles");

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ProfileId).HasColumnName("profile_id");
        builder.Property(x => x.ProfileTypeId);
        builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.IsBot).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.BotToken).HasMaxLength(500);
        builder.Property(x => x.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired()
            .HasDefaultValue(ChatProfileStatus.Offline);
        builder.Property(x => x.LastSeenAt);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);


        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.ProfileId).IsUnique()
            .HasFilter("profile_id IS NOT NULL");
    }
}