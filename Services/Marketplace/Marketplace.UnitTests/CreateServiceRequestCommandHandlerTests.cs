using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class CreateServiceRequestCommandHandlerTests
{
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateServiceRequestCommandHandler _handler;

    public CreateServiceRequestCommandHandlerTests()
    {
        _handler = new CreateServiceRequestCommandHandler(
            _requestRepository,
            _listingRepository,
            _profileDirectory,
            _unitOfWork);
    }

    private static ProfileDirectoryEntry CreateActivatedProfile(Guid userId)
    {
        var entry = ProfileDirectoryEntry.Create(Guid.NewGuid(), userId);
        entry.ApplyActivation("User", "TG", "XOF");
        return entry;
    }

    private static ServiceListing CreateActiveListing(Guid providerId)
    {
        var listing = ServiceListing.Create(
            providerId,
            Guid.NewGuid(),
            "Title",
            "Description",
            "contact@test.com",
            PaymentMode.WalletPayment,
            1000m,
            "XOF");
        listing.Approve(); // sets IsActive = true, Status = Approved
        return listing;
    }

    [Fact]
    public async Task Handle_WhenListingNotFound_ShouldReturnFailure()
    {
        var listingId = Guid.NewGuid();
        _listingRepository.GetByIdAsync(listingId).Returns((ServiceListing?)null);

        var command = new CreateServiceRequestCommand(listingId, Guid.NewGuid(), "Hello", 1000m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Listing not found or inactive");
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task Handle_WhenListingInactive_ShouldReturnFailure()
    {
        var providerId = Guid.NewGuid();
        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", null, PaymentMode.ContactOnly, null, null);
        // Not approved, so IsActive = false

        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);

        var command = new CreateServiceRequestCommand(listing.Id, Guid.NewGuid(), "Hello", null, null);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Listing not found or inactive");
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task Handle_WhenRequesterIsProvider_ShouldReturnFailure()
    {
        var providerId = Guid.NewGuid();
        var listing = CreateActiveListing(providerId);
        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);

        var command = new CreateServiceRequestCommand(listing.Id, providerId, "Self request", 1000m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("You cannot request your own service");
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task Handle_WhenRequesterNotFoundInDirectory_ShouldReturnFailure()
    {
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var listing = CreateActiveListing(providerId);

        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);
        _profileDirectory.GetByUserIdAsync(requesterId).Returns((ProfileDirectoryEntry?)null);

        var command = new CreateServiceRequestCommand(listing.Id, requesterId, "Hello", 1000m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Votre profil doit être activé");
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task Handle_WhenRequesterSuspended_ShouldReturnFailure()
    {
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var listing = CreateActiveListing(providerId);

        var requesterProfile = CreateActivatedProfile(requesterId);
        requesterProfile.ApplySuspension();

        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);
        _profileDirectory.GetByUserIdAsync(requesterId).Returns(requesterProfile);

        var command = new CreateServiceRequestCommand(listing.Id, requesterId, "Hello", 1000m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Votre profil doit être activé");
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task Handle_WhenProviderNotEligible_ShouldReturnFailure()
    {
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var listing = CreateActiveListing(providerId);

        var requesterProfile = CreateActivatedProfile(requesterId);
        var providerProfile = ProfileDirectoryEntry.Create(Guid.NewGuid(), providerId); // not activated

        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);
        _profileDirectory.GetByUserIdAsync(requesterId).Returns(requesterProfile);
        _profileDirectory.GetByUserIdAsync(providerId).Returns(providerProfile);

        var command = new CreateServiceRequestCommand(listing.Id, requesterId, "Hello", 1000m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Le profil du prestataire n'est pas actif");
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task Handle_WhenBothEligible_ShouldCreateRequestAndReturnSuccess()
    {
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var listing = CreateActiveListing(providerId);

        var requesterProfile = CreateActivatedProfile(requesterId);
        var providerProfile = CreateActivatedProfile(providerId);

        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);
        _profileDirectory.GetByUserIdAsync(requesterId).Returns(requesterProfile);
        _profileDirectory.GetByUserIdAsync(providerId).Returns(providerProfile);

        var command = new CreateServiceRequestCommand(listing.Id, requesterId, "Please help", 1500m, "XOF");

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _requestRepository.Received(1).AddAsync(Arg.Is<ServiceRequest>(r =>
            r.ListingId == listing.Id &&
            r.RequesterId == requesterId &&
            r.ProviderId == providerId &&
            r.Message == "Please help" &&
            r.AgreedPrice == 1500m &&
            r.AgreedCurrency == "XOF" &&
            r.Status == ServiceRequestStatus.Pending));

        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
