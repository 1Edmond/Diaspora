using Chat.Application.Commands;
using Chat.Application.Requests;
using Chat.Application.Responses;
using Chat.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace Chat.Infrastructure.Hubs;

public sealed class ChatHub(IMessageBus bus) : Hub
{
    // ── Groupes SignalR ───────────────────────────────────────────────────────
    // Chaque conversation = un groupe SignalR "conversation:{id}"
    // Chaque user connecté = groupe "user:{chatProfileId}"

    public override async Task OnConnectedAsync()
    {
        var chatProfileId = Context.User?.FindFirst("chat_profile_id")?.Value;
        if (!string.IsNullOrEmpty(chatProfileId))
        {
            // Rejoindre le groupe personnel de l'utilisateur
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"user:{chatProfileId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    // ── Rejoindre / quitter une conversation ──────────────────────────────────

    public async Task JoinConversation(Guid conversationId)
        => await Groups.AddToGroupAsync(
            Context.ConnectionId, $"conversation:{conversationId}");

    public async Task LeaveConversation(Guid conversationId)
        => await Groups.RemoveFromGroupAsync(
            Context.ConnectionId, $"conversation:{conversationId}");

    // ── Messages ──────────────────────────────────────────────────────────────

    public async Task SendTextMessage(
        Guid conversationId, Guid senderChatProfileId,
        string content, Guid? replyToMessageId = null,
        List<MentionRequest>? mentions = null)
    {
        var result = await bus.InvokeAsync<Result<SendMessageResponse>>(
            new SendMessageCommand(
                conversationId, senderChatProfileId,
                MessageType.Text, content, replyToMessageId,
                mentions ?? new List<MentionRequest>()));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task EditMessage(
        Guid messageId, Guid requestingChatProfileId, string newContent)
    {
        var result = await bus.InvokeAsync<Result>(
            new EditMessageCommand(messageId, requestingChatProfileId, newContent));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task DeleteMessage(Guid messageId, Guid requestingChatProfileId)
    {
        var result = await bus.InvokeAsync<Result>(
            new DeleteMessageCommand(messageId, requestingChatProfileId));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task AddReaction(Guid messageId, Guid chatProfileId, string emoji)
    {
        var result = await bus.InvokeAsync<Result>(
            new AddReactionCommand(messageId, chatProfileId, emoji));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task MarkAsRead(Guid conversationId, Guid chatProfileId)
    {
        await bus.InvokeAsync<Result>(
            new MarkAsReadCommand(conversationId, chatProfileId));
    }

    // ── Typing indicators ─────────────────────────────────────────────────────

    public async Task StartTyping(Guid conversationId, Guid chatProfileId, string displayName)
        => await Clients.OthersInGroup($"conversation:{conversationId}")
            .SendAsync("UserStartedTyping", new { conversationId, chatProfileId, displayName });

    public async Task StopTyping(Guid conversationId, Guid chatProfileId)
        => await Clients.OthersInGroup($"conversation:{conversationId}")
            .SendAsync("UserStoppedTyping", new { conversationId, chatProfileId });

    // ── Appels ────────────────────────────────────────────────────────────────

    public async Task StartCall(
        Guid conversationId, Guid initiatorChatProfileId, CallType type)
    {
        var result = await bus.InvokeAsync<Result<StartCallResponse>>(
            new StartCallCommand(conversationId, initiatorChatProfileId, type));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
        else
            await Clients.Caller.SendAsync("CallStarted", result.Value);
    }

    public async Task JoinCall(Guid callId, Guid chatProfileId)
    {
        var result = await bus.InvokeAsync<Result>(
            new JoinCallCommand(callId, chatProfileId));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
        else
            await Clients.OthersInGroup($"call:{callId}")
                .SendAsync("ParticipantJoined", new { callId, chatProfileId });

        await Groups.AddToGroupAsync(Context.ConnectionId, $"call:{callId}");
    }

    public async Task EndCall(Guid callId, Guid requestingChatProfileId)
    {
        var result = await bus.InvokeAsync<Result>(
            new EndCallCommand(callId, requestingChatProfileId));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    // ── WebRTC Signaling (ICE candidates, SDP offers/answers) ────────────────

    public async Task SendWebRtcOffer(Guid callId, Guid targetChatProfileId, object sdpOffer)
        => await Clients.Group($"user:{targetChatProfileId}")
            .SendAsync("WebRtcOffer", new { callId, sdpOffer });

    public async Task SendWebRtcAnswer(Guid callId, Guid targetChatProfileId, object sdpAnswer)
        => await Clients.Group($"user:{targetChatProfileId}")
            .SendAsync("WebRtcAnswer", new { callId, sdpAnswer });

    public async Task SendIceCandidate(Guid callId, Guid targetChatProfileId, object candidate)
        => await Clients.Group($"user:{targetChatProfileId}")
            .SendAsync("IceCandidate", new { callId, candidate });
}
