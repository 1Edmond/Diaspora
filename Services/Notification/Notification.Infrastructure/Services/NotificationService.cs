using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Shared.Infrastructure.Inbox;
using System.Text.Json;

namespace Notification.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IIdempotencyService idempotencyService,
        ILogger<NotificationService> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task EnqueueNotificationAsync(
        NotificationContext context,
        CancellationToken cancellationToken = default)
    {
      
        if (await _idempotencyService.IsProcessedAsync(context.MessageId, cancellationToken))
        {
            _logger.LogInformation(
                "Notification {MessageId} already processed, skipping",
                context.MessageId);
            return;
        }


        var inboxMessage = new InboxMessage(
            messageId: context.MessageId,
            messageType: context.EventType,
            content: JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                WriteIndented = false
            }),
            correlationId: context.MessageId.ToString(),
            sourceSystem: "NotificationService"
        );

        await _idempotencyService.SaveInboxMessageAsync(inboxMessage, cancellationToken);

        _logger.LogInformation(
            "Notification {MessageId} for event {EventType} saved to inbox",
            context.MessageId,
            context.EventType);
    }
}