using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/listings/{listingId:guid}/reviews")]
public class ReviewController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    // Public — consultation libre
    [HttpGet]
    public async Task<IActionResult> GetReviews(
        Guid listingId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<ReviewDto>>>(
            new GetListingReviewsQuery(listingId, page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.review",
        action: "create",
        description: "Submit a review for a service",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(Guid listingId, [FromBody] CreateReviewRequest request)
    {
        var command = new CreateReviewCommand(listingId, GetCurrentUserId(), request.Rating, request.Comment, request.IsVerified);
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(
        resourceKey: "marketplace.review",
        action: "update",
        description: "Update own review",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> Update(Guid reviewId, [FromBody] CreateReviewRequest request)
    {
        var command = new UpdateReviewCommand(reviewId, GetCurrentUserId(), request.Rating, request.Comment);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.review",
        action: "delete",
        description: "Delete a review",
        recommendedRoles: new[] { "User", "Admin", "Moderator" }
    )]
    [HttpDelete("{reviewId:guid}")]
    public async Task<IActionResult> Delete(Guid reviewId)
    {
        // Le statut admin/modérateur est déterminé côté serveur à partir des rôles
        // du token authentifié — il ne doit jamais être accepté depuis la requête du client.
        var isAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Moderator");

        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteReviewCommand(reviewId, GetCurrentUserId(), isAdmin));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.review",
        action: "reply",
        description: "Reply publicly to a review as the listing's provider",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{reviewId:guid}/reply")]
    public async Task<IActionResult> Reply(Guid reviewId, [FromBody] ReplyToReviewRequest request)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new ReplyToReviewCommand(reviewId, GetCurrentUserId(), request.ReplyText));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record CreateReviewRequest(int Rating, string Comment, bool IsVerified = false);
public record ReplyToReviewRequest(string ReplyText);