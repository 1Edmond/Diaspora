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

        await _reviewRepository.DeleteAsync(review);

        var (avg, count) = await _reviewRepository.GetRatingStatsAsync(review.ListingId);
        var listing = await _listingRepository.GetByIdAsync(review.ListingId);
        listing?.RecalculateRating(avg, count);
        if (listing is not null) await _listingRepository.UpdateAsync(listing);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}