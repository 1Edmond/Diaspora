using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class ReplyToReviewCommandHandler(
    IReviewRepository _reviewRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(ReplyToReviewCommand command)
    {
        var review = await _reviewRepository.GetByIdWithListingAsync(command.ReviewId);
        if (review is null || review.IsDeleted)
            return Result.Failure("Review not found.");

        // Seul le prestataire propriétaire du listing concerné peut répondre publiquement
        if (review.Listing.ProviderId != command.ProviderId)
            return Result.Failure("You are not the provider of the listing this review belongs to.");

        try
        {
            review.Reply(command.ReplyText);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _reviewRepository.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}