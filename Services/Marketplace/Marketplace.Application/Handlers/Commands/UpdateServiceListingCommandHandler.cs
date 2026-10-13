using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class UpdateServiceListingCommandHandler(
    IServiceListingRepository _listingRepository,
    ICategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(UpdateServiceListingCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.Id);
        if (listing is null)
            return Result.Failure("Listing not found.");

        if (listing.ProviderId != command.RequestedBy)
            return Result.Failure("You are not the owner of this listing.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure("Category not found or inactive.");

        listing.Update(
            command.Title,
            command.Description,
            command.ContactInfo,
            command.CategoryId,
            command.PaymentMode,
            command.Price,
            command.Currency,
            command.City,
            command.Country,
            command.Latitude,
            command.Longitude
        );

        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
