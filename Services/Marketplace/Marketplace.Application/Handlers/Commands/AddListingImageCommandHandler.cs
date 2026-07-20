using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class AddListingImageCommandHandler(
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(AddListingImageCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null)
            return Result.Failure("Listing not found.");

        if (listing.ProviderId != command.RequestedBy)
            return Result.Failure("You are not the owner of this listing.");

        if (listing.ImageUrls.Count >= 5)
            return Result.Failure("Maximum 5 images per listing.");

        listing.AddImage(command.ImagePath);
        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}