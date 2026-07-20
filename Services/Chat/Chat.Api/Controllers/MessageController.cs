using Chat.Application.Commands;
using Chat.Application.Dtos;
using Chat.Application.Querys;
using Chat.Application.Requests;
using Chat.Application.Responses;
using Chat.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/chat/messages")]
[Authorize]
public sealed class MessageController(
    IMessageBus _messageBus) : ControllerBase
{
    [Permission("chat", "read-messages", "Get messages in a conversation",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("conversation/{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] Guid requestingChatProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<MessageDto>>>(
            new GetMessagesQuery(conversationId, requestingChatProfileId, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "send-message", "Send a text message",
        recommendedRoles: new[] { "User" })]
    [HttpPost("text")]
    public async Task<IActionResult> SendText(
        [FromBody] SendTextRequest request,
        CancellationToken cancellationToken)
    {
        var mentions = request.Mentions?.Select(m => new MentionRequest(
            m.Type, m.ChatProfileId, m.MentionGroupId)).ToList() ?? new();

        var result = await _messageBus.InvokeAsync<Result<SendMessageResponse>>(
            new SendMessageCommand(
                request.ConversationId, request.SenderChatProfileId,
                MessageType.Text, request.Content, request.ReplyToMessageId, mentions),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "send-message", "Send a file, image or voice message",
        recommendedRoles: new[] { "User" })]
    [HttpPost("attachment")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendAttachment(
        [FromForm] SendAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<SendMessageResponse>>(
            new SendMessageCommand(
                request.ConversationId, request.SenderChatProfileId,
                request.Type, null, request.ReplyToMessageId,
                new List<MentionRequest>(), request.File),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "edit-message", "Edit own message",
        recommendedRoles: new[] { "User" })]
    [HttpPut("{messageId:guid}")]
    public async Task<IActionResult> Edit(
        Guid messageId,
        [FromBody] EditMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new EditMessageCommand(messageId, request.RequestingChatProfileId, request.NewContent),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "delete-message", "Delete a message",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> Delete(
        Guid messageId,
        [FromQuery] Guid requestingChatProfileId,
        [FromQuery] bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteMessageCommand(messageId, requestingChatProfileId, isAdmin),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "react", "Add a reaction to a message",
        recommendedRoles: new[] { "User" })]
    [HttpPost("{messageId:guid}/reactions")]
    public async Task<IActionResult> AddReaction(
        Guid messageId,
        [FromBody] ReactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new AddReactionCommand(messageId, request.ChatProfileId, request.Emoji),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "react", "Remove a reaction from a message",
        recommendedRoles: new[] { "User" })]
    [HttpDelete("{messageId:guid}/reactions")]
    public async Task<IActionResult> RemoveReaction(
        Guid messageId,
        [FromQuery] Guid chatProfileId,
        [FromQuery] string emoji,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new RemoveReactionCommand(messageId, chatProfileId, emoji),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}