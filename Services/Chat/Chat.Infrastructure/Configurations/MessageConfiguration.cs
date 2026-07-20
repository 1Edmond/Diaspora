using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class MessageConfiguration : BaseEntityConfiguration<Message>
{
    public override void Configure(EntityTypeBuilder<Message> builder)
    {
        base.Configure(builder);

        builder.ToTable("messages");

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.SenderChatProfileId);
        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt });

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.SenderChatProfileId).IsRequired();
        builder.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000);
        builder.Property(x => x.ReplyToMessageId);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.EditedAt);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reactions)
            .WithOne()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Mentions)
            .WithOne()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
