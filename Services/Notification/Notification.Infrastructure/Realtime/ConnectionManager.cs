using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Realtime;


public sealed class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConnectionEntry> _connections = new();
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger;
    }

    public Task<bool> AddConnectionAsync(Guid userId, HttpResponse response, CancellationToken ct)
    {
        var entry = new ConnectionEntry(response, ct);
        _connections[userId] = entry; // remplace toute connexion précédente pour ce user

        _logger.LogInformation("SSE connection added for user {UserId}", userId);
        return Task.FromResult(true);
    }

    public void RemoveConnection(Guid userId)
    {
        if (_connections.TryRemove(userId, out _))
        {
            _logger.LogInformation("SSE connection removed for user {UserId}", userId);
        }
    }

    public bool IsConnected(Guid userId) => _connections.ContainsKey(userId);

    public async Task SendToUserAsync(Guid userId, long sequence, Guid notificationId)
    {
        if (!_connections.TryGetValue(userId, out var entry)) return;

        if (entry.Ct.IsCancellationRequested)
        {
            RemoveConnection(userId);
            return;
        }

        try
        {
            await entry.WriteLock.WaitAsync(entry.Ct);
            try
            {
                var payload =
                    $"id: {sequence}\n" +
                    $"event: notification.created\n" +
                    $"data: {{\"id\":\"{notificationId}\",\"sequence\":{sequence}}}\n\n";

                var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
                await entry.Response.Body.WriteAsync(bytes, entry.Ct);
                await entry.Response.Body.FlushAsync(entry.Ct);
            }
            finally
            {
                entry.WriteLock.Release();
            }
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or OperationCanceledException)
        {
            _logger.LogInformation(ex, "Write failed for user {UserId}, removing connection", userId);
            RemoveConnection(userId);
        }
    }

    private sealed class ConnectionEntry
    {
        public HttpResponse Response { get; }
        public CancellationToken Ct { get; }
        public SemaphoreSlim WriteLock { get; } = new(1, 1);

        public ConnectionEntry(HttpResponse response, CancellationToken ct)
        {
            Response = response;
            Ct = ct;
        }
    }
}