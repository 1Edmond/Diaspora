using Chat.Application.Dtos;
using Chat.Application.Interfaces;
using Chat.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Services;

public sealed class ChatNotificationService(IHubContext<ChatHub> hubContext)
    : IChatNotificationService
{
    public async Task NotifyMessageSent(
        Guid conversationId, MessageDto message, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("MessageReceived", message, ct);

    public async Task NotifyMessageEdited(
        Guid conversationId, Guid messageId, string newContent, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("MessageEdited", new { messageId, newContent }, ct);

    public async Task NotifyMessageDeleted(
        Guid conversationId, Guid messageId, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("MessageDeleted", new { messageId }, ct);

    public async Task NotifyReactionAdded(
        Guid conversationId, Guid messageId, Guid chatProfileId,
        string emoji, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("ReactionAdded", new { messageId, chatProfileId, emoji }, ct);

    public async Task NotifyParticipantAdded(
        Guid conversationId, Guid chatProfileId, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("ParticipantAdded", new { conversationId, chatProfileId }, ct);

    public async Task NotifyCallStarted(
        Guid conversationId, Guid callId, Guid initiatorChatProfileId,
        string callType, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("IncomingCall", new { callId, initiatorChatProfileId, callType }, ct);

    public async Task NotifyCallEnded(
        Guid conversationId, Guid callId, string status, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("CallEnded", new { callId, status }, ct);

    public async Task NotifyProfileStatusChanged(
        Guid chatProfileId, string status, CancellationToken ct = default)
        => await hubContext.Clients
            .Group($"user:{chatProfileId}")
            .SendAsync("StatusChanged", new { chatProfileId, status }, ct);
}