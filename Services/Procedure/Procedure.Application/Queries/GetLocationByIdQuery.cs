using Procedure.Application.DTOs;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;

namespace Procedure.Application.Queries;

public sealed record GetLocationByIdQuery(Guid Id) : IQuery<Result<LocationDto>>;
