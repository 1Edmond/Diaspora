using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Configurations;

public sealed class MessageAttachmentConfiguration : BaseEntityConfiguration<MessageAttachment>
{
    public override void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        base.Configure(builder);

        builder.ToTable("message_attachments");

        builder.HasIndex(x => x.MessageId);

        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.DurationMs);
        builder.Property(x => x.Width);
        builder.Property(x => x.Height);
    }
}