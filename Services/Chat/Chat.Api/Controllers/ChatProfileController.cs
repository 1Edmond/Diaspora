using Chat.Application.Commands;
using Chat.Application.Dtos;
using Chat.Application.Querys;
using Chat.Application.Requests;
using Chat.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/chat/profiles")]
[Authorize]
public sealed class ChatProfileController(
    IMessageBus _messageBus) : ControllerBase
{
    [Permission("chat", "read-profile", "Get a chat profile",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("{chatProfileId:guid}")]
    public async Task<IActionResult> GetById(
        Guid chatProfileId, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<ChatProfileDto>>(
            new GetChatProfileQuery(chatProfileId), cancellationToken);

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "read-profile-by-external", "Resolve a chat profile from an ExternalProfileId",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("by-external/{externalProfileId:guid}")]
    public async Task<IActionResult> GetByExternalProfileId(
        Guid externalProfileId, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<ChatProfileDto>>(
            new GetChatProfileByExternalProfileIdQuery(externalProfileId), cancellationToken);

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("chat", "create-profile", "Create a chat profile",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateChatProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateChatProfileCommand(
            request.UserId, request.ProfileId, request.ProfileTypeId,
            request.DisplayName, request.AvatarUrl,
            request.IsBot, request.BotToken);

        var result = await _messageBus.InvokeAsync<Result<CreateChatProfileResponse>>(
            command, cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
