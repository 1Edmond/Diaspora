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

public class CreateReviewCommandHandlerTests
{
    private readonly IReviewRepository _reviewRepository = Substitute.For<IReviewRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _handler = new CreateReviewCommandHandler(
            _reviewRepository,
            _listingRepository,
            _requestRepository,
            _unitOfWork);
    }

    private ServiceListing CreateActiveListing(Guid listingId, Guid providerId)
    {
        var listing = ServiceListing.Create(
            providerId,
            Guid.NewGuid(),
            "Traduction certifiée",
            "Description du service",
            "contact@test.com",
            PaymentMode.ContactOnly,
            50m,
            "EUR");

        // Utilise la méthode du domaine pour approuver et rendre actif le listing
        listing.Approve();
        typeof(ServiceListing).GetProperty(nameof(ServiceListing.Id))!.SetValue(listing, listingId);
        return listing;
    }

    [Fact]
    public async Task Handle_WhenCompletedRequestExists_ShouldCreateVerifiedReview()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var listing = CreateActiveListing(listingId, providerId);

        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        _reviewRepository.CountCreatedSinceAsync(reviewerId, Arg.Any<DateTime>()).Returns(0);
        _reviewRepository.HasReviewedAsync(listingId, reviewerId).Returns(false);
        _requestRepository.HasCompletedRequestAsync(reviewerId, listingId).Returns(true);
        _reviewRepository.GetRatingStatsAsync(listingId).Returns((4.5, 1));

        Review? savedReview = null;
        await _reviewRepository.AddAsync(Arg.Do<Review>(r => savedReview = r));

        var command = new CreateReviewCommand(listingId, reviewerId, 5, "Service excellent");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedReview.Should().NotBeNull();
        savedReview!.IsVerified.Should().BeTrue("une demande complétée existe pour ce demandeur");
        listing.AverageRating.Should().Be(4.5);
        listing.ReviewCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenNoCompletedRequest_ShouldCreateUnverifiedReview()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var listing = CreateActiveListing(listingId, providerId);

        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        _reviewRepository.CountCreatedSinceAsync(reviewerId, Arg.Any<DateTime>()).Returns(0);
        _reviewRepository.HasReviewedAsync(listingId, reviewerId).Returns(false);
        _requestRepository.HasCompletedRequestAsync(reviewerId, listingId).Returns(false);
        _reviewRepository.GetRatingStatsAsync(listingId).Returns((3.0, 1));

        Review? savedReview = null;
        await _reviewRepository.AddAsync(Arg.Do<Review>(r => savedReview = r));

        var command = new CreateReviewCommand(listingId, reviewerId, 3, "Service correct");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedReview.Should().NotBeNull();
        savedReview!.IsVerified.Should().BeFalse("aucune demande complétée pour ce demandeur");
    }

    [Fact]
    public async Task Handle_WhenReviewerIsProvider_ShouldFailWithSelfReviewError()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listing = CreateActiveListing(listingId, providerId);

        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        _reviewRepository.CountCreatedSinceAsync(providerId, Arg.Any<DateTime>()).Returns(0);

        var command = new CreateReviewCommand(listingId, providerId, 5, "Mon propre service est génial");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You cannot review your own listing.");
        await _reviewRepository.DidNotReceive().AddAsync(Arg.Any<Review>());
    }

    [Fact]
    public async Task Handle_WhenReviewerAlreadyReviewedIncludingSoftDeleted_ShouldReturnFailureWithoutException()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var listing = CreateActiveListing(listingId, providerId);

        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        _reviewRepository.CountCreatedSinceAsync(reviewerId, Arg.Any<DateTime>()).Returns(0);
        // HasReviewedAsync avec IgnoreQueryFilters retourne true pour un avis déjà existant (même soft-deleted)
        _reviewRepository.HasReviewedAsync(listingId, reviewerId).Returns(true);

        var command = new CreateReviewCommand(listingId, reviewerId, 4, "Nouvel avis après suppression");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You have already reviewed this listing.");
        await _reviewRepository.DidNotReceive().AddAsync(Arg.Any<Review>());
    }

    [Fact]
    public async Task Handle_ShouldRecalculateListingRatingFromPersistedReviews()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var listing = CreateActiveListing(listingId, providerId);

        _listingRepository.GetByIdAsync(listingId).Returns(listing);
        _reviewRepository.CountCreatedSinceAsync(reviewerId, Arg.Any<DateTime>()).Returns(0);
        _reviewRepository.HasReviewedAsync(listingId, reviewerId).Returns(false);
        _requestRepository.HasCompletedRequestAsync(reviewerId, listingId).Returns(true);
        // Stats retournées après persistance : moyenne 4.67, 3 avis
        _reviewRepository.GetRatingStatsAsync(listingId).Returns((4.66666, 3));

        var command = new CreateReviewCommand(listingId, reviewerId, 5, "Top");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        listing.AverageRating.Should().Be(4.67);
        listing.ReviewCount.Should().Be(3);
        await _listingRepository.Received(1).UpdateAsync(listing);
        await _unitOfWork.Received(2).SaveChangesAsync(); // Une fois pour l'avis, une fois pour le listing
    }
}
