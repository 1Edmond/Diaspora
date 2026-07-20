using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CreateReviewCommandHandler(
    IReviewRepository _reviewRepository,
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateReviewCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null || !listing.IsActive)
            return Result.Failure<Guid>("Listing not found or inactive.");

        if (listing.ProviderId == command.ReviewerId)
            return Result.Failure<Guid>("You cannot review your own listing.");

        var alreadyReviewed = await _reviewRepository.HasReviewedAsync(command.ListingId, command.ReviewerId);
        if (alreadyReviewed)
            return Result.Failure<Guid>("You have already reviewed this listing.");

        var review = Review.Create(command.ListingId, command.ReviewerId, command.Rating, command.Comment);
        await _reviewRepository.AddAsync(review);

        // Recalculate rating
        var (avg, count) = await _reviewRepository.GetRatingStatsAsync(command.ListingId);
        var newAvg = (avg * count + command.Rating) / (count + 1);
        listing.RecalculateRating(Math.Round(newAvg, 2), count + 1);
        await _listingRepository.UpdateAsync(listing);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success(review.Id);
    }
}