using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Messaging.RabbitMq.Abstractions;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq.Outbox;

/// <summary>
/// Background service that polls the outbox table for pending messages
/// and publishes them to RabbitMQ. Runs on a configurable interval.
/// </summary>
public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly int _batchSize;

    public OutboxDispatcher(
        IServiceProvider serviceProvider,
        IRabbitMqPublisher publisher,
        ILogger<OutboxDispatcher> logger,
        OutboxDispatcherSettings? settings = null)
    {
        _serviceProvider = serviceProvider;
        _publisher = publisher;
        _logger = logger;
        _pollingInterval = settings?.PollingInterval ?? TimeSpan.FromSeconds(5);
        _batchSize = settings?.BatchSize ?? 50;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxDispatcher started. Polling every {Interval}s, batch size {BatchSize}",
            _pollingInterval.TotalSeconds, _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing outbox batch");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var messages = await repository.GetPendingAsync(_batchSize, cancellationToken);

        if (messages.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublishAsync(
                    exchange: message.Exchange,
                    routingKey: message.RoutingKey,
                    message: JsonSerializer.Deserialize<object>(message.Payload)!,
                    messageId: message.MessageId,
                    correlationId: message.CorrelationId,
                    traceId: message.TraceId,
                    partnerId: message.PartnerId,
                    schemaVersion: message.SchemaVersion,
                    cancellationToken: cancellationToken);

                await repository.MarkAsSentAsync(message.MessageId, cancellationToken);

                _logger.LogInformation(
                    "Outbox message {MessageId} dispatched to {Exchange}/{RoutingKey}",
                    message.MessageId, message.Exchange, message.RoutingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to dispatch outbox message {MessageId}, retry count: {RetryCount}",
                    message.MessageId, message.RetryCount);

                await repository.MarkAsFailedAsync(message.MessageId, ex.Message, cancellationToken);
            }
        }
    }
}

/// <summary>
/// Settings for the OutboxDispatcher.
/// </summary>
public sealed class OutboxDispatcherSettings
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; set; } = 50;
}
