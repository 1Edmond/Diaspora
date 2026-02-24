using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Shared.Infrastructure.Inbox;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.MessageId)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => new { x.Status, x.ReceivedAt });

        builder.HasIndex(x => x.CorrelationId);

        builder.Property(x => x.MessageId)
            .IsRequired();

        builder.Property(x => x.MessageType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.Property(x => x.Error)
            .HasMaxLength(4000);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property(x => x.SourceSystem)
            .HasMaxLength(100);
    }
}