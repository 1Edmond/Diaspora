using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Seeders;

public class CategorySeeder(MarketplaceDbContext _context, ILogger<CategorySeeder> _logger)
{
    private static readonly (string NameFr, string NameEn, string DescFr, string DescEn, int Order)[] DefaultCategories =
    [
        ("Transport & Logistique", "Transport & Logistics", "Services de transport, déménagement et logistique", "Transportation, relocation and logistics services", 1),
        ("Traduction & Interprétariat", "Translation & Interpreting", "Traduction certifiée de documents et interprétariat", "Certified document translation and interpretation", 2),
        ("Démarches administratives", "Administrative Assistance", "Assistance pour formalités et démarches administratives", "Assistance with administrative procedures and paperwork", 3),
        ("Cours & Formation", "Tutoring & Training", "Soutien scolaire, apprentissage linguistique et formations", "Academic tutoring, language learning and training", 4),
        ("Logement & Installation", "Housing & Relocation", "Recherche de logement et aide à l'installation", "Accommodation search and settlement assistance", 5),
        ("Informatique & Numérique", "IT & Digital Services", "Dépannage informatique, développement et services tech", "IT support, software development and tech services", 6),
        ("Santé & Accompagnement", "Health & Assistance", "Orientation médicale, accompagnement et bien-être", "Medical orientation, healthcare accompaniment and wellness", 7),
        ("Restauration & Événements", "Catering & Events", "Traiteur, préparation culinaire et organisation d'événements", "Catering, culinary preparation and event management", 8),
        ("Commerce & Achats", "Trade & Purchasing", "Achat, vente, approvisionnement et relais colis", "Purchasing, parcel relay and trade services", 9),
        ("Services aux Particuliers", "Personal Services", "Entraide, conciergerie et services du quotidien", "Community mutual aid, concierge and daily assistance", 10)
    ];

    public async Task SeedAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        _logger.LogInformation("[MarketplaceService] Seeding default neutral categories with translations...");

        var translationSet = _context.Set<Shared.Domain.Translations.Translation<Category>>();

        foreach (var (nameFr, nameEn, descFr, descEn, order) in DefaultCategories)
        {
            var category = Category.Create(nameFr, descFr, null, order);
            await _context.Categories.AddAsync(category);

            var frTrans = Shared.Domain.Translations.Translation<Category>.Create(
                category.Id,
                "fr",
                new Dictionary<string, string>
                {
                    [nameof(Category.Name)] = nameFr,
                    [nameof(Category.Description)] = descFr
                });

            var enTrans = Shared.Domain.Translations.Translation<Category>.Create(
                category.Id,
                "en",
                new Dictionary<string, string>
                {
                    [nameof(Category.Name)] = nameEn,
                    [nameof(Category.Description)] = descEn
                });

            await translationSet.AddAsync(frTrans);
            await translationSet.AddAsync(enTrans);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("[MarketplaceService] {Count} neutral categories seeded with translations", DefaultCategories.Length);
    }
}
