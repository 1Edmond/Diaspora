using Procedure.Application.Commands;
using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;

namespace Procedure.Application.Handlers.Commands;

public sealed class UpdateLocationCommandHandler(
    ILocationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<LocationDto>> Handle(
        UpdateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var location = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (location is null)
            return Result.Failure<LocationDto>("Location not found.");

        if (string.IsNullOrWhiteSpace(command.Name))
            return Result.Failure<LocationDto>("Location name is required.");

        if (command.Latitude < -90 || command.Latitude > 90)
            return Result.Failure<LocationDto>("Latitude must be between -90 and 90.");

        if (command.Longitude < -180 || command.Longitude > 180)
            return Result.Failure<LocationDto>("Longitude must be between -180 and 180.");

        var addressResult = Address.Create(
            command.Street,
            command.City,
            command.State,
            command.PostalCode,
            command.Country);

        if (addressResult.IsFailure)
            return Result.Failure<LocationDto>(addressResult.Error);

        OpeningHours openingHours;
        if (command.Schedule is { Count: > 0 })
        {
            var schedule = command.Schedule
                .Select(s =>
                    s.IsClosed || !s.OpenTime.HasValue || !s.CloseTime.HasValue
                        ? DaySchedule.Closed(s.Day)
                        : DaySchedule.Open(s.Day, s.OpenTime.Value, s.CloseTime.Value))
                .ToList();
            openingHours = new OpeningHours(schedule);
        }
        else
        {
            openingHours = location.OpeningHours;
        }

        location.Update(
            command.Name,
            addressResult.Value,
            command.Latitude,
            command.Longitude,
            command.PhoneNumber,
            command.Website,
            openingHours);

        await repository.UpdateAsync(location, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(location.ToDto());
    }
}
