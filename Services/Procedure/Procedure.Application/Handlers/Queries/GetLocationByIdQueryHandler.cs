using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Shared.Domain.Commons;

namespace Procedure.Application.Handlers.Queries;

public sealed class GetLocationByIdQueryHandler(ILocationRepository repository)
{
    public async Task<Result<LocationDto>> Handle(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var location = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (location is null)
            return Result.Failure<LocationDto>("Location not found.");

        return Result.Success(location.ToDto());
    }
}
