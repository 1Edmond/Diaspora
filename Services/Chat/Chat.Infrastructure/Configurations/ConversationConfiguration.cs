using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class ConversationConfiguration : BaseEntityConfiguration<Conversation>
{
    public override void Configure(EntityTypeBuilder<Conversation> builder)
    {
        base.Configure(builder);

        builder.ToTable("conversations");

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.CreatedByChatProfileId);

        builder.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedByChatProfileId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasMany(x => x.Participants)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MentionGroups)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
