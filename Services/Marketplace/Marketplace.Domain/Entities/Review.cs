using Marketplace.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Entities;

public class Review : AggregateRoot, ISoftDeletable
{
    public Guid ListingId { get; private set; }
    public Guid ReviewerId { get; private set; }       // ExternalProfile Guid
    public int Rating { get; private set; }            // 1-5
    public string Comment { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }       // A eu une transaction avec le prestataire

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public ServiceListing Listing { get; private set; } = null!;

    private Review() { }

    public static Review Create(Guid listingId, Guid reviewerId, int rating, string comment, bool isVerified = false)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            ReviewerId = reviewerId,
            Rating = rating,
            Comment = comment,
            IsVerified = isVerified,
            IsDeleted = false
        };

        review.AddDomainEvent(new ReviewCreatedEvent(review.Id, listingId, reviewerId, rating));
        return review;
    }

    public void Update(int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        Rating = rating;
        Comment = comment;
    }

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
