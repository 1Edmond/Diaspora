using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class RemoveFavoriteCommandHandler(
    IFavoriteRepository _favoriteRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(RemoveFavoriteCommand command)
    {
        var favorite = await _favoriteRepository.GetByUserAndListingAsync(command.UserId, command.ListingId);
        if (favorite is null)
            return Result.Success(); // déjà absent — opération idempotente

        await _favoriteRepository.DeleteAsync(favorite);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}