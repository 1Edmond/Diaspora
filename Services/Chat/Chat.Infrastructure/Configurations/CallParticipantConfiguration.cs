using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class CallParticipantConfiguration : BaseEntityConfiguration<CallParticipant>
{
    public override void Configure(EntityTypeBuilder<CallParticipant> builder)
    {
        base.Configure(builder);

        builder.ToTable("call_participants");

        builder.HasIndex(x => new { x.CallId, x.ChatProfileId }).IsUnique();

        builder.Property(x => x.CallId).IsRequired();
        builder.Property(x => x.ChatProfileId).IsRequired();
        builder.Property(x => x.InvitedAt).IsRequired();
        builder.Property(x => x.JoinedAt);
        builder.Property(x => x.LeftAt);
        builder.Property(x => x.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.ChatProfile)
            .WithMany()
            .HasForeignKey(x => x.ChatProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}