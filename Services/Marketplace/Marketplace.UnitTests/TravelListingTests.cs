using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class TravelListingTests
{
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public void SetTravelDetails_WithValidCorridor_ShouldInitializeRemainingCapacity()
    {
        // Arrange
        var listing = ServiceListing.Create(
            providerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            title: "Voyage Lomé - Moscou",
            description: "Transport de colis disponible",
            contactInfo: "+22890000000",
            paymentMode: PaymentMode.WalletPayment,
            price: 20000m,
            currency: "RUB");

        var depDate = DateTime.UtcNow.AddDays(10);
        var arrDate = DateTime.UtcNow.AddDays(11);
        var pickupDeadline = DateTime.UtcNow.AddDays(9);

        // Act
        var result = listing.SetTravelDetails(
            depCountry: "TG",
            depCity: "Lomé",
            arrCountry: "RU",
            arrCity: "Moscou",
            depDate: depDate,
            arrDate: arrDate,
            pickupDeadline: pickupDeadline,
            capacityKg: 20m,
            pricePerKg: 1000m,
            transportMode: "Flight",
            transportNumber: "ET706",
            prohibitedItems: "Produits périssables, liquides interdits",
            meetingPoints: "Aéroport Gnassingbé Eyadéma ou centre-ville Lomé");

        // Assert
        result.IsSuccess.Should().BeTrue();
        listing.DepartureCountryCode.Should().Be("TG");
        listing.DepartureCity.Should().Be("Lomé");
        listing.ArrivalCountryCode.Should().Be("RU");
        listing.ArrivalCity.Should().Be("Moscou");
        listing.DepartureDate.Should().Be(depDate);
        listing.ArrivalDate.Should().Be(arrDate);
        listing.PickupDeadline.Should().Be(pickupDeadline);
        listing.CapacityKg.Should().Be(20m);
        listing.RemainingKg.Should().Be(20m);
        listing.PricePerKg.Should().Be(1000m);
        listing.TransportMode.Should().Be("Flight");
        listing.FlightOrTransportNumber.Should().Be("ET706");
        listing.ProhibitedItemsDescription.Should().Be("Produits périssables, liquides interdits");
        listing.MeetingPoints.Should().Be("Aéroport Gnassingbé Eyadéma ou centre-ville Lomé");
    }

    [Fact]
    public void SetTravelDetails_WithArrivalBeforeDeparture_ShouldFail()
    {
        var listing = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Vol", "Desc", null, PaymentMode.WalletPayment, null, "RUB");

        var depDate = DateTime.UtcNow.AddDays(10);
        var arrDate = DateTime.UtcNow.AddDays(9); // Invalid: before departure

        var result = listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou", depDate, arrDate, null, 20m, 1000m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("postérieure");
    }

    [Fact]
    public void SetTravelDetails_WithPickupDeadlineAfterDeparture_ShouldFail()
    {
        var listing = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Vol", "Desc", null, PaymentMode.WalletPayment, null, "RUB");

        var depDate = DateTime.UtcNow.AddDays(10);
        var arrDate = DateTime.UtcNow.AddDays(11);
        var pickupDeadline = DateTime.UtcNow.AddDays(12); // Invalid: after departure

        var result = listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou", depDate, arrDate, pickupDeadline, 20m, 1000m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("antérieure");
    }

    [Fact]
    public void ReserveCapacity_WhenSufficient_ShouldDecrementRemainingKg()
    {
        var listing = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Vol", "Desc", null, PaymentMode.WalletPayment, null, "RUB");

        listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null,
            capacityKg: 20m, pricePerKg: 1000m);

        var result1 = listing.ReserveCapacity(5m);
        result1.IsSuccess.Should().BeTrue();
        listing.RemainingKg.Should().Be(15m);

        var result2 = listing.ReserveCapacity(15m);
        result2.IsSuccess.Should().BeTrue();
        listing.RemainingKg.Should().Be(0m);
    }

    [Fact]
    public void ReserveCapacity_WhenExceedingRemaining_ShouldFail()
    {
        var listing = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Vol", "Desc", null, PaymentMode.WalletPayment, null, "RUB");

        listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null,
            capacityKg: 10m, pricePerKg: 1000m);

        listing.ReserveCapacity(8m);

        var result = listing.ReserveCapacity(3m); // Only 2kg left
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("insuffisante");
        listing.RemainingKg.Should().Be(2m);
    }

    [Fact]
    public void ReleaseCapacity_ShouldRestoreRemainingCapacity_UpToMaxCapacity()
    {
        var listing = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Vol", "Desc", null, PaymentMode.WalletPayment, null, "RUB");

        listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null,
            capacityKg: 20m, pricePerKg: 1000m);

        listing.ReserveCapacity(10m);
        listing.RemainingKg.Should().Be(10m);

        var releaseResult = listing.ReleaseCapacity(5m);
        releaseResult.IsSuccess.Should().BeTrue();
        listing.RemainingKg.Should().Be(15m);

        // Releasing more than capacity caps at CapacityKg (20m)
        listing.ReleaseCapacity(20m);
        listing.RemainingKg.Should().Be(20m);
    }

    [Fact]
    public async Task CreateServiceListingCommandHandler_WithTravelDetails_ShouldCreateListingWithCapacity()
    {
        var handler = new CreateServiceListingCommandHandler(
            _listingRepository, _categoryRepository, _profileDirectory, _unitOfWork);

        var providerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var profileEntry = ProfileDirectoryEntry.Create(Guid.NewGuid(), providerId);
        profileEntry.ApplyActivation("Koffi Mensah", "TG", "XOF");
        profileEntry.SetVerificationLevel(VerificationLevel.L2_IdentityVerified);
        _profileDirectory.GetByUserIdAsync(providerId).Returns(profileEntry);

        var category = Category.Create("Transport de colis", "Transport P2P", "icon.png", 1);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        var command = new CreateServiceListingCommand(
            ProviderId: providerId,
            CategoryId: categoryId,
            Title: "Trajet Lomé -> Moscou",
            Description: "Place disponible en soute",
            ContactInfo: "tg@telegram",
            PaymentMode: PaymentMode.WalletPayment,
            Price: 20000m,
            Currency: "XOF",
            DepartureCountryCode: "TG",
            DepartureCity: "Lomé",
            ArrivalCountryCode: "RU",
            ArrivalCity: "Moscou",
            DepartureDate: DateTime.UtcNow.AddDays(10),
            ArrivalDate: DateTime.UtcNow.AddDays(11),
            PickupDeadline: DateTime.UtcNow.AddDays(9),
            CapacityKg: 25m,
            PricePerKg: 1000m,
            TransportMode: "Flight",
            FlightOrTransportNumber: "ET706");

        ServiceListing? savedListing = null;
        await _listingRepository.AddAsync(Arg.Do<ServiceListing>(l => savedListing = l));

        var result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        savedListing.Should().NotBeNull();
        savedListing!.DepartureCountryCode.Should().Be("TG");
        savedListing.ArrivalCountryCode.Should().Be("RU");
        savedListing.CapacityKg.Should().Be(25m);
        savedListing.RemainingKg.Should().Be(25m);
        savedListing.PricePerKg.Should().Be(1000m);
        savedListing.TransportMode.Should().Be("Flight");
    }

    [Fact]
    public async Task CreateServiceListingCommandHandler_WhenCapacityExceedsCategoryPolicy_ShouldFail()
    {
        var handler = new CreateServiceListingCommandHandler(
            _listingRepository, _categoryRepository, _profileDirectory, _unitOfWork);

        var providerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var profileEntry = ProfileDirectoryEntry.Create(Guid.NewGuid(), providerId);
        profileEntry.ApplyActivation("Koffi Mensah", "TG", "XOF");
        profileEntry.SetVerificationLevel(VerificationLevel.L2_IdentityVerified);
        _profileDirectory.GetByUserIdAsync(providerId).Returns(profileEntry);

        var category = Category.Create("Transport de colis", "Transport P2P", "icon.png", 1);
        var policy = CategoryPolicy.Create(category.Id, VerificationLevel.L2_IdentityVerified, maxWeightKg: 30m);
        category.SetPolicy(policy);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        var command = new CreateServiceListingCommand(
            ProviderId: providerId,
            CategoryId: categoryId,
            Title: "Trajet Lomé -> Moscou",
            Description: "Place disponible en soute",
            ContactInfo: "tg@telegram",
            PaymentMode: PaymentMode.WalletPayment,
            Price: 40000m,
            Currency: "XOF",
            DepartureCountryCode: "TG",
            DepartureCity: "Lomé",
            ArrivalCountryCode: "RU",
            ArrivalCity: "Moscou",
            DepartureDate: DateTime.UtcNow.AddDays(10),
            ArrivalDate: DateTime.UtcNow.AddDays(11),
            CapacityKg: 50m, // Exceeds 30kg max
            PricePerKg: 1000m);

        var result = await handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("dépasse le maximum autorisé");
    }
}
