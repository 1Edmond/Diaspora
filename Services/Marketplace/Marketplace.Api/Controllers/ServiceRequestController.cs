using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/service-requests")]
public class ServiceRequestController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "marketplace.request",
        action: "read-sent",
        description: "View sent service requests",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceRequestDto>>>(
            new GetMyServiceRequestsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "read-received",
        description: "View received service requests",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceRequestDto>>>(
            new GetMyReceivedRequestsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "read-detail",
        description: "View a specific service request",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result<ServiceRequestDto>>(
            new GetServiceRequestByIdQuery(id, GetCurrentUserId()));

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "create",
        description: "Send a service request",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestCommand command)
    {
        var cmd = command with { RequesterId = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result<Guid>>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "accept",
        description: "Accept a received service request",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new AcceptServiceRequestCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Request accepted" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "pay",
        description: "Pay for an accepted service request via escrow",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new PayServiceRequestCommand(id, GetCurrentUserId()));

        if (result.IsFailure)
        {
            if (result.Error == "Access denied.")
                return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Accepted(new { message = "Payment request submitted for processing." });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "decline",
        description: "Decline a received service request",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id, [FromBody] string reason)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeclineServiceRequestCommand(id, GetCurrentUserId(), reason));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Request declined" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "complete",
        description: "Mark a service request as completed",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new CompleteServiceRequestCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Request completed" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "cancel",
        description: "Cancel a sent service request",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new CancelServiceRequestCommand(id, GetCurrentUserId(), reason));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Request cancelled" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "link-chat-thread",
        description: "Link a chat thread to a service request",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/chat-thread")]
    public async Task<IActionResult> LinkChatThread(Guid id, [FromBody] LinkChatThreadRequest request)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new LinkServiceRequestChatThreadCommand(id, GetCurrentUserId(), request.ChatThreadId));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "read-all",
        description: "View all service requests with filters (admin)",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ServiceRequestStatus? status = null,
        [FromQuery] PaymentStatus? paymentStatus = null,
        [FromQuery] Guid? providerId = null,
        [FromQuery] Guid? requesterId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? search = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = new GetAllServiceRequestsQuery(page, pageSize, status, paymentStatus, providerId, requesterId, dateFrom, dateTo, search);

        var result = await _messageBus.InvokeAsync<Result<PagedList<ServiceRequestDto>>>(query);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "confirm-handover",
        description: "Confirm parcel handover with OTP and photo proof",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/confirm-handover")]
    public async Task<IActionResult> ConfirmHandover(Guid id, [FromBody] ConfirmHandoverRequest request)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new ConfirmHandoverCommand(id, GetCurrentUserId(), request.Otp, request.ProofPhotoUrl));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Handover confirmed" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "mark-in-transit",
        description: "Mark parcel as in transit",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/mark-in-transit")]
    public async Task<IActionResult> MarkInTransit(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new MarkInTransitCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Status updated to InTransit" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "mark-arrived",
        description: "Mark parcel as arrived at destination",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/mark-arrived")]
    public async Task<IActionResult> MarkArrived(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new MarkArrivedAtDestinationCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Status updated to ArrivedAtDestination" });
    }

    [Permission(
        resourceKey: "marketplace.request",
        action: "confirm-delivery",
        description: "Confirm parcel delivery with OTP and photo proof",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/confirm-delivery")]
    public async Task<IActionResult> ConfirmDelivery(Guid id, [FromBody] ConfirmDeliveryRequest request)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new ConfirmDeliveryCommand(id, GetCurrentUserId(), request.Otp, request.ProofPhotoUrl));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Delivery confirmed" });
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record LinkChatThreadRequest(Guid ChatThreadId);
public record ConfirmHandoverRequest(string Otp, string? ProofPhotoUrl = null);
public record ConfirmDeliveryRequest(string Otp, string? ProofPhotoUrl = null);