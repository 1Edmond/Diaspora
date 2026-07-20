using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Realtime;

public sealed class RedisNotificationSubscriberService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RedisNotificationSubscriberService> _logger;

    private record RedisNotificationMessage(long Sequence, Guid Id);

    public RedisNotificationSubscriberService(
        IConnectionMultiplexer redis,
        IConnectionManager connectionManager,
        ILogger<RedisNotificationSubscriberService> logger)
    {
        _redis = redis;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Pattern("notification:user:*"),
            async (channel, message) =>
            {
                try
                {
                    await HandleMessageAsync(channel, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling Redis message on channel {Channel}", channel);
                }
            });

        _logger.LogInformation("Subscribed to notification:user:* pattern");
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });
    }

    private async Task HandleMessageAsync(RedisChannel channel, RedisValue message)
    {
        const string prefix = "notification:user:";
        var channelName = channel.ToString();
        if (!channelName.StartsWith(prefix, StringComparison.Ordinal)) return;

        if (!Guid.TryParse(channelName[prefix.Length..], out var userId))
        {
            _logger.LogWarning("Could not parse userId from channel {Channel}", channelName);
            return;
        }

        //var payload = System.Text.Json.JsonSerializer.Deserialize<RedisNotificationMessage>(message!);
        //if (payload is null) return;

        //// Si l'utilisateur n'est pas connecté sur cette instance → ignoré silencieusement
        //await _connectionManager.SendToUserAsync(userId, payload.Sequence, payload.Id);
    }
}
