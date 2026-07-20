using Marketplace.Application.Dtos;
using Marketplace.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Extensions;

public static class EntityDtoExtensions
{
    public static ServiceRequestDto MapToDto(this ServiceRequest r) => new(
        r.Id, r.ListingId, r.Listing?.Title ?? string.Empty,
        r.RequesterId, string.Empty,
        r.ProviderId, string.Empty,
        r.Message, r.Status,
        r.AgreedPrice, r.AgreedCurrency,
        r.PaymentStatus, r.WalletTransactionId,
        r.CreatedAt, r.AcceptedAt, r.CompletedAt
    );
}
