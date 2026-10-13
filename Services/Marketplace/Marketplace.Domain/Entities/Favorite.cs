using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Entities;

/// <summary>Favori d'un utilisateur pour un listing (wishlist).</summary>
public class Favorite : AggregateRoot
{
    public Guid ListingId { get; private set; }
    public Guid UserId { get; private set; }

    public ServiceListing Listing { get; private set; } = null!;

    private Favorite() { }

    public static Favorite Create(Guid listingId, Guid userId)
    {
        return new Favorite
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            UserId = userId
        };
    }
}