using Marketplace.Application.Abstractions;
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
        var validation = ReviewRules.Validate(command.Rating, command.Comment);
        if (validation.IsFailure)
            return validation;

        var review = await _reviewRepository.GetByIdAsync(command.Id);
        if (review is null)
            return Result.Failure("Review not found.");

        if (review.ReviewerId != command.ReviewerId)
            return Result.Failure("You are not the author of this review.");

        review.Update(command.Rating, ReviewRules.NormalizeComment(command.Comment));
        await _reviewRepository.UpdateAsync(review);

        // Correction : la modification doit être PERSISTÉE avant de recalculer les statistiques,
        // sinon GetRatingStatsAsync relit l'ancienne note et la moyenne du listing reste périmée.
        await _unitOfWork.SaveChangesAsync();

        var (average, count) = await _reviewRepository.GetRatingStatsAsync(review.ListingId);
        var listing = await _listingRepository.GetByIdAsync(review.ListingId);
        if (listing is not null)
        {
            listing.RecalculateRating(Math.Round(average, 2), count);
            await _listingRepository.UpdateAsync(listing);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Success();
    }
}
