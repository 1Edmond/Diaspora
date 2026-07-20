using Marketplace.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Queries;

public record GetServiceListingByIdQuery(Guid Id) : IQuery<Result<ServiceListingDto>>;
