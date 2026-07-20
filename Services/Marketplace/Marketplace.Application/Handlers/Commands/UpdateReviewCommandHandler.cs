using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class UpdateReviewCommandHandler(
    IReviewRepository _reviewRepository,
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(UpdateReviewCommand command)
    {
        var review = await _reviewRepository.GetByIdAsync(command.Id);
        if (review is null)
            return Result.Failure("Review not found.");

        if (review.ReviewerId != command.ReviewerId)
            return Result.Failure("You are not the author of this review.");

        review.Update(command.Rating, command.Comment);
        await _reviewRepository.UpdateAsync(review);

        // Recalculate rating
        var (avg, count) = await _reviewRepository.GetRatingStatsAsync(review.ListingId);
        var listing = await _listingRepository.GetByIdAsync(review.ListingId);
        listing?.RecalculateRating(avg, count);
        if (listing is not null) await _listingRepository.UpdateAsync(listing);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}