using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class SuspendServiceListingCommandHandler(
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(SuspendServiceListingCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.Id);
        if (listing is null)
            return Result.Failure("Listing not found.");

        listing.Suspend();
        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}