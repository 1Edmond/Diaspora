using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
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