using Marketplace.Application.Abstractions;
using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

using Wolverine;

namespace Marketplace.Application.Handlers.Commands;

public class CreateReviewCommandHandler(
    IReviewRepository _reviewRepository,
    IServiceListingRepository _listingRepository,
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus? _messageBus = null)
{
    public async Task<Result<Guid>> Handle(CreateReviewCommand command)
    {
        var validation = ReviewRules.Validate(command.Rating, command.Comment);
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        var recentCount = await _reviewRepository.CountCreatedSinceAsync(
            command.ReviewerId, DateTime.UtcNow - RateLimitConstants.ReviewCreationWindow);
        if (recentCount >= RateLimitConstants.MaxReviewsPerWindow)
            return Result.Failure<Guid>(
                $"Trop d'avis postés récemment. Limite : {RateLimitConstants.MaxReviewsPerWindow} avis / {RateLimitConstants.ReviewCreationWindow.TotalHours}h.");

        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null || !listing.IsActive)
            return Result.Failure<Guid>("Listing not found or inactive.");

        if (listing.ProviderId == command.ReviewerId)
            return Result.Failure<Guid>("You cannot review your own listing.");

        var alreadyReviewed = await _reviewRepository.HasReviewedAsync(command.ListingId, command.ReviewerId);
        if (alreadyReviewed)
            return Result.Failure<Guid>("You have already reviewed this listing.");

        // Le statut « vérifié » est dérivé exclusivement de l'état serveur : l'auteur doit avoir
        // une demande de service COMPLÉTÉE sur ce listing. Il n'est jamais fourni par le client.
        var isVerified = await _requestRepository.HasCompletedRequestAsync(
            command.ReviewerId, command.ListingId);

        var review = Review.Create(
            command.ListingId,
            command.ReviewerId,
            command.Rating,
            ReviewRules.NormalizeComment(command.Comment),
            isVerified);

        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Les statistiques sont recalculées depuis l'état PERSISTÉ (et non incrémentalement) :
        // l'opération est idempotente et se « auto-répare » si un appel concurrent a modifié
        // les avis entre-temps.
        var (average, count) = await _reviewRepository.GetRatingStatsAsync(command.ListingId);
        listing.RecalculateRating(Math.Round(average, 2), count);
        await _listingRepository.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        if (_messageBus is not null)
        {
            await _messageBus.PublishAsync(new Shared.Messaging.Events.Marketplace.ReviewCreatedIntegrationEvent(
                review.Id,
                review.ListingId,
                listing.ProviderId,
                review.ReviewerId,
                review.Rating,
                review.Comment));
        }

        return Result.Success(review.Id);
    }
}
