using Marketplace.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;
public record CreateServiceListingCommand(
    Guid ProviderId,
    Guid CategoryId,
    string Title,
    string Description,
    string? ContactInfo,
    PaymentMode PaymentMode,
    decimal? Price,
    string? Currency,
    string? City = null,
    string? Country = null,
    double? Latitude = null,
    double? Longitude = null
) : ICommand<Result<Guid>>;