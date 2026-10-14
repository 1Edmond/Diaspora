using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Domain.Entities;

public class ServiceListing : AggregateRoot, ISoftDeletable
{
    public Guid ProviderId { get; private set; }        // ExternalProfile Guid
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; }    // WhatsApp, Telegram, etc.

    // Payment
    public PaymentMode PaymentMode { get; private set; }
    public decimal? Price { get; private set; }
    public string? Currency { get; private set; }       // RUB, USD, XOF

    // Status
    public ListingStatus Status { get; private set; }
    public bool IsActive { get; private set; }

    // Localisation (recherche géographique)
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    // Corridor & Trajets
    public string? DepartureCountryCode { get; private set; }
    public string? DepartureCity { get; private set; }
    public string? ArrivalCountryCode { get; private set; }
    public string? ArrivalCity { get; private set; }
    public DateTime? DepartureDate { get; private set; }
    public DateTime? ArrivalDate { get; private set; }
    public DateTime? PickupDeadline { get; private set; }

    // Capacité & Tarification colis
    public decimal? CapacityKg { get; private set; }
    public decimal? RemainingKg { get; private set; }
    public decimal? PricePerKg { get; private set; }

    // Logistique
    public string? TransportMode { get; private set; }
    public string? FlightOrTransportNumber { get; private set; }
    public string? ProhibitedItemsDescription { get; private set; }
    public string? MeetingPoints { get; private set; }

    // Disponibilité (créneaux hebdomadaires récurrents, UTC)
    private readonly List<AvailabilitySlot> _availabilitySlots = new();
    public IReadOnlyCollection<AvailabilitySlot> AvailabilitySlots => _availabilitySlots.AsReadOnly();

    // Images (stockées en chemin local, Storage Service plus tard)
    private readonly List<string> _imageUrls = new();
    public IReadOnlyCollection<string> ImageUrls => _imageUrls.AsReadOnly();

    // Stats
    public double AverageRating { get; private set; }
    public int ReviewCount { get; private set; }
    public int ViewCount { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }


    // Navigation
    public Category Category { get; private set; } = null!;
    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    private readonly List<ServiceRequest> _requests = new();
    public IReadOnlyCollection<ServiceRequest> Requests => _requests.AsReadOnly();

    private ServiceListing() { }

    public static ServiceListing Create(
        Guid providerId,
        Guid categoryId,
        string title,
        string description,
        string? contactInfo,
        PaymentMode paymentMode,
        decimal? price,
        string? currency,
        string? city = null,
        string? country = null,
        double? latitude = null,
        double? longitude = null)
    {
        var listing = new ServiceListing
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
            ContactInfo = contactInfo,
            PaymentMode = paymentMode,
            Price = price,
            Currency = currency,
            City = city,
            Country = country,
            Latitude = latitude,
            Longitude = longitude,
            Status = ListingStatus.Pending,
            IsActive = false,
            AverageRating = 0,
            ReviewCount = 0,
            ViewCount = 0,
            IsDeleted = false
        };

        listing.AddDomainEvent(new ServiceListingCreatedEvent(listing.Id, providerId, title));
        return listing;
    }

    public void Update(
        string title,
        string description,
        string? contactInfo,
        Guid categoryId,
        PaymentMode paymentMode,
        decimal? price,
        string? currency,
        string? city = null,
        string? country = null,
        double? latitude = null,
        double? longitude = null)
    {
        Title = title;
        Description = description;
        ContactInfo = contactInfo;
        CategoryId = categoryId;
        PaymentMode = paymentMode;
        Price = price;
        Currency = currency;
        City = city;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;

        // Un listing déjà approuvé qui est modifié repasse en modération :
        // on évite qu'un provider contourne le contrôle admin en éditant un listing actif.
        if (Status == ListingStatus.Approved)
        {
            Status = ListingStatus.Pending;
            IsActive = false;
        }
    }

    public void SetAvailability(IEnumerable<AvailabilitySlot> slots)
    {
        _availabilitySlots.Clear();
        _availabilitySlots.AddRange(slots);
    }

    /// <summary>
    /// Met à jour les valeurs de pays et devise normalisées sans altérer le statut de publication ni le flag IsActive.
    /// </summary>
    public void NormalizeLocale(string? country, string? currency)
    {
        Country = country;
        Currency = currency;
    }

    public bool IsAvailableAt(DateTime utcMoment)
        => _availabilitySlots.Count == 0 || _availabilitySlots.Any(s => s.Contains(utcMoment));

    public void Approve()
    {
        Status = ListingStatus.Approved;
        IsActive = true;
        AddDomainEvent(new ServiceListingApprovedEvent(Id, ProviderId));
    }

    public void Reject(string reason)
    {
        Status = ListingStatus.Rejected;
        IsActive = false;
        AddDomainEvent(new ServiceListingRejectedEvent(Id, ProviderId, reason));
    }

    public void Suspend()
    {
        Status = ListingStatus.Suspended;
        IsActive = false;
    }

    public void AddImage(string imageUrl) => _imageUrls.Add(imageUrl);
    public void RemoveImage(string imageUrl) => _imageUrls.Remove(imageUrl);

    public void IncrementViewCount() => ViewCount++;

    public void RecalculateRating(double newAverage, int newCount)
    {
        AverageRating = newAverage;
        ReviewCount = newCount;
    }

    public Result SetTravelDetails(
        string depCountry,
        string depCity,
        string arrCountry,
        string arrCity,
        DateTime depDate,
        DateTime arrDate,
        DateTime? pickupDeadline,
        decimal capacityKg,
        decimal pricePerKg,
        string? transportMode = null,
        string? transportNumber = null,
        string? prohibitedItems = null,
        string? meetingPoints = null)
    {
        if (string.IsNullOrWhiteSpace(depCountry))
            return Result.Failure("Le pays de départ est obligatoire.");
        if (string.IsNullOrWhiteSpace(depCity))
            return Result.Failure("La ville de départ est obligatoire.");
        if (string.IsNullOrWhiteSpace(arrCountry))
            return Result.Failure("Le pays d'arrivée est obligatoire.");
        if (string.IsNullOrWhiteSpace(arrCity))
            return Result.Failure("La ville d'arrivée est obligatoire.");
        if (arrDate < depDate)
            return Result.Failure("La date d'arrivée doit être postérieure ou égale à la date de départ.");
        if (pickupDeadline.HasValue && pickupDeadline.Value > depDate)
            return Result.Failure("La date limite de récupération doit être antérieure ou égale à la date de départ.");
        if (capacityKg <= 0)
            return Result.Failure("La capacité (en kg) doit être strictement positive.");
        if (pricePerKg < 0)
            return Result.Failure("Le prix par kg ne peut pas être négatif.");

        DepartureCountryCode = depCountry.Trim().ToUpperInvariant();
        DepartureCity = depCity.Trim();
        ArrivalCountryCode = arrCountry.Trim().ToUpperInvariant();
        ArrivalCity = arrCity.Trim();
        DepartureDate = depDate;
        ArrivalDate = arrDate;
        PickupDeadline = pickupDeadline;
        CapacityKg = capacityKg;
        RemainingKg = capacityKg;
        PricePerKg = pricePerKg;
        TransportMode = transportMode;
        FlightOrTransportNumber = transportNumber;
        ProhibitedItemsDescription = prohibitedItems;
        MeetingPoints = meetingPoints;

        return Result.Success();
    }

    public Result ReserveCapacity(decimal kg)
    {
        if (kg <= 0)
            return Result.Failure("Le poids réservé doit être supérieur à zéro.");

        if (!RemainingKg.HasValue)
            return Result.Failure("Cette annonce n'a pas de capacité définie.");

        if (RemainingKg.Value < kg)
            return Result.Failure($"Capacité restante insuffisante ({RemainingKg.Value} kg disponibles, {kg} kg demandés).");

        RemainingKg -= kg;
        return Result.Success();
    }

    public Result ReleaseCapacity(decimal kg)
    {
        if (kg <= 0)
            return Result.Failure("Le poids à libérer doit être supérieur à zéro.");

        if (!RemainingKg.HasValue)
            return Result.Failure("Cette annonce n'a pas de capacité définie.");

        var maxCap = CapacityKg ?? RemainingKg.Value;
        RemainingKg = Math.Min(maxCap, RemainingKg.Value + kg);
        return Result.Success();
    }

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        IsActive = false;
    }
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}