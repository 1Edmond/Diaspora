using Microsoft.EntityFrameworkCore;

namespace Shared.Messaging.RabbitMq.Outbox;

/// <summary>
/// Outbox message entity for reliable messaging via the Transactional Outbox Pattern.
/// Stored in PostgreSQL alongside business data in the same transaction.
/// </summary>
public sealed class OutboxMessage
{
    public Guid MessageId { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }

    // Tracing context for OpenTelemetry propagation
    public string? TraceId { get; set; }
    public string? CorrelationId { get; set; }
    public string? PartnerId { get; set; }
    public string? SchemaVersion { get; set; }
}

public enum OutboxMessageStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

/// <summary>
/// EF Core configuration for outbox_messages table.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.MessageId);
        builder.Property(m => m.MessageId).HasColumnName("message_id");

        builder.Property(m => m.Exchange).HasColumnName("exchange").HasMaxLength(256).IsRequired();
        builder.Property(m => m.RoutingKey).HasColumnName("routing_key").HasMaxLength(256).IsRequired();
        builder.Property(m => m.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.MessageType).HasColumnName("message_type").HasMaxLength(512).IsRequired();

        builder.Property(m => m.Status).HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(m => m.SentAt).HasColumnName("sent_at").HasColumnType("timestamptz");
        builder.Property(m => m.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
        builder.Property(m => m.Error).HasColumnName("error").HasMaxLength(2000);

        builder.Property(m => m.TraceId).HasColumnName("trace_id").HasMaxLength(128);
        builder.Property(m => m.CorrelationId).HasColumnName("correlation_id").HasMaxLength(128);
        builder.Property(m => m.PartnerId).HasColumnName("partner_id").HasMaxLength(128);
        builder.Property(m => m.SchemaVersion).HasColumnName("schema_version").HasMaxLength(50);

        // Index for polling PENDING messages
        builder.HasIndex(m => new { m.Status, m.CreatedAt })
            .HasDatabaseName("ix_outbox_messages_status_created");
    }
}
