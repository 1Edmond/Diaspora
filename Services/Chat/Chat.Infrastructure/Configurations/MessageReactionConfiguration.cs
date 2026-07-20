using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class MessageReactionConfiguration : BaseEntityConfiguration<MessageReaction>
{
    public override void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("message_reactions");

        builder.HasIndex(x => new { x.MessageId, x.ChatProfileId, x.Emoji }).IsUnique();

        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.ChatProfileId).IsRequired();
        builder.Property(x => x.Emoji).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ReactedAt).IsRequired();
    }
}