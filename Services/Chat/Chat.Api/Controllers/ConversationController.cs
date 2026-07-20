using Chat.Application.Commands;
using Chat.Application.Dtos;
using Chat.Application.Querys;
using Chat.Application.Requests;
using Chat.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/chat/conversations")]
[Authorize]
public sealed class ConversationController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<ConversationController> _logger) : ControllerBase
{
    [Permission("chat", "read-conversations", "Get conversation list",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("{chatProfileId:guid}")]
    public async Task<IActionResult> GetConversations(
        Guid chatProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<ConversationDto>>>(
            new GetConversationsQuery(chatProfileId, pageNumber, pageSize), cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "create-dm", "Create a direct message",
        recommendedRoles: new[] { "User" })]
    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirect(
        [FromBody] CreateDirectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<CreateConversationResponse>>(
            new CreateDirectMessageCommand(
                request.InitiatorChatProfileId, request.RecipientChatProfileId),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "create-group", "Create a group conversation",
        recommendedRoles: new[] { "User" })]
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateGroupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<CreateConversationResponse>>(
            new CreateGroupCommand(
                request.Title, request.Description, request.AvatarUrl,
                request.CreatedByChatProfileId, request.InitialParticipantIds ?? new()),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "create-community", "Create a community channel",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpPost("community")]
    public async Task<IActionResult> CreateCommunity(
        [FromBody] CreateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<CreateCommunityResponse>>(
            new CreateCommunityCommand(
                request.Title, request.Description,
                request.AvatarUrl, request.CreatedByChatProfileId),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "create-broadcast", "Create a broadcast channel",
        recommendedRoles: new[] { "Admin" })]
    [HttpPost("broadcast")]
    public async Task<IActionResult> CreateBroadcast(
        [FromBody] CreateBroadcastRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<CreateBroadcastResponse>>(
            new CreateBroadcastCommand(
                request.Title, request.Description, request.CreatedByChatProfileId),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "manage-participants", "Add a participant to a conversation",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpPost("{conversationId:guid}/participants")]
    public async Task<IActionResult> AddParticipant(
        Guid conversationId,
        [FromBody] AddParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new AddParticipantCommand(
                conversationId, request.RequestingChatProfileId,
                request.ChatProfileId, request.Role),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "manage-participants", "Remove a participant from a conversation",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpDelete("{conversationId:guid}/participants/{chatProfileId:guid}")]
    public async Task<IActionResult> RemoveParticipant(
        Guid conversationId, Guid chatProfileId,
        [FromQuery] Guid requestingChatProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new RemoveParticipantCommand(conversationId, requestingChatProfileId, chatProfileId),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "manage-participants", "Add a bot to a group",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpPost("{conversationId:guid}/bots")]
    public async Task<IActionResult> AddBot(
        Guid conversationId,
        [FromBody] AddBotRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new AddBotCommand(conversationId, request.RequestingChatProfileId, request.BotChatProfileId),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "read-conversations", "Get unread message count",
        recommendedRoles: new[] { "User" })]
    [HttpGet("{conversationId:guid}/unread/{chatProfileId:guid}")]
    public async Task<IActionResult> GetUnreadCount(
        Guid conversationId, Guid chatProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<UnreadCountDto>>(
            new GetUnreadCountQuery(chatProfileId, conversationId), cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "read-conversations", "Mark conversation as read",
        recommendedRoles: new[] { "User" })]
    [HttpPost("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid conversationId,
        [FromBody] MarkAsReadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new MarkAsReadCommand(conversationId, request.ChatProfileId), cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "manage-mention-groups", "Create a mention group",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpPost("{conversationId:guid}/mention-groups")]
    public async Task<IActionResult> CreateMentionGroup(
        Guid conversationId,
        [FromBody] CreateMentionGroupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<CreateMentionGroupResponse>>(
            new CreateMentionGroupCommand(
                conversationId, request.RequestingChatProfileId,
                request.Name, request.MemberChatProfileIds ?? new()),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}