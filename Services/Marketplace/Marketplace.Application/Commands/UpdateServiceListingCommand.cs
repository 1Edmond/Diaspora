using Marketplace.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record UpdateServiceListingCommand(
    Guid Id,
    Guid RequestedBy,
    string Title,
    string Description,
    string? ContactInfo,
    Guid CategoryId,
    PaymentMode PaymentMode,
    decimal? Price,
    string? Currency
) : ICommand<Result>;
