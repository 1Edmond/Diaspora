using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class CategoryPolicyEligibilityTests
{
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateServiceListingCommandHandler _handler;

    public CategoryPolicyEligibilityTests()
    {
        _handler = new CreateServiceListingCommandHandler(
            _listingRepository,
            _categoryRepository,
            _profileDirectory,
            _unitOfWork);
    }

    private static ProfileDirectoryEntry CreateProviderWithLevel(Guid userId, VerificationLevel level)
    {
        var entry = ProfileDirectoryEntry.Create(Guid.NewGuid(), userId);
        entry.ApplyActivation("John Doe", "TG", "XOF");
        entry.SetVerificationLevel(level);
        return entry;
    }

    [Theory]
    [InlineData(VerificationLevel.L0_Registered)]
    [InlineData(VerificationLevel.L1_IdentityDeclared)]
    public async Task Handle_WhenCategoryRequiresL2AndProviderHasLowerLevel_ShouldReturnFailure(VerificationLevel providerLevel)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var provider = CreateProviderWithLevel(userId, providerLevel);

        var category = Category.Create("Transport de colis", "Description", "icon", 1);
        var policy = CategoryPolicy.Create(category.Id, minPublishLevel: VerificationLevel.L2_IdentityVerified);
        category.SetPolicy(policy);

        _profileDirectory.GetByUserIdAsync(userId).Returns(provider);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);
        _listingRepository.CountCreatedSinceAsync(userId, Arg.Any<DateTime>()).Returns(0);

        var command = new CreateServiceListingCommand(
            userId, categoryId, "Trajet Togo Russie", "Description", "+22890000000",
            PaymentMode.Both, 1000m, "XOF", "Lomé", "TG");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("niveau de vérification");
        result.Error.Should().Contain("L2_IdentityVerified");
        await _listingRepository.DidNotReceive().AddAsync(Arg.Any<ServiceListing>());
    }

    [Theory]
    [InlineData(VerificationLevel.L2_IdentityVerified)]
    [InlineData(VerificationLevel.L3_EnhancedVerified)]
    public async Task Handle_WhenCategoryRequiresL2AndProviderHasL2OrHigher_ShouldSucceed(VerificationLevel providerLevel)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var provider = CreateProviderWithLevel(userId, providerLevel);

        var category = Category.Create("Transport de colis", "Description", "icon", 1);
        var policy = CategoryPolicy.Create(category.Id, minPublishLevel: VerificationLevel.L2_IdentityVerified);
        category.SetPolicy(policy);

        _profileDirectory.GetByUserIdAsync(userId).Returns(provider);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);
        _listingRepository.CountCreatedSinceAsync(userId, Arg.Any<DateTime>()).Returns(0);

        var command = new CreateServiceListingCommand(
            userId, categoryId, "Trajet Togo Russie", "Description", "+22890000000",
            PaymentMode.Both, 1000m, "XOF", "Lomé", "TG");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _listingRepository.Received(1).AddAsync(Arg.Any<ServiceListing>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCategoryHasNoPolicy_ShouldSucceedForActivatedProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var provider = CreateProviderWithLevel(userId, VerificationLevel.L0_Registered);

        var category = Category.Create("Service standard", "Description", "icon", 1);
        // Pas de policy attachée (null)

        _profileDirectory.GetByUserIdAsync(userId).Returns(provider);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);
        _listingRepository.CountCreatedSinceAsync(userId, Arg.Any<DateTime>()).Returns(0);

        var command = new CreateServiceListingCommand(
            userId, categoryId, "Service basique", "Description", "+22890000000",
            PaymentMode.Both, 1000m, "XOF", "Lomé", "TG");

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _listingRepository.Received(1).AddAsync(Arg.Any<ServiceListing>());
    }
}
