using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Requests;
using Chat.Application.Responses;
using Chat.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Domain.Commons;
using System.Security.Claims;
using Wolverine;

namespace Chat.Infrastructure.Hubs;

[Authorize]
public sealed class ChatHub(IMessageBus bus, IChatProfileRepository chatProfileRepository) : Hub
{
    // ── Groupes SignalR ───────────────────────────────────────────────────────
    // Chaque conversation = un groupe SignalR "conversation:{id}"
    // Chaque user connecté = groupe "user:{chatProfileId}"
    //
    // ChatProfileId n'est plus jamais fait confiance depuis le client : résolu
    // une fois à la connexion à partir du UserId du JWT (fiable, posé par
    // GatewayAuthenticationHandler), puis mis en cache dans Context.Items pour
    // toute la durée de la connexion WebSocket.

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var chatProfile = await chatProfileRepository.GetByUserIdAsync(userId);

        if (chatProfile is null)
        {
            // Pas de ChatProfile pour cet utilisateur : on le laisse connecté
            // (health checks, futurs cas d'usage), mais aucune méthode de ce
            // Hub ne pourra rien faire pour lui — voir RequireChatProfileId().
            await base.OnConnectedAsync();
            return;
        }

        Context.Items["ChatProfileId"] = chatProfile.Id;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{chatProfile.Id}");

        await bus.PublishAsync(new UpdatePresenceCommand(chatProfile.Id, IsOnline: true));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("ChatProfileId", out var chatProfileId) && chatProfileId is Guid id)
        {
            await bus.PublishAsync(new UpdatePresenceCommand(id, IsOnline: false));
        }

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
        Guid conversationId, string content,
        Guid? replyToMessageId = null, List<MentionRequest>? mentions = null)
    {
        var senderChatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result<SendMessageResponse>>(
            new SendMessageCommand(
                conversationId, senderChatProfileId,
                MessageType.Text, content, replyToMessageId,
                mentions ?? new List<MentionRequest>()));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task EditMessage(Guid messageId, string newContent)
    {
        var requestingChatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result>(
            new EditMessageCommand(messageId, requestingChatProfileId, newContent));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task DeleteMessage(Guid messageId)
    {
        var requestingChatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result>(
            new DeleteMessageCommand(messageId, requestingChatProfileId));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task AddReaction(Guid messageId, string emoji)
    {
        var chatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result>(
            new AddReactionCommand(messageId, chatProfileId, emoji));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    public async Task MarkAsRead(Guid conversationId)
    {
        var chatProfileId = RequireChatProfileId();

        await bus.InvokeAsync<Result>(new MarkAsReadCommand(conversationId, chatProfileId));
    }

    // ── Typing indicators ─────────────────────────────────────────────────────

    public async Task StartTyping(Guid conversationId, string displayName)
        => await Clients.OthersInGroup($"conversation:{conversationId}")
            .SendAsync("UserStartedTyping", new { conversationId, chatProfileId = RequireChatProfileId(), displayName });

    public async Task StopTyping(Guid conversationId)
        => await Clients.OthersInGroup($"conversation:{conversationId}")
            .SendAsync("UserStoppedTyping", new { conversationId, chatProfileId = RequireChatProfileId() });

    // ── Appels ────────────────────────────────────────────────────────────────

    public async Task StartCall(Guid conversationId, CallType type)
    {
        var initiatorChatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result<StartCallResponse>>(
            new StartCallCommand(conversationId, initiatorChatProfileId, type));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
        else
            await Clients.Caller.SendAsync("CallStarted", result.Value);
    }

    public async Task JoinCall(Guid callId)
    {
        var chatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result>(new JoinCallCommand(callId, chatProfileId));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
        else
            await Clients.OthersInGroup($"call:{callId}")
                .SendAsync("ParticipantJoined", new { callId, chatProfileId });

        await Groups.AddToGroupAsync(Context.ConnectionId, $"call:{callId}");
    }

    public async Task EndCall(Guid callId)
    {
        var requestingChatProfileId = RequireChatProfileId();

        var result = await bus.InvokeAsync<Result>(new EndCallCommand(callId, requestingChatProfileId));

        if (result.IsFailure)
            await Clients.Caller.SendAsync("Error", result.Error);
    }

    // ── WebRTC Signaling (ICE candidates, SDP offers/answers) ────────────────

    public async Task SendWebRtcOffer(Guid callId, Guid targetChatProfileId, object sdpOffer)
        => await Clients.Group($"user:{targetChatProfileId}")
            .SendAsync("WebRtcOffer", new { callId, sdpOffer, fromChatProfileId = RequireChatProfileId() });

    public async Task SendWebRtcAnswer(Guid callId, Guid targetChatProfileId, object sdpAnswer)
        => await Clients.Group($"user:{targetChatProfileId}")
            .SendAsync("WebRtcAnswer", new { callId, sdpAnswer, fromChatProfileId = RequireChatProfileId() });

    public async Task SendIceCandidate(Guid callId, Guid targetChatProfileId, object candidate)
        => await Clients.Group($"user:{targetChatProfileId}")
            .SendAsync("IceCandidate", new { callId, candidate, fromChatProfileId = RequireChatProfileId() });

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        var raw = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("Missing user identity.");

        return Guid.Parse(raw);
    }

    private Guid RequireChatProfileId()
    {
        if (Context.Items.TryGetValue("ChatProfileId", out var value) && value is Guid id)
            return id;

        throw new HubException("No chat profile associated with this connection.");
    }
}
