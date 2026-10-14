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

public class DeleteReviewCommandHandlerTests
{
    private readonly IReviewRepository _reviewRepository = Substitute.For<IReviewRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly DeleteReviewCommandHandler _handler;

    public DeleteReviewCommandHandlerTests()
    {
        _handler = new DeleteReviewCommandHandler(
            _reviewRepository,
            _listingRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenReviewNotFound_ShouldReturnFailure()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        _reviewRepository.GetByIdAsync(reviewId).Returns((Review?)null);

        var command = new DeleteReviewCommand(reviewId, Guid.NewGuid(), IsAdmin: false);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Review not found.");
        await _reviewRepository.DidNotReceive().DeleteAsync(Arg.Any<Review>());
    }

    [Fact]
    public async Task Handle_WhenNotAuthorAndNotAdmin_ShouldReturnFailure()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var review = Review.Create(Guid.NewGuid(), authorId, 5, "Comment");

        _reviewRepository.GetByIdAsync(reviewId).Returns(review);

        var command = new DeleteReviewCommand(reviewId, strangerId, IsAdmin: false);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not the author of this review.");
        await _reviewRepository.DidNotReceive().DeleteAsync(Arg.Any<Review>());
    }

    [Fact]
    public async Task Handle_WhenAuthorDeletes_ShouldPersistDeleteBeforeRecalculatingAverage()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var review = Review.Create(listingId, authorId, 5, "Super avis");
        typeof(Review).GetProperty(nameof(Review.Id))!.SetValue(review, reviewId);

        var listing = ServiceListing.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ménage",
            "Description",
            null,
            PaymentMode.ContactOnly,
            30m,
            "EUR");
        listing.Approve();
        typeof(ServiceListing).GetProperty(nameof(ServiceListing.Id))!.SetValue(listing, listingId);

        _reviewRepository.GetByIdAsync(reviewId).Returns(review);
        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        // Après suppression persistée, la nouvelle moyenne (ex: 0 avis restants)
        _reviewRepository.GetRatingStatsAsync(listingId).Returns((0.0, 0));

        var command = new DeleteReviewCommand(reviewId, authorId, IsAdmin: false);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        listing.AverageRating.Should().Be(0.0);
        listing.ReviewCount.Should().Be(0);

        // Vérification que la suppression est persistée dans le premier SaveChanges,
        // puis la moyenne est recalculée et le listing est mis à jour dans le second SaveChanges
        Received.InOrder(async () =>
        {
            await _reviewRepository.DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();
            await _reviewRepository.GetRatingStatsAsync(listingId);
            await _listingRepository.UpdateAsync(listing);
            await _unitOfWork.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task Handle_WhenAdminDeletesOtherUserReview_ShouldSucceedAndRecalculate()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var review = Review.Create(listingId, authorId, 1, "Avis diffamatoire");
        typeof(Review).GetProperty(nameof(Review.Id))!.SetValue(review, reviewId);

        var listing = ServiceListing.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ménage",
            "Description",
            null,
            PaymentMode.ContactOnly,
            30m,
            "EUR");
        listing.Approve();
        typeof(ServiceListing).GetProperty(nameof(ServiceListing.Id))!.SetValue(listing, listingId);

        _reviewRepository.GetByIdAsync(reviewId).Returns(review);
        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        _reviewRepository.GetRatingStatsAsync(listingId).Returns((4.0, 2));

        var command = new DeleteReviewCommand(reviewId, adminId, IsAdmin: true);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        listing.AverageRating.Should().Be(4.0);
        listing.ReviewCount.Should().Be(2);
        await _reviewRepository.Received(1).DeleteAsync(review);
    }
}
