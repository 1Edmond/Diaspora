using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CreateServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IServiceListingRepository _listingRepository,
    IProfileDirectoryRepository _profileDirectory,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateServiceRequestCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        if (listing is null || !listing.IsActive)
            return Result.Failure<Guid>("Listing not found or inactive.");

        if (listing.ProviderId == command.RequesterId)
            return Result.Failure<Guid>("You cannot request your own service.");

        var requester = await _profileDirectory.GetByUserIdAsync(command.RequesterId);
        if (requester is null || !requester.IsEligibleProvider)
            return Result.Failure<Guid>("Votre profil doit être activé (et non suspendu) pour faire une demande.");

        var provider = await _profileDirectory.GetByUserIdAsync(listing.ProviderId);
        if (provider is null || !provider.IsEligibleProvider)
            return Result.Failure<Guid>("Le profil du prestataire n'est pas actif ou est suspendu.");

        var agreedPrice = command.AgreedPrice;
        var agreedCurrency = command.AgreedCurrency ?? listing.Currency;

        if (command.WeightKg.HasValue)
        {
            if (command.WeightKg.Value <= 0)
                return Result.Failure<Guid>("Le poids doit être strictement supérieur à zéro.");

            if (listing.RemainingKg.HasValue && listing.RemainingKg.Value < command.WeightKg.Value)
                return Result.Failure<Guid>($"Capacité restante insuffisante ({listing.RemainingKg.Value} kg disponibles).");

            if ((!agreedPrice.HasValue || agreedPrice.Value == 0) && listing.PricePerKg.HasValue)
            {
                agreedPrice = command.WeightKg.Value * listing.PricePerKg.Value;
            }
        }

        var request = ServiceRequest.Create(
            command.ListingId,
            command.RequesterId,
            listing.ProviderId,
            command.Message,
            agreedPrice,
            agreedCurrency,
            command.WeightKg,
            command.PackageDescription,
            command.PackageContentCategory,
            command.DeclaredValue
        );

        await _requestRepository.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(request.Id);
    }
}