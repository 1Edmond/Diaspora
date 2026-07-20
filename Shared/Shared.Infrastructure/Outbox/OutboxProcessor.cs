using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using System.Text.Json;
using Wolverine;

namespace Shared.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
            }

        }

        _logger.LogInformation("Outbox Processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messages = await repository.GetUnprocessedMessagesAsync(20, cancellationToken);
        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var eventType = EventTypeRegistry.Resolve(message.Type);
                if (eventType == null)
                {
                    _logger.LogError(
                        "Unknown event type {Type} for outbox message {MessageId}",
                        message.Type,
                        message.Id
                    );
                    await repository.MarkAsFailedAsync(
                        message.Id,
                        $"Unknown event type {message.Type}",
                        cancellationToken
                    );
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event == null)
                {
                    _logger.LogError(
                        "Failed to deserialize outbox message {MessageId}",
                        message.Id
                    );
                    await repository.MarkAsFailedAsync(
                        message.Id,
                        "Failed to deserialize event",
                        cancellationToken
                    );
                    continue;
                }

                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    _logger.LogInformation(
                        "Publishing {EventType} (ID: {MessageId}) to RabbitMQ",
                        eventType.Name,
                        message.Id
                    );

                    
                    await messageBus.PublishAsync((dynamic)@event);

                    _logger.LogInformation(
                        "Successfully published {EventType} (ID: {MessageId})",
                        eventType.Name,
                        message.Id
                    );

                    await repository.MarkAsProcessedAsync(message.Id, cancellationToken);
                }, cancellationToken);

                _logger.LogInformation(
                    "Outbox message {MessageId} processed",
                    message.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}",
                    message.Id
                );
                await repository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }


}
