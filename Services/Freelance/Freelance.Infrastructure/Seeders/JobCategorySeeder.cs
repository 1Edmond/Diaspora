using Freelance.Domain.Entities;
using Freelance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Freelance.Infrastructure.Seeders;

public class JobCategorySeeder(
    FreelanceDbContext context,
    ILogger<JobCategorySeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            if (await context.JobCategories.AnyAsync())
            {
                logger.LogInformation("JobCategories already seeded");
                return;
            }

            logger.LogInformation("Seeding JobCategories...");

            var categories = new[]
            {
                JobCategory.Create("Nettoyage", "Services de nettoyage résidentiel et commercial", true, 1),
                JobCategory.Create("Jardinage", "Entretien d'espaces verts, tonte, taille", true, 2),
                JobCategory.Create("Bricolage", "Petits travaux de réparation et montage", true, 3),
                JobCategory.Create("Livraison", "Transport et livraison de colis", true, 4),
                JobCategory.Create("Garde d'enfants", "Baby-sitting, garde d'enfants", true, 5),
                JobCategory.Create("Soutien scolaire", "Aide aux devoirs, cours particuliers", true, 6),
                JobCategory.Create("Soins aux personnes âgées", "Aide à domicile, compagnie", true, 7),
                JobCategory.Create("Événementiel", "Staff pour événements, accueil", true, 8),
                JobCategory.Create("Informatique", "Support IT, développement, dépannage", true, 9),
                JobCategory.Create("Autre", "Autres services freelance", true, 10),
            };

            await context.JobCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            logger.LogInformation("JobCategories seeded successfully: {Count} categories", categories.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding JobCategories");
            throw;
        }
    }
}