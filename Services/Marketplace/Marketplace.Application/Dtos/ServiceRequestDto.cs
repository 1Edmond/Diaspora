using Marketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record ServiceRequestDto(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    Guid RequesterId,
    string RequesterName,
    Guid ProviderId,
    string ProviderName,
    string Message,
    ServiceRequestStatus Status,
    decimal? AgreedPrice,
    string? AgreedCurrency,
    PaymentStatus PaymentStatus,
    Guid? WalletTransactionId,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? CompletedAt
);