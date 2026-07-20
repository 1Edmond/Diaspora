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
[Route("api/chat/calls")]
[Authorize]
public sealed class CallController(
    IMessageBus _messageBus) : ControllerBase
{
    [Permission("chat", "start-call", "Start an audio or video call",
        recommendedRoles: new[] { "User" })]
    [HttpPost]
    public async Task<IActionResult> Start(
        [FromBody] StartCallRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<StartCallResponse>>(
            new StartCallCommand(
                request.ConversationId, request.InitiatorChatProfileId, request.Type),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "join-call", "Join an active call",
        recommendedRoles: new[] { "User" })]
    [HttpPost("{callId:guid}/join")]
    public async Task<IActionResult> Join(
        Guid callId,
        [FromBody] JoinCallRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new JoinCallCommand(callId, request.ChatProfileId), cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "end-call", "End or leave a call",
        recommendedRoles: new[] { "User" })]
    [HttpPost("{callId:guid}/end")]
    public async Task<IActionResult> End(
        Guid callId,
        [FromBody] EndCallRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new EndCallCommand(callId, request.RequestingChatProfileId, request.FinalStatus),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("chat", "read-calls", "Get call history for a conversation",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("conversation/{conversationId:guid}")]
    public async Task<IActionResult> GetHistory(
        Guid conversationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<CallDto>>>(
            new GetCallHistoryQuery(conversationId, pageNumber, pageSize), cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}