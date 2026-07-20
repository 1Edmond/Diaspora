using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class ConversationParticipantConfiguration
    : BaseEntityConfiguration<ConversationParticipant>
{
    public override void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        base.Configure(builder);

        builder.ToTable("conversation_participants");

        builder.HasIndex(x => new { x.ConversationId, x.ChatProfileId }).IsUnique();
        builder.HasIndex(x => x.ChatProfileId);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.ChatProfileId).IsRequired();
        builder.Property(x => x.Role)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();
        builder.Property(x => x.LastReadAt);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasOne(x => x.ChatProfile)
            .WithMany()
            .HasForeignKey(x => x.ChatProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
