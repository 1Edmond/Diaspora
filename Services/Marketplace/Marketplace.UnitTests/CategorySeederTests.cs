using FluentAssertions;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Marketplace.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Domain.Translations;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class CategorySeederTests
{
    private static MarketplaceDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MarketplaceDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var integrationEventMapper = Substitute.For<IIntegrationEventMapper>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var logger = Substitute.For<ILogger<MarketplaceDbContext>>();
        var currentUserService = Substitute.For<ICurrentUserService>();

        return new MarketplaceDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService);
    }

    [Fact]
    public async Task SeedAsync_ShouldSeedAll12CategoriesWithTrilingualTranslations()
    {
        // Arrange
        using var context = CreateInMemoryContext(Guid.NewGuid().ToString());
        var logger = Substitute.For<ILogger<CategorySeeder>>();
        var seeder = new CategorySeeder(context, logger);

        // Act
        await seeder.SeedAsync();

        // Assert
        var categories = await context.Categories.ToListAsync();
        categories.Should().HaveCount(12);

        // Verify Parcel transport category exists with fixed GUID
        var parcelCategory = categories.FirstOrDefault(c => c.Id == ServiceDataConstants.ParcelTransportCategoryId);
        parcelCategory.Should().NotBeNull();
        parcelCategory!.Name.Should().Be("Transport de colis");

        // Verify translations
        var translations = await context.Set<Translation<Category>>().ToListAsync();
        translations.Should().HaveCount(36); // 12 categories * 3 languages (fr, en, ru)

        var parcelTranslations = translations.Where(t => t.OwnerId == ServiceDataConstants.ParcelTransportCategoryId).ToList();
        parcelTranslations.Should().HaveCount(3);
        parcelTranslations.Select(t => t.CultureCode).Should().BeEquivalentTo(new[] { "fr", "en", "ru" });

        var ruTrans = parcelTranslations.First(t => t.CultureCode == "ru");
        ruTrans.Fields[nameof(Category.Name)].Should().Be("Перевозка посылок");
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_ShouldBeIdempotent()
    {
        // Arrange
        using var context = CreateInMemoryContext(Guid.NewGuid().ToString());
        var logger = Substitute.For<ILogger<CategorySeeder>>();
        var seeder = new CategorySeeder(context, logger);

        // Act
        await seeder.SeedAsync();
        await seeder.SeedAsync(); // Second run

        // Assert
        var categories = await context.Categories.ToListAsync();
        categories.Should().HaveCount(12);

        var translations = await context.Set<Translation<Category>>().ToListAsync();
        translations.Should().HaveCount(36);
    }
}
