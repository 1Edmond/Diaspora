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
        var countryCheck = ListingLocaleRules.ValidateCountry(country);
        if (countryCheck.IsFailure)
            return Result.Failure<Guid>(countryCheck.Error);

        var currency = ListingLocaleRules.NormalizeCurrency(command.Currency) ?? provider.Currency;
        var currencyCheck = ListingLocaleRules.ValidateCurrency(currency);
        if (currencyCheck.IsFailure)
            return Result.Failure<Guid>(currencyCheck.Error);

        // 3. Anti-spam.
        var recentCount = await _listingRepository.CountCreatedSinceAsync(
            command.ProviderId, DateTime.UtcNow - RateLimitConstants.ListingCreationWindow);
        if (recentCount >= RateLimitConstants.MaxListingsPerWindow)
            return Result.Failure<Guid>(
                $"Trop de publications récentes. Limite : {RateLimitConstants.MaxListingsPerWindow} listings / {RateLimitConstants.ListingCreationWindow.TotalHours}h.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsActive)
            return Result.Failure<Guid>("Category not found or inactive.");

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

        await _listingRepository.AddAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(listing.Id);
    }
}
