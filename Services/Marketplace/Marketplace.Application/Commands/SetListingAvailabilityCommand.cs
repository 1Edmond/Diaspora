using Marketplace.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record SetListingAvailabilityCommand(
    Guid ListingId,
    Guid RequestedBy,
    IReadOnlyCollection<AvailabilitySlotDto> Slots
) : ICommand<Result>;