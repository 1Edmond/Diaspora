using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class DeleteServiceListingCommandHandler(
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(DeleteServiceListingCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.Id);
        if (listing is null)
            return Result.Failure("Listing not found.");

        if (listing.ProviderId != command.RequestedBy)
            return Result.Failure("You are not the owner of this listing.");

        await _listingRepository.DeleteAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}