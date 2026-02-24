using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Inbox;

public class IdempotencyService : IIdempotencyService
{
    private readonly DbContext _context;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(DbContext context, ILogger<IdempotencyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsProcessedAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<InboxMessage>()
            .AnyAsync(
                m => m.MessageId == messageId
                  && m.Status == InboxMessageStatus.Processed,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<InboxMessage>()
            .AnyAsync(m => m.MessageId == messageId, cancellationToken);
    }

    public async Task<InboxMessage?> GetMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<InboxMessage>()
            .FirstOrDefaultAsync(m => m.MessageId == messageId, cancellationToken);
    }

    public async Task SaveInboxMessageAsync(
        InboxMessage message,
        CancellationToken cancellationToken = default)
    {
        // Vérifier si le message existe déjà
        var exists = await ExistsAsync(message.MessageId, cancellationToken);
        if (exists)
        {
            _logger.LogWarning(
                "Message {MessageId} already exists in inbox, skipping",
                message.MessageId);
            return;
        }

        await _context.Set<InboxMessage>().AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inbox message {MessageId} of type {MessageType} saved",
            message.MessageId,
            message.MessageType);
    }

    /// <summary>
    /// Exécute une action une seule fois de manière idempotente
    /// </summary>
    public async Task<T?> ProcessOnceAsync<T>(
        Guid messageId,
        string messageType,
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        // Vérifier si déjà traité
        if (await IsProcessedAsync(messageId, cancellationToken))
        {
            _logger.LogInformation(
                "Message {MessageId} already processed, skipping",
                messageId);
            return default;
        }

        // Vérifier si le message existe
        var message = await GetMessageAsync(messageId, cancellationToken);

        if (message == null)
        {
            // Créer un nouveau message inbox
            message = new InboxMessage(
                messageId,
                messageType,
                string.Empty
            );
            await SaveInboxMessageAsync(message, cancellationToken);
        }

        try
        {
            // Marquer comme en cours de traitement
            message.MarkAsProcessing();
            await _context.SaveChangesAsync(cancellationToken);

            // Exécuter l'action
            var result = await action();

            // Marquer comme traité
            message.MarkAsProcessed();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Message {MessageId} processed successfully",
                messageId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing message {MessageId}: {Error}",
                messageId,
                ex.Message);

            message.MarkAsFailed(ex.Message);
            await _context.SaveChangesAsync(cancellationToken);

            throw;
        }
    }
}