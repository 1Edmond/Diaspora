using Marketplace.Application.Abstractions;
using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CreateServiceListingCommandHandler(
    IServiceListingRepository _listingRepository,
    ICategoryRepository _categoryRepository,
    IProfileDirectoryRepository _profileDirectory,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateServiceListingCommand command)
    {
        // 1. Le prestataire doit exister dans l'annuaire local, être activé et ne pas être suspendu.
        //    command.ProviderId est l'identifiant utilisateur issu du jeton.
        var provider = await _profileDirectory.GetByUserIdAsync(command.ProviderId);
        if (provider is null || !provider.IsEligibleProvider)
            return Result.Failure<Guid>(
                "Votre profil doit être activé (et non suspendu) pour publier un service.");

        // 2. Pays et devise : normalisés et validés, avec repli sur ceux du profil.
        var country = ListingLocaleRules.NormalizeCountry(command.Country) ?? provider.CountryCode;
        var currency = ListingLocaleRules.NormalizeCurrency(command.Currency) ?? provider.Currency;

        var localeCheck = ListingLocaleRules.ValidateCountryAndCurrency(country, currency);
        if (localeCheck.IsFailure)
            return Result.Failure<Guid>(localeCheck.Error);

        // 3. Anti-spam.
        var recentCount = await _listingRepository.CountCreatedSinceAsync(
            command.ProviderId, DateTime.UtcNow - RateLimitConstants.ListingCreationWindow);
        if (recentCount >= RateLimitConstants.MaxListingsPerWindow)
            return Result.Failure<Guid>(
                $"Trop de publications récentes. Limite : {RateLimitConstants.MaxListingsPerWindow} listings / {RateLimitConstants.ListingCreationWindow.TotalHours}h.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure<Guid>("Category not found or inactive.");

        // 4. Contrôle d'éligibilité zéro-confiance basé sur le niveau de vérification requis par la catégorie.
        if (category.Policy is not null && provider.Level < category.Policy.MinPublishLevel)
        {
            return Result.Failure<Guid>(
                $"Votre niveau de vérification ({provider.Level}) est insuffisant pour publier dans la catégorie '{category.Name}'. Niveau minimum requis : {category.Policy.MinPublishLevel}.");
        }

        // 5. Contrôle du poids maximum autorisé par la politique de catégorie.
        if (category.Policy?.MaxWeightKg.HasValue == true && command.CapacityKg.HasValue && command.CapacityKg.Value > category.Policy.MaxWeightKg.Value)
        {
            return Result.Failure<Guid>(
                $"La capacité proposée ({command.CapacityKg.Value} kg) dépasse le maximum autorisé pour cette catégorie ({category.Policy.MaxWeightKg.Value} kg).");
        }

        var listing = ServiceListing.Create(
            command.ProviderId,
            command.CategoryId,
            command.Title,
            command.Description,
            command.ContactInfo,
            command.PaymentMode,
            command.Price,
            currency,
            command.City,
            country,
            command.Latitude,
            command.Longitude
        );

        if (!string.IsNullOrWhiteSpace(command.DepartureCountryCode)
            && !string.IsNullOrWhiteSpace(command.ArrivalCountryCode)
            && command.DepartureDate.HasValue
            && command.ArrivalDate.HasValue
            && command.CapacityKg.HasValue
            && command.PricePerKg.HasValue)
        {
            var travelResult = listing.SetTravelDetails(
                command.DepartureCountryCode,
                command.DepartureCity ?? string.Empty,
                command.ArrivalCountryCode,
                command.ArrivalCity ?? string.Empty,
                command.DepartureDate.Value,
                command.ArrivalDate.Value,
                command.PickupDeadline,
                command.CapacityKg.Value,
                command.PricePerKg.Value,
                command.TransportMode,
                command.FlightOrTransportNumber,
                command.ProhibitedItemsDescription,
                command.MeetingPoints);

            if (travelResult.IsFailure)
                return Result.Failure<Guid>(travelResult.Error);
        }

        await _listingRepository.AddAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(listing.Id);
    }
}
