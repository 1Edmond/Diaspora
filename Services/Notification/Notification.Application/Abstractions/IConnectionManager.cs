using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface IConnectionManager
{
    Task<bool> AddConnectionAsync(Guid userId, HttpResponse response, CancellationToken ct);
    void RemoveConnection(Guid userId);
    Task SendToUserAsync(Guid userId, long sequence, Guid notificationId);
    bool IsConnected(Guid userId);
}
