
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Text.Json;

public class InAppNotificationSender : INotificationSender
{
    private readonly IRepository<UserNotification> _userNotifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotificationOutbox _realtimeOutbox;
    private readonly ILogger<InAppNotificationSender> _logger;
    private readonly IReadOnlyDictionary<string, IInAppNotificationPayloadMapper> _mappers;

    public InAppNotificationSender(
        IRepository<UserNotification> userNotifications,
        IUnitOfWork unitOfWork,
        IRealtimeNotificationOutbox realtimeOutbox,
        IEnumerable<IInAppNotificationPayloadMapper> mappers, // NOUVEAU
        ILogger<InAppNotificationSender> logger)
    {
        _userNotifications = userNotifications;
        _unitOfWork = unitOfWork;
        _realtimeOutbox = realtimeOutbox;
        _logger = logger;
        _mappers = mappers.ToDictionary(m => m.EventType, m => m);
    }

    public NotificationChannel Channel => NotificationChannel.InApp;

    public async Task<Result> SendAsync(NotificationContext context)
    {
        var payload = ExtractPayload(context);
        if (payload is null || payload.UserId == Guid.Empty)
        {
            _logger.LogError(
                "[InAppNotificationSender] Invalid or missing payload for message {MessageId} (event: {EventType})",
                context.MessageId, context.EventType);
            return Result.Failure("Invalid in-app payload");
        }

        try
        {
            var userNotification = new UserNotification(
                payload.UserId,
                context.EventType,
                JsonSerializer.Serialize(payload),
                messageId: context.MessageId);

            await _userNotifications.AddAsync(userNotification);
            await _unitOfWork.SaveChangesAsync();

            _realtimeOutbox.Enqueue(payload.UserId, userNotification.Sequence, userNotification.Id);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                    "[InAppNotificationSender] In-app notification queued (post-commit) for user {UserId}, message {MessageId}",
                    payload.UserId, context.MessageId);

            return Result.Success("In-app notification created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[InAppNotificationSender] Failed to create in-app notification for message {MessageId}",
                context.MessageId);
            return Result.Failure($"Failed to create in-app notification: {ex.Message}");
        }
    }

    private InAppNotificationPayload? ExtractPayload(NotificationContext context)
    {
        try
        {
            // context.Payload est un JsonElement à ce stade (désérialisé depuis
            // InboxMessage.Content via NotificationContext.Payload : object)
            if (context.Payload is JsonElement rawJson)
            {
                // 1. Priorité au mapper dédié à ce type d'event, s'il existe
                if (_mappers.TryGetValue(context.EventType, out var mapper))
                {
                    var mapped = mapper.Map(rawJson);
                    if (mapped is null)
                        _logger.LogWarning(
                            "[InAppNotificationSender] Mapper for {EventType} returned null for message {MessageId}",
                            context.EventType, context.MessageId);
                    return mapped;
                }

                // 2. Fallback : le payload est déjà au format InAppNotificationPayload
                // (utile si un event est publié directement avec Title/Body/Data)
                return rawJson.Deserialize<InAppNotificationPayload>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // Cas où context.Payload n'est pas un JsonElement (objet typé direct, tests unitaires...)
            return context.Payload switch
            {
                InAppNotificationPayload typed => typed,
                string json => JsonSerializer.Deserialize<InAppNotificationPayload>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                _ => JsonSerializer.Deserialize<InAppNotificationPayload>(
                    JsonSerializer.Serialize(context.Payload),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex,
                "[InAppNotificationSender] Failed to deserialize payload for message {MessageId}",
                context.MessageId);
            return null;
        }
    }
}