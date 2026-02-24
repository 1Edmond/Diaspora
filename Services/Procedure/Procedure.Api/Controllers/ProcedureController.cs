using Microsoft.AspNetCore.Mvc;
using Procedure.Application.Commands;
using Procedure.Application.DTOs;
using Procedure.Application.Queries;
using Procedure.Application.Requests;
using Procedure.Application.Responses;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Security.Attributes;
using Wolverine;

namespace Procedure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcedureController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<ProcedureController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "procedure",
        action: "read",
        description: "Get a procedure by ID",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{procedureId:guid}")]
    public async Task<IActionResult> GetById(
        Guid procedureId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<ProcedureDto>>(
            new GetProcedureByIdQuery(procedureId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure",
        action: "list",
        description: "Get procedures by profile type and profile type ID",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetByProfileType(
        [FromQuery] ProcedureProfileType profileType,
        [FromQuery] Guid profileTypeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<ProcedureDto>>>(
            new GetProceduresByProfileTypeQuery(profileType, profileTypeId, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure",
        action: "create",
        description: "Create a new procedure (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<CreateProcedureResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure",
        action: "update",
        description: "Update an existing procedure (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{procedureId:guid}")]
    public async Task<IActionResult> Update(
        Guid procedureId,
        [FromBody] UpdateProcedureRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProcedureCommand(
            procedureId,
            request.Title,
            request.Description,
            request.CostAmount,
            request.CostCurrency,
            request.EstimatedDurationDays,
            request.LocationId,
            request.DependencyIds,
            request.RequiredDocumentTypeIds);

        var result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Procedure updated successfully" });
    }

    [Permission(
        resourceKey: "procedure",
        action: "delete",
        description: "Deactivate a procedure (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpDelete("{procedureId:guid}")]
    public async Task<IActionResult> Delete(
        Guid procedureId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteProcedureCommand(procedureId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Procedure deactivated successfully" });
    }
}


