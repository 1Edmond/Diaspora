using Marketplace.Application.DTOs;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Queries;

public sealed record GetDisputeByIdQuery(Guid DisputeId) : IQuery<Result<DisputeDto>>;
