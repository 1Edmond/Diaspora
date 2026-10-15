using Marketplace.Application.Commands;
using Marketplace.Application.DTOs;
using Marketplace.Application.Queries;
using Marketplace.Application.Requests;
using Marketplace.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using System;
using System.Threading.Tasks;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/disputes")]
public class DisputeController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "marketplace.dispute",
        action: "list",
        description: "List marketplace disputes",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetDisputes(
        [FromQuery] DisputeStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int? page = null,
        [FromQuery] int pageSize = 20)
    {
        var actualPage = page ?? pageNumber;
        var query = new GetDisputesQuery(status, actualPage, pageSize);
        var result = await _messageBus.InvokeAsync<Result<PagedList<DisputeDto>>>(query);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.dispute",
        action: "read",
        description: "Get dispute details",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDisputeById(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result<DisputeDto>>(new GetDisputeByIdQuery(id));

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.dispute",
        action: "resolve",
        description: "Arbitrate and resolve dispute",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveDisputeRequest request)
    {
        var command = new ResolveDisputeCommand(
            id,
            GetCurrentUserId(),
            request.FinalStatus,
            request.ResolutionNotes,
            request.BuyerRefundPercentage,
            request.SellerPayoutPercentage
        );

        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Dispute resolved successfully" });
    }

    private Guid GetCurrentUserId()
    {
        if (Guid.TryParse(_currentUserService.UserId, out var userId))
            return userId;
        return Guid.Empty;
    }
}
