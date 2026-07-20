using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class RemoveListingImageCommandHandler(
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(RemoveListingImageCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null)
            return Result.Failure("Listing not found.");

        if (listing.ProviderId != command.RequestedBy)
            return Result.Failure("You are not the owner of this listing.");

        listing.RemoveImage(command.ImagePath);
        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}