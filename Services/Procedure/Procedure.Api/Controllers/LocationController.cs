using Microsoft.AspNetCore.Mvc;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Shared.Domain.ValueObjects;
using Shared.Security.Attributes;

namespace Procedure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(
    ILocationRepository _repository,
    ILogger<LocationController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "procedure.location",
        action: "list",
        description: "List all active locations",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var locations = await _repository.GetAllActiveAsync(cancellationToken);
        return Ok(locations.Select(l => l.ToDto()));
    }

    [Permission(
        resourceKey: "procedure.location",
        action: "create",
        description: "Create a location (admin)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        // Construire le Value Object Address depuis Shared.Domain
        var addressResult = Address.Create(
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        if (addressResult.IsFailure)
            return BadRequest(new { error = addressResult.Error });

        var schedule = request.Schedule.Select(s =>
            s.IsClosed
                ? DaySchedule.Closed(s.Day)
                : DaySchedule.Open(s.Day, s.OpenTime!.Value, s.CloseTime!.Value))
            .ToList();

        var result = Location.Create(
            request.Name,
            addressResult.Value,
            request.Latitude,
            request.Longitude,
            request.PhoneNumber,
            request.Website,
            new OpeningHours(schedule));

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        await _repository.AddAsync(result.Value, cancellationToken);

        return Ok(result.Value.ToDto());
    }
}

public sealed record CreateLocationRequest(
    string Name,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    string? PhoneNumber,
    string? Website,
    List<DayScheduleRequest> Schedule
);

public sealed record DayScheduleRequest(
    DayOfWeek Day,
    bool IsClosed,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);

