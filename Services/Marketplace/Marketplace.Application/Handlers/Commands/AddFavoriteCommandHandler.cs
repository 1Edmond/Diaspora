using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class AddFavoriteCommandHandler(
    IFavoriteRepository _favoriteRepository,
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(AddFavoriteCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null)
            return Result.Failure("Listing not found.");

        var existing = await _favoriteRepository.GetByUserAndListingAsync(command.UserId, command.ListingId);
        if (existing is not null)
            return Result.Success(); // déjà en favori — opération idempotente

        var favorite = Favorite.Create(command.ListingId, command.UserId);
        await _favoriteRepository.AddAsync(favorite);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}