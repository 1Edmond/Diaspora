using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class RejectServiceListingCommandHandler(
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(RejectServiceListingCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.Id);
        if (listing is null)
            return Result.Failure("Listing not found.");

        listing.Reject(command.Reason);
        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}