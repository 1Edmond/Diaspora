using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceListingController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<ServiceListingController> _logger) : ControllerBase
{
    // Public — consultation libre sans authentification
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] PaymentMode? paymentMode = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] ListingSortBy sortBy = ListingSortBy.Relevance,
        [FromQuery] double? userLatitude = null,
        [FromQuery] double? userLongitude = null,
        [FromQuery] double? maxDistanceKm = null,
        [FromQuery] bool availableNow = false)
    {
        // Le pageSize est plafonné pour éviter les requêtes abusivement coûteuses
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = new GetServiceListingsQuery(
            page, pageSize, categoryId, search, paymentMode, minPrice, maxPrice,
            ActiveOnly: true, city, country, sortBy, userLatitude, userLongitude, maxDistanceKm, availableNow);

        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceListingSummaryDto>>>(query);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result<ServiceListingDto>>(
            new GetServiceListingByIdQuery(id));

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "read-my",
        description: "View own listings",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyListings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceListingSummaryDto>>>(
            new GetMyListingsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "read-pending",
        description: "View pending listings for moderation",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceListingSummaryDto>>>(
            new GetPendingListingsQuery(page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "create",
        description: "Create a service listing (external profiles only)",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceListingCommand command)
    {
        var cmd = command with { ProviderId = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result<Guid>>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "update",
        description: "Update own listing",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceListingCommand command)
    {
        var cmd = command with { Id = id, RequestedBy = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "delete",
        description: "Delete own listing",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteServiceListingCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "approve",
        description: "Approve a pending listing",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new ApproveServiceListingCommand(id));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Listing approved successfully" });
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "reject",
        description: "Reject a pending listing",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string reason)
    {
        var result = await _messageBus.InvokeAsync<Result>(new RejectServiceListingCommand(id, reason));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Listing rejected" });
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "suspend",
        description: "Suspend an active listing",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new SuspendServiceListingCommand(id));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Listing suspended" });
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "add-image",
        description: "Add an image to own listing",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, [FromBody] string imagePath)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new AddListingImageCommand(id, GetCurrentUserId(), imagePath));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok();
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "remove-image",
        description: "Remove an image from own listing",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("{id:guid}/images")]
    public async Task<IActionResult> RemoveImage(Guid id, [FromBody] string imagePath)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new RemoveListingImageCommand(id, GetCurrentUserId(), imagePath));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok();
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "set-availability",
        description: "Set weekly availability slots for own listing",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut("{id:guid}/availability")]
    public async Task<IActionResult> SetAvailability(Guid id, [FromBody] IReadOnlyCollection<AvailabilitySlotDto> slots)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new SetListingAvailabilityCommand(id, GetCurrentUserId(), slots));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "read-stats",
        description: "View own provider statistics dashboard",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("stats/my")]
    public async Task<IActionResult> GetMyStats()
    {
        var result = await _messageBus.InvokeAsync<Result<ProviderStatsDto>>(
            new GetProviderStatsQuery(GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "read-stats",
        description: "View any provider statistics dashboard (admin)",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("stats/provider/{providerId:guid}")]
    public async Task<IActionResult> GetProviderStats(Guid providerId)
    {
        var result = await _messageBus.InvokeAsync<Result<ProviderStatsDto>>(
            new GetProviderStatsQuery(providerId));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "read-all",
        description: "View all listings with status filter (admin)",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] PaymentMode? paymentMode = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] ListingSortBy sortBy = ListingSortBy.Relevance,
        [FromQuery] double? userLatitude = null,
        [FromQuery] double? userLongitude = null,
        [FromQuery] double? maxDistanceKm = null,
        [FromQuery] bool availableNow = false,
        [FromQuery] ListingStatus? status = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = new GetServiceListingsQuery(
            page, pageSize, categoryId, search, paymentMode, minPrice, maxPrice,
            ActiveOnly: false, city, country, sortBy, userLatitude, userLongitude, maxDistanceKm, availableNow, status);

        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceListingSummaryDto>>>(query);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.listing",
        action: "admin-normalize",
        description: "Normalize country and currency to ISO standards for existing listings",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("admin/normalize-locales")]
    public async Task<IActionResult> NormalizeLocales()
    {
        var result = await _messageBus.InvokeAsync<Result<Marketplace.Application.Normalization.NormalizationReport>>(
            new Marketplace.Application.Commands.NormalizeListingLocalesCommand());

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}