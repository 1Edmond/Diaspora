using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using System;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api")]
public class FavoriteController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "marketplace.favorite",
        action: "add",
        description: "Add a listing to favorites",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("listings/{listingId:guid}/favorite")]
    public async Task<IActionResult> Add(Guid listingId)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new AddFavoriteCommand(listingId, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.favorite",
        action: "remove",
        description: "Remove a listing from favorites",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("listings/{listingId:guid}/favorite")]
    public async Task<IActionResult> Remove(Guid listingId)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new RemoveFavoriteCommand(listingId, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.favorite",
        action: "read-my",
        description: "View own favorite listings",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("favorites/my")]
    public async Task<IActionResult> GetMyFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceListingSummaryDto>>>(
            new GetMyFavoritesQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}