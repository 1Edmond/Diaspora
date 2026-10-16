using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Seeders;

public class CategorySeeder(MarketplaceDbContext _context, ILogger<CategorySeeder> _logger)
{
    private record CategorySeedDefinition(
        Guid? FixedId,
        string NameFr,
        string NameEn,
        string NameRu,
        string DescFr,
        string DescEn,
        string DescRu,
        int Order);

    private static readonly CategorySeedDefinition[] DefaultCategories =
    [
        new(ServiceDataConstants.ParcelTransportCategoryId, "Transport de colis", "Parcel Transport", "Перевозка посылок",
            "Transport de colis de ville à ville et entre pays par des voyageurs de confiance",
            "Intercity and international parcel delivery by trusted travelers",
            "Доставка посылок между городами и странами проверенными попутчиками", 1),
        new(null, "Logement & Hébergement", "Housing & Accommodation", "Жилье и проживание",
            "Recherche de logement, colocation et hébergement temporaire",
            "Housing search, flatsharing and temporary accommodation",
            "Поиск жилья, аренда и временное размещение", 2),
        new(null, "Traduction & Interprétariat", "Translation & Interpreting", "Перевод и сопровождение",
            "Traduction assermentée de documents et interprétariat pour rendez-vous",
            "Certified document translation and in-person interpretation",
            "Заверенный перевод документов и сопровождение на встречах", 3),
        new(null, "Démarches & Aide administrative", "Administrative Assistance", "Административная помощь",
            "Aide aux démarches consulaires, préfectorales et formulaires",
            "Assistance with consular and administrative procedures",
            "Помощь в консульских и административных процедурах", 4),
        new(null, "Courses & Achats", "Groceries & Shopping", "Покупки и доставка",
            "Achat et livraison de produits locaux ou d'épicerie",
            "Purchasing and delivery of local goods and groceries",
            "Покупка и доставка местных товаров и продуктов", 5),
        new(null, "Soutien scolaire & Cours", "Tutoring & Lessons", "Репетиторство и курсы",
            "Cours particuliers, soutien scolaire et perfectionnement linguistique",
            "Private tutoring, academic support and language lessons",
            "Частные уроки, репетиторство и изучение языков", 6),
        new(null, "Ménage & Entretien", "Cleaning & Maintenance", "Уборка и клининг",
            "Nettoyage, ménage à domicile et entretien d'espaces",
            "Home cleaning, housekeeping and space maintenance",
            "Уборка квартир, клининг и поддержание порядка", 7),
        new(null, "Réparation & Bricolage", "Repair & Handyman", "Ремонт и мастер на час",
            "Petits travaux, réparations courantes et montage de meubles",
            "Minor repairs, maintenance and furniture assembly",
            "Мелкий бытовой ремонт и сборка мебели", 8),
        new(null, "Informatique & Numérique", "IT & Digital Services", "IT и цифровые услуги",
            "Assistance informatique, dépannage PC/Mac et services digitaux",
            "Computer support, tech troubleshooting and digital services",
            "Компьютерная помощь, настройка техники и цифровые услуги", 9),
        new(null, "Santé & Accompagnement", "Health & Assistance", "Здоровье и сопровождение",
            "Orientation médicale, accompagnement aux rendez-vous de santé",
            "Healthcare guidance and accompaniment to medical appointments",
            "Медицинская ориентация и сопровождение к врачам", 10),
        new(null, "Restauration & Événements", "Catering & Events", "Кейтеринг и мероприятия",
            "Cuisine traiteur, spécialités culinaires et préparation d'événements",
            "Catering, local culinary specialties and event organization",
            "Кейтеринг, традиционная кухня и организация мероприятий", 11),
        new(null, "Autre prestation", "Other Services", "Другие услуги",
            "Autres services et prestations diverses de la communauté",
            "Other miscellaneous community services",
            "Прочие услуги и взаимопомощь сообщества", 12)
    ];

    public async Task SeedAsync()
    {
        _logger.LogInformation("[MarketplaceService] Seeding/Syncing categories with trilingual translations (FR/EN/RU)...");

        var translationSet = _context.Set<Shared.Domain.Translations.Translation<Category>>();
        var existingCategories = await _context.Categories.ToListAsync();
        var existingTranslations = await translationSet.ToListAsync();

        foreach (var def in DefaultCategories)
        {
            var category = existingCategories.FirstOrDefault(c =>
                (def.FixedId.HasValue && c.Id == def.FixedId.Value) ||
                string.Equals(c.Name, def.NameFr, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, def.NameEn, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                category = Category.Create(def.NameFr, def.DescFr, null, def.Order, def.FixedId);
                await _context.Categories.AddAsync(category);
                existingCategories.Add(category);
            }

            void EnsureTranslation(string culture, string name, string desc)
            {
                var trans = existingTranslations.FirstOrDefault(t => t.OwnerId == category.Id && t.CultureCode == culture);
                if (trans == null)
                {
                    trans = Shared.Domain.Translations.Translation<Category>.Create(
                        category.Id,
                        culture,
                        new Dictionary<string, string>
                        {
                            [nameof(Category.Name)] = name,
                            [nameof(Category.Description)] = desc
                        });
                    translationSet.Add(trans);
                    existingTranslations.Add(trans);
                }
            }

            EnsureTranslation("fr", def.NameFr, def.DescFr);
            EnsureTranslation("en", def.NameEn, def.DescEn);
            EnsureTranslation("ru", def.NameRu, def.DescRu);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("[MarketplaceService] Categories and trilingual translations successfully synchronized.");
    }
}
