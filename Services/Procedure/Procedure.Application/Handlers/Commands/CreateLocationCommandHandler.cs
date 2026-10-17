using Procedure.Application.Commands;
using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;

namespace Procedure.Application.Handlers.Commands;

public sealed class CreateLocationCommandHandler(
    ILocationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<LocationDto>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var addressResult = Address.Create(
            command.Street,
            command.City,
            command.State,
            command.PostalCode,
            command.Country);

        if (addressResult.IsFailure)
            return Result.Failure<LocationDto>(addressResult.Error);

        var schedule = (command.Schedule ?? [])
            .Select(s =>
                s.IsClosed || !s.OpenTime.HasValue || !s.CloseTime.HasValue
                    ? DaySchedule.Closed(s.Day)
                    : DaySchedule.Open(s.Day, s.OpenTime.Value, s.CloseTime.Value))
            .ToList();

        var openingHours = schedule.Count > 0
            ? new OpeningHours(schedule)
            : OpeningHours.Empty();

        var result = Location.Create(
            command.Name,
            addressResult.Value,
            command.Latitude,
            command.Longitude,
            command.PhoneNumber,
            command.Website,
            openingHours);

        if (result.IsFailure)
            return Result.Failure<LocationDto>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.ToDto());
    }
}
