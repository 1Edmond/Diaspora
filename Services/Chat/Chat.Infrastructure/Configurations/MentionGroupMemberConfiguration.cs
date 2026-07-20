using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class MentionGroupMemberConfiguration
    : BaseEntityConfiguration<MentionGroupMember>
{
    public override void Configure(EntityTypeBuilder<MentionGroupMember> builder)
    {
        base.Configure(builder);

        builder.ToTable("mention_group_members");

        builder.HasIndex(x => new { x.MentionGroupId, x.ChatProfileId }).IsUnique();

        builder.Property(x => x.MentionGroupId).IsRequired();
        builder.Property(x => x.ChatProfileId).IsRequired();
    }
}