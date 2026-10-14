using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class DeleteReviewCommandHandler(
    IReviewRepository _reviewRepository,
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(DeleteReviewCommand command)
    {
        var review = await _reviewRepository.GetByIdAsync(command.Id);
        if (review is null)
            return Result.Failure("Review not found.");

        if (!command.IsAdmin && review.ReviewerId != command.RequestedBy)
            return Result.Failure("You are not the author of this review.");

        var listingId = review.ListingId;

        await _reviewRepository.DeleteAsync(review);

        // Correction : la suppression (soft delete appliqué au SaveChanges) doit être PERSISTÉE
        // avant de recalculer les statistiques, sinon l'avis supprimé est encore compté.
        await _unitOfWork.SaveChangesAsync();

        var (average, count) = await _reviewRepository.GetRatingStatsAsync(listingId);
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is not null)
        {
            listing.RecalculateRating(Math.Round(average, 2), count);
            await _listingRepository.UpdateAsync(listing);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Success();
    }
}
