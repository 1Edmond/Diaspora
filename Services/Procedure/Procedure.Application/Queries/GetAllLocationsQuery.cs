using Procedure.Application.DTOs;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;

namespace Procedure.Application.Queries;

public sealed record GetAllLocationsQuery(
    string? CountryCode = null,
    string? City = null,
    string? Search = null
) : IQuery<Result<IReadOnlyList<LocationDto>>>;
