using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Notification.Domain.Enums;
using Shared.Domain.Commons;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Realtime;

public sealed class RedisNotificationPublisher : INotificationRealtimePublisher
{

    public NotificationChannel Channel => NotificationChannel.Push;

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisNotificationPublisher> _logger;

    public RedisNotificationPublisher(IConnectionMultiplexer redis, ILogger<RedisNotificationPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PublishToUserAsync(Guid userId, long sequence, Guid notificationId, CancellationToken ct = default)
    {
        var subscriber = _redis.GetSubscriber();
        var channel = RedisChannel.Literal($"notification:user:{userId}");
        var message = System.Text.Json.JsonSerializer.Serialize(new { sequence, id = notificationId });

        await subscriber.PublishAsync(channel, message);

        _logger.LogDebug(
            "Published notification {Id} (seq {Sequence}) to {Channel}",
            notificationId, sequence, channel);
    }

    public Task<Result> SendAsync(NotificationContext context)
    {
        throw new NotImplementedException();
    }
}