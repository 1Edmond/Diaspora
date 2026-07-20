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
    private static readonly (string Name, string Description, int Order)[] DefaultCategories =
    [
        ("Transport", "Services de transport et déplacement en Russie", 1),
        ("Traduction", "Traduction de documents, interprétariat", 2),
        ("Aide administrative", "Aide pour les démarches administratives", 3),
        ("Cours particuliers", "Soutien scolaire et cours de langue", 4),
        ("Logement", "Aide pour trouver un logement", 5),
        ("Informatique", "Aide informatique et technologique", 6),
        ("Santé", "Accompagnement médical et conseils santé", 7),
        ("Cuisine", "Préparation de plats togolais et africains", 8),
        ("Commerce", "Vente et achat de produits", 9),
        ("Autre", "Autres services communautaires", 10)
    ];

    public async Task SeedAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        _logger.LogInformation("[MarketplaceService] Seeding default categories...");

        foreach (var (name, description, order) in DefaultCategories)
        {
            var category = Category.Create(name, description, null, order);
            await _context.Categories.AddAsync(category);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("[MarketplaceService] {Count} categories seeded", DefaultCategories.Length);
    }
}
