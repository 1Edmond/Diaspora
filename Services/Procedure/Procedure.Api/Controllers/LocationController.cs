using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procedure.Application.Commands;
using Procedure.Application.DTOs;
using Procedure.Application.Queries;
using Procedure.Application.Requests;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Security.Attributes;
using Wolverine;

namespace Procedure.Api.Controllers;

[ApiController]
[Route("api/locations")]
[Route("api/[controller]")]
[Authorize]
public class LocationController(
    IMessageBus _messageBus,
    IUnitOfWork _unitOfWork,
    ILogger<LocationController> _logger) : ControllerBase
{
    [Permission(resourceKey: "procedure.location", action: "read", description: "Consulter les lieux et représentations consulaires", recommendedRoles: new[] { "Admin", "User", "Moderator" })]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? countryCode = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _messageBus.InvokeAsync<Result<IReadOnlyList<LocationDto>>>(
            new GetAllLocationsQuery(countryCode, city, search),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(resourceKey: "procedure.location", action: "read", description: "Consulter les détails d'un lieu", recommendedRoles: new[] { "Admin", "User", "Moderator" })]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<LocationDto>>(
            new GetLocationByIdQuery(id),
            cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(resourceKey: "procedure.location", action: "manage", description: "Créer un lieu ou représentation consulaire", recommendedRoles: new[] { "Admin" })]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var street = !string.IsNullOrWhiteSpace(request.Street)
            ? request.Street
            : (!string.IsNullOrWhiteSpace(request.Address) ? request.Address : "-");

        var country = !string.IsNullOrWhiteSpace(request.Country)
            ? request.Country
            : (!string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode : "-");

        var command = new CreateLocationCommand(
            request.Name,
            street,
            request.City,
            request.State ?? string.Empty,
            request.PostalCode ?? string.Empty,
            country,
            request.Latitude ?? 0,
            request.Longitude ?? 0,
            request.PhoneNumber,
            request.Website ?? request.Email,
            request.Schedule);

        var result = await _messageBus.InvokeAsync<Result<LocationDto>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(result.Value);
    }

    [Permission(resourceKey: "procedure.location", action: "manage", description: "Modifier un lieu ou représentation consulaire", recommendedRoles: new[] { "Admin" })]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var street = !string.IsNullOrWhiteSpace(request.Street)
            ? request.Street
            : (!string.IsNullOrWhiteSpace(request.Address) ? request.Address : "-");

        var country = !string.IsNullOrWhiteSpace(request.Country)
            ? request.Country
            : (!string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode : "-");

        var command = new UpdateLocationCommand(
            id,
            request.Name,
            street,
            request.City,
            request.State ?? string.Empty,
            request.PostalCode ?? string.Empty,
            country,
            request.Latitude ?? 0,
            request.Longitude ?? 0,
            request.PhoneNumber,
            request.Website ?? request.Email,
            request.Schedule);

        var result = await _messageBus.InvokeAsync<Result<LocationDto>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(result.Value);
    }

    [Permission(resourceKey: "procedure.location", action: "manage", description: "Supprimer un lieu ou représentation consulaire", recommendedRoles: new[] { "Admin" })]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteLocationCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Location deleted successfully" });
    }
}

public sealed record CreateLocationRequest(
    string Name,
    string? Street = null,
    string City = "",
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    double? Latitude = 0,
    double? Longitude = 0,
    string? PhoneNumber = null,
    string? Website = null,
    List<DayScheduleRequest>? Schedule = null,
    string? Address = null,
    string? CountryCode = null,
    string? Email = null,
    string? OpeningHours = null
);

public sealed record UpdateLocationRequest(
    string Name,
    string? Street = null,
    string City = "",
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    double? Latitude = 0,
    double? Longitude = 0,
    string? PhoneNumber = null,
    string? Website = null,
    List<DayScheduleRequest>? Schedule = null,
    string? Address = null,
    string? CountryCode = null,
    string? Email = null,
    string? OpeningHours = null
);
