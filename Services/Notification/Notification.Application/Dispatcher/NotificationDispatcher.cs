using Notification.Application.Abstractions;
using Notification.Application.Models;
using Notification.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Dispatcher;

public class NotificationDispatcher
{
    private readonly IRepository<NotificationRule> _rules;
    private readonly IRepository<NotificationLog> _logs;
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationDispatcher(
        IRepository<NotificationRule> rules,
        IRepository<NotificationLog> logs,
        IEnumerable<INotificationSender> senders)
    {
        _rules = rules;
        _logs = logs;
        _senders = senders;
    }

    public async Task<NotificationLog?> DispatchAsync(NotificationContext context)
    {
        var alreadySent = await _logs.FirstOrDefaultAsync(x => x.MessageId == context.MessageId);
        if (alreadySent != null)
            return null;

        var rule = await _rules.FirstOrDefaultAsync(x => x.EventType == context.EventType && x.IsActive);
        if (rule == null)
            return null;

       
        var anySent = false;
        foreach (var sender in _senders)
        {
            if (rule.Channels.HasFlag(sender.Channel))
            {
                var result = await sender.SendAsync(context);
                anySent = anySent || result.IsSuccess;
            }
        }

        if (!anySent)
            return null;

        return new NotificationLog(context.MessageId, context.EventType);
    }
}