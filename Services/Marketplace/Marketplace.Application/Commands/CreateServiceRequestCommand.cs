using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record CreateServiceRequestCommand(
    Guid ListingId,
    Guid RequesterId,
    string Message,
    decimal? AgreedPrice,
    string? AgreedCurrency
) : ICommand<Result<Guid>>;