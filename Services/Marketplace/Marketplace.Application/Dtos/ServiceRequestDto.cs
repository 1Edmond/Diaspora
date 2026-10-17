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
    Guid? RequesterProfileId,
    string RequesterName,
    Guid ProviderId,
    Guid? ProviderProfileId,
    string ProviderName,
    bool ProviderIsVerified,
    string? ProviderCountryCode,
    string Message,
    ServiceRequestStatus Status,
    decimal? AgreedPrice,
    string? AgreedCurrency,
    PaymentStatus PaymentStatus,
    Guid? WalletTransactionId,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? CompletedAt,
    Guid? ChatThreadId,
    decimal? WeightKg = null,
    string? PackageDescription = null,
    string? PackageContentCategory = null,
    decimal? DeclaredValue = null,
    DateTime? HandoverConfirmedAt = null,
    string? HandoverProofUrl = null,
    DateTime? DeliveryConfirmedAt = null,
    string? DeliveryProofUrl = null,
    bool HasHandoverOtp = false,
    bool HasDeliveryOtp = false,
    string? HandoverOtpCode = null,
    string? DeliveryOtpCode = null
);