using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Shared.Domain.Commons;

namespace Procedure.Application.Handlers.Queries;

public sealed class GetAllLocationsQueryHandler(ILocationRepository repository)
{
    public async Task<Result<IReadOnlyList<LocationDto>>> Handle(
        GetAllLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var locations = await repository.GetAllActiveAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var countryFilter = query.CountryCode.Trim();
            locations = locations.Where(l =>
                l.Address.Country.Equals(countryFilter, StringComparison.OrdinalIgnoreCase)
                || l.Address.Country.Contains(countryFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var cityFilter = query.City.Trim();
            locations = locations.Where(l =>
                l.Address.City.Equals(cityFilter, StringComparison.OrdinalIgnoreCase)
                || l.Address.City.Contains(cityFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchFilter = query.Search.Trim();
            locations = locations.Where(l =>
                l.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                || l.Address.Street.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                || l.Address.City.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                || l.Address.Country.Contains(searchFilter, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = locations.Select(l => l.ToDto()).ToList();
        return Result.Success<IReadOnlyList<LocationDto>>(dtos);
    }
}
