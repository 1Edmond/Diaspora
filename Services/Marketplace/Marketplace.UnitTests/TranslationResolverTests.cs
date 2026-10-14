using FluentAssertions;
using Marketplace.Domain.Entities;
using Shared.Application.Translations;
using Shared.Domain.Translations;
using Xunit;

namespace Marketplace.UnitTests;

public class TranslationResolverTests
{
    [Fact]
    public void Resolve_WhenExactPreferredLanguageFound_ShouldReturnPreferredTranslation()
    {
        var category = Category.Create("Services administratifs", "Description par defaut", "admin", 1);
        var translations = new List<Translation<Category>>
        {
            Translation<Category>.Create(category.Id, "en", new Dictionary<string, string>
            {
                ["Name"] = "Administrative Services",
                ["Description"] = "English description"
            }),
            Translation<Category>.Create(category.Id, "es", new Dictionary<string, string>
            {
                ["Name"] = "Servicios Administrativos",
                ["Description"] = "Spanish description"
            })
        };

        var resolved = TranslationResolver.Resolve(category.Name, "Name", translations, "es");

        resolved.Should().Be("Servicios Administrativos");
    }

    [Fact]
    public void Resolve_WhenPreferredLanguageNotFound_ShouldFallbackToEnglish()
    {
        var category = Category.Create("Services administratifs", "Description par defaut", "admin", 1);
        var translations = new List<Translation<Category>>
        {
            Translation<Category>.Create(category.Id, "en", new Dictionary<string, string>
            {
                ["Name"] = "Administrative Services",
                ["Description"] = "English description"
            }),
            Translation<Category>.Create(category.Id, "fr", new Dictionary<string, string>
            {
                ["Name"] = "Services Administratifs FR",
                ["Description"] = "French description"
            })
        };

        var resolved = TranslationResolver.Resolve(category.Name, "Name", translations, "de");

        resolved.Should().Be("Administrative Services");
    }

    [Fact]
    public void Resolve_WhenPreferredAndEnglishNotFound_ShouldFallbackToFrench()
    {
        var category = Category.Create("Services administratifs", "Description par defaut", "admin", 1);
        var translations = new List<Translation<Category>>
        {
            Translation<Category>.Create(category.Id, "fr", new Dictionary<string, string>
            {
                ["Name"] = "Services Administratifs FR",
                ["Description"] = "French description"
            })
        };

        var resolved = TranslationResolver.Resolve(category.Name, "Name", translations, "de");

        resolved.Should().Be("Services Administratifs FR");
    }

    [Fact]
    public void Resolve_WhenNoPreferredNoEnNoFr_ShouldFallbackToFirstAvailable()
    {
        var category = Category.Create("Services administratifs", "Description par defaut", "admin", 1);
        var translations = new List<Translation<Category>>
        {
            Translation<Category>.Create(category.Id, "ru", new Dictionary<string, string>
            {
                ["Name"] = "Административные услуги",
                ["Description"] = "Russian description"
            })
        };

        var resolved = TranslationResolver.Resolve(category.Name, "Name", translations, "de");

        resolved.Should().Be("Административные услуги");
    }

    [Fact]
    public void Resolve_WhenNoTranslationsAvailable_ShouldReturnDefaultValue()
    {
        var category = Category.Create("Services administratifs", "Description par defaut", "admin", 1);
        var translations = new List<Translation<Category>>();

        var resolved = TranslationResolver.Resolve(category.Name, "Name", translations, "de");

        resolved.Should().Be("Services administratifs");
    }
}
