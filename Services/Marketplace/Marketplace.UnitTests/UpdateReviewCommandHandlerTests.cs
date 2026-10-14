using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Domain.Interfaces;
using Xunit;

namespace Marketplace.UnitTests;

public class UpdateReviewCommandHandlerTests
{
    private readonly IReviewRepository _reviewRepository = Substitute.For<IReviewRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly UpdateReviewCommandHandler _handler;

    public UpdateReviewCommandHandlerTests()
    {
        _handler = new UpdateReviewCommandHandler(
            _reviewRepository,
            _listingRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithInvalidRating_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 6, "Note invalide");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Rating must be between 1 and 5");
        await _reviewRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotAuthor_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var review = Review.Create(Guid.NewGuid(), authorId, 4, "Avis initial");

        _reviewRepository.GetByIdAsync(reviewId).Returns(review);

        var command = new UpdateReviewCommand(reviewId, callerId, 5, "Tentative de piratage");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not the author of this review.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistUpdateAndRecalculateAverage()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var review = Review.Create(listingId, authorId, 4, "Avis initial");
        typeof(Review).GetProperty(nameof(Review.Id))!.SetValue(review, reviewId);

        var listing = ServiceListing.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Plomberie",
            "Description",
            null,
            PaymentMode.ContactOnly,
            100m,
            "EUR");
        listing.Approve();
        typeof(ServiceListing).GetProperty(nameof(ServiceListing.Id))!.SetValue(listing, listingId);

        _reviewRepository.GetByIdAsync(reviewId).Returns(review);
        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        // Après persistance, la nouvelle moyenne calculée par la base de données
        _reviewRepository.GetRatingStatsAsync(listingId).Returns((4.8, 5));

        var command = new UpdateReviewCommand(reviewId, authorId, 5, "Mise à jour : excellent !");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Mise à jour : excellent !");
        listing.AverageRating.Should().Be(4.8);
        listing.ReviewCount.Should().Be(5);

        // Vérification de l'ordre : la première sauvegarde persiste l'avis, puis recalcul, puis mise à jour du listing
        Received.InOrder(async () =>
        {
            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            await _reviewRepository.GetRatingStatsAsync(listingId);
            await _listingRepository.UpdateAsync(listing);
            await _unitOfWork.SaveChangesAsync();
        });
    }
}
