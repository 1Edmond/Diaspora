using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class MessageMentionConfiguration : BaseEntityConfiguration<MessageMention>
{
    public override void Configure(EntityTypeBuilder<MessageMention> builder)
    {
        base.Configure(builder);

        builder.ToTable("message_mentions");

        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ChatProfileId).HasFilter("chat_profile_id IS NOT NULL");

        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ChatProfileId);
        builder.Property(x => x.MentionGroupId);
    }
}