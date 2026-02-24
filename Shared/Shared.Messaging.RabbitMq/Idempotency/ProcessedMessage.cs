using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Messaging.RabbitMq.Idempotency;

/// <summary>
/// Tracks processed messages for consumer idempotency (inbox pattern).
/// Before processing a message, consumers check if the MessageId has been seen.
/// </summary>
public sealed class ProcessedMessage
{
    public Guid MessageId { get; set; }
    public string ConsumerName { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// EF Core configuration for processed_messages table.
/// </summary>
public sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("processed_messages");

        builder.HasKey(m => new { m.MessageId, m.ConsumerName });

        builder.Property(m => m.MessageId).HasColumnName("message_id");
        builder.Property(m => m.ConsumerName).HasColumnName("consumer_name").HasMaxLength(256);
        builder.Property(m => m.ProcessedAt).HasColumnName("processed_at").HasColumnType("timestamptz");

        builder.HasIndex(m => m.ProcessedAt)
            .HasDatabaseName("ix_processed_messages_processed_at");
    }
}
