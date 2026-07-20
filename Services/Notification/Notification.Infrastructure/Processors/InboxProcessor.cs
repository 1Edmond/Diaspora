using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Dispatcher;
using Notification.Application.Models;
using Notification.Domain.Entities;
using Shared.Application.Abstractions;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Inbox;
using System.Text.Json;

namespace Notification.Infrastructure.Processors;

public class InboxProcessor : IInboxProcessor
{
    private readonly IRepository<InboxMessage> _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<NotificationLog> _logs;
    private readonly NotificationDispatcher _notificationDispatcher;
    private readonly IRealtimeNotificationOutbox _realtimeOutbox; // NOUVEAU
    private readonly INotificationRealtimePublisher _realtimePublisher; // NOUVEAU
    private readonly ILogger<InboxProcessor> _logger;
    private const int MaxRetryAttempts = 3;

    public InboxProcessor(
        IRepository<InboxMessage> inboxRepository,
        IUnitOfWork unitOfWork,
        IRepository<NotificationLog> logs,
        NotificationDispatcher notificationDispatcher,
        IRealtimeNotificationOutbox realtimeOutbox,
        INotificationRealtimePublisher realtimePublisher,
        ILogger<InboxProcessor> logger)
    {
        _inboxRepository = inboxRepository;
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
        _realtimeOutbox = realtimeOutbox;
        _realtimePublisher = realtimePublisher;
        _logger = logger;
        _logs = logs;
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var pendingMessages = (await _inboxRepository.FindAsync(
           x => x.Status == InboxMessageStatus.Pending
             || (x.Status == InboxMessageStatus.Failed && x.ProcessingAttempts < MaxRetryAttempts),
           cancellationToken))
           .OrderBy(x => x.ReceivedAt)
           .Take(100)
           .ToList();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Found {Count} pending inbox messages to process", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessMessageAsync(message.MessageId, cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _inboxRepository.FirstOrDefaultAsync(x => x.MessageId == messageId, cancellationToken);

        if (message == null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Inbox message {MessageId} not found", messageId);
            return;
        }

        if (message.Status == InboxMessageStatus.Processed)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Inbox message {MessageId} already processed", messageId);
            return;
        }

        var transactionSucceeded = false;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            try
            {
                message.MarkAsProcessing();
                await _inboxRepository.UpdateAsync(message, cancellationToken);

                var notificationContext = JsonSerializer.Deserialize<NotificationContext>(message.Content)
                        ?? throw new InvalidOperationException($"Failed to deserialize notification context for message {messageId}");

                var log = await _notificationDispatcher.DispatchAsync(notificationContext) ?? throw new InvalidOperationException(
                        $"Failed to process notification for message {messageId}");

                await _logs.AddAsync(log, cancellationToken);
                message.MarkAsProcessed();
                await _inboxRepository.UpdateAsync(message, cancellationToken);

                transactionSucceeded = true;

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation(
                        "Successfully processed inbox message {MessageId} for event type {EventType}",
                        message.MessageId, notificationContext.EventType);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Error processing inbox message {MessageId}: {Error}", message.MessageId, ex.Message);

                message.MarkAsFailed(ex.Message);
                await _inboxRepository.UpdateAsync(message, cancellationToken);

                if (!message.CanRetry(MaxRetryAttempts))
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning(
                            "Inbox message {MessageId} has reached max retry attempts ({Attempts}). Manual intervention may be required.",
                            message.MessageId, message.ProcessingAttempts);
                }

                // IMPORTANT : on vide l'outbox même en cas d'échec, pour ne pas
                // publier au prochain message des entrées orphelines d'une
                // transaction qui vient d'être rollback.
                _realtimeOutbox.DequeueAll();
            }
        }, cancellationToken);

        // NOUVEAU : publication SSE uniquement APRÈS commit réussi de la transaction.
        if (transactionSucceeded)
        {
            var pending = _realtimeOutbox.DequeueAll();
            foreach (var item in pending)
            {
                await _realtimePublisher.PublishToUserAsync(
                    item.UserId, item.Sequence, item.NotificationId, cancellationToken);
            }
        }
    }
}