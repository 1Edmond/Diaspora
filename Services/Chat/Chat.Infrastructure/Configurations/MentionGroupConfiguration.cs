using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class MentionGroupConfiguration : BaseEntityConfiguration<MentionGroup>
{
    public override void Configure(EntityTypeBuilder<MentionGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("mention_groups");

        builder.HasIndex(x => new { x.ConversationId, x.Name }).IsUnique();

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(x => x.MentionGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}