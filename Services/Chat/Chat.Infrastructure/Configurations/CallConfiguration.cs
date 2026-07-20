using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class CallConfiguration : BaseEntityConfiguration<Call>
{
    public override void Configure(EntityTypeBuilder<Call> builder)
    {
        base.Configure(builder);

        builder.ToTable("calls");

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.Status);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.InitiatorChatProfileId).IsRequired();
        builder.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.StartedAt);
        builder.Property(x => x.EndedAt);

        builder.Ignore(x => x.Duration); // computed property

        builder.HasMany(x => x.Participants)
            .WithOne()
            .HasForeignKey(x => x.CallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
