using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CreateServiceListingCommandHandler(
    IServiceListingRepository _listingRepository,
    ICategoryRepository _categoryRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateServiceListingCommand command)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure<Guid>("Category not found or inactive.");

        var listing = ServiceListing.Create(
            command.ProviderId,
            command.CategoryId,
            command.Title,
            command.Description,
            command.ContactInfo,
            command.PaymentMode,
            command.Price,
            command.Currency
        );

        await _listingRepository.AddAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(listing.Id);
    }
}
