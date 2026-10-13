using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class SetListingAvailabilityCommandHandler(
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    private const int MaxSlots = 30;

    public async Task<Result> Handle(SetListingAvailabilityCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null)
            return Result.Failure("Listing not found.");

        if (listing.ProviderId != command.RequestedBy)
            return Result.Failure("You are not the owner of this listing.");

        if (command.Slots.Count > MaxSlots)
            return Result.Failure($"Too many availability slots (max {MaxSlots}).");

        try
        {
            var slots = command.Slots
                .Select(s => new AvailabilitySlot(s.Day, s.StartTime, s.EndTime))
                .ToList();

            listing.SetAvailability(slots);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}