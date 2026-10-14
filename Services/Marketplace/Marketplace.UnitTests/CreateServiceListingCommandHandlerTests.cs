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

public class CreateServiceListingCommandHandlerTests
{
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateServiceListingCommandHandler _handler;

    public CreateServiceListingCommandHandlerTests()
    {
        _handler = new CreateServiceListingCommandHandler(
            _listingRepository,
            _categoryRepository,
            _profileDirectory,
            _unitOfWork);
    }

    private static ProfileDirectoryEntry CreateActivatedProfile(
        Guid userId,
        string countryCode = "TG",
        string currency = "XOF")
    {
        var entry = ProfileDirectoryEntry.Create(Guid.NewGuid(), userId);
        entry.ApplyActivation("Valid Provider", countryCode, currency);
        return entry;
    }

    private static Category CreateActiveCategory(Guid categoryId)
    {
        return Category.Create("Services juridiques", "Description", "law-icon", 1);
    }

    [Fact]
    public async Task Handle_WhenProviderNotFoundInDirectory_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        _profileDirectory.GetByUserIdAsync(userId).Returns((ProfileDirectoryEntry?)null);

        var command = new CreateServiceListingCommand(
            userId, Guid.NewGuid(), "Title", "Description", "contact@test.com",
            PaymentMode.ContactOnly, 100m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("activé (et non suspendu)");
        await _listingRepository.DidNotReceive().AddAsync(Arg.Any<ServiceListing>());
    }

    [Fact]
    public async Task Handle_WhenProviderNotActivated_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var profile = ProfileDirectoryEntry.Create(Guid.NewGuid(), userId); // Not activated
        _profileDirectory.GetByUserIdAsync(userId).Returns(profile);

        var command = new CreateServiceListingCommand(
            userId, Guid.NewGuid(), "Title", "Description", "contact@test.com",
            PaymentMode.ContactOnly, 100m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("activé (et non suspendu)");
    }

    [Fact]
    public async Task Handle_WhenProviderIsSuspended_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var profile = CreateActivatedProfile(userId);
        profile.ApplySuspension(); // Suspended
        _profileDirectory.GetByUserIdAsync(userId).Returns(profile);

        var command = new CreateServiceListingCommand(
            userId, Guid.NewGuid(), "Title", "Description", "contact@test.com",
            PaymentMode.ContactOnly, 100m, "XOF");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("activé (et non suspendu)");
    }

    [Fact]
    public async Task Handle_WhenCountryIsNotIsoAlpha2_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var profile = CreateActivatedProfile(userId);
        _profileDirectory.GetByUserIdAsync(userId).Returns(profile);

        var command = new CreateServiceListingCommand(
            userId, Guid.NewGuid(), "Title", "Description", "contact@test.com",
            PaymentMode.ContactOnly, 100m, "XOF", Country: "Togo");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ISO 3166-1 alpha-2");
    }

    [Fact]
    public async Task Handle_WhenCurrencyIsNotIso4217_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var profile = CreateActivatedProfile(userId);
        _profileDirectory.GetByUserIdAsync(userId).Returns(profile);

        var command = new CreateServiceListingCommand(
            userId, Guid.NewGuid(), "Title", "Description", "contact@test.com",
            PaymentMode.ContactOnly, 100m, "FRANCS", Country: "TG");

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ISO 4217");
    }

    [Fact]
    public async Task Handle_WhenProviderEligibleAndDataValid_ShouldCreateListingSuccessfully()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var profile = CreateActivatedProfile(userId, "TG", "XOF");
        var category = CreateActiveCategory(categoryId);

        _profileDirectory.GetByUserIdAsync(userId).Returns(profile);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);
        _listingRepository.CountCreatedSinceAsync(userId, Arg.Any<DateTime>()).Returns(0);

        var command = new CreateServiceListingCommand(
            userId, categoryId, "Mon Service", "Description du service", "contact@test.com",
            PaymentMode.ContactOnly, 5000m, "xof", City: "Lomé", Country: "tg");

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _listingRepository.Received(1).AddAsync(Arg.Is<ServiceListing>(l =>
            l.ProviderId == userId &&
            l.CategoryId == categoryId &&
            l.Country == "TG" &&
            l.Currency == "XOF" &&
            l.City == "Lomé"));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCountryAndCurrencyOmitted_ShouldFallbackToProviderProfile()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var profile = CreateActivatedProfile(userId, "BJ", "XOF");
        var category = CreateActiveCategory(categoryId);

        _profileDirectory.GetByUserIdAsync(userId).Returns(profile);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);
        _listingRepository.CountCreatedSinceAsync(userId, Arg.Any<DateTime>()).Returns(0);

        var command = new CreateServiceListingCommand(
            userId, categoryId, "Mon Service", "Description du service", "contact@test.com",
            PaymentMode.ContactOnly, 5000m, Currency: null, Country: null);

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();

        await _listingRepository.Received(1).AddAsync(Arg.Is<ServiceListing>(l =>
            l.Country == "BJ" &&
            l.Currency == "XOF"));
    }
}
