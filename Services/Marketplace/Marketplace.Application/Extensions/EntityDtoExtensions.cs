using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using System;

namespace Marketplace.Application.Extensions;

public static class EntityDtoExtensions
{
    public static ServiceRequestDto MapToDto(
        this ServiceRequest r,
        ProfileDirectoryEntry? requesterProfile = null,
        ProfileDirectoryEntry? providerProfile = null,
        bool includeOtpCodes = false) => new(
        r.Id,
        r.ListingId,
        r.Listing?.Title ?? string.Empty,
        r.RequesterId,
        requesterProfile?.Id,
        requesterProfile?.DisplayName ?? string.Empty,
        r.ProviderId,
        providerProfile?.Id,
        providerProfile?.DisplayName ?? string.Empty,
        providerProfile?.IsVerified ?? false,
        providerProfile?.CountryCode,
        r.Message,
        r.Status,
        r.AgreedPrice,
        r.AgreedCurrency,
        r.PaymentStatus,
        r.WalletTransactionId,
        r.CreatedAt,
        r.AcceptedAt,
        r.CompletedAt,
        r.ChatThreadId,
        r.WeightKg,
        r.PackageDescription,
        r.PackageContentCategory,
        r.DeclaredValue,
        r.HandoverConfirmedAt,
        r.HandoverProofUrl,
        r.DeliveryConfirmedAt,
        r.DeliveryProofUrl,
        !string.IsNullOrWhiteSpace(r.HandoverOtpHash),
        !string.IsNullOrWhiteSpace(r.DeliveryOtpHash),
        includeOtpCodes ? r.HandoverOtpCode : null,
        includeOtpCodes ? r.DeliveryOtpCode : null,
        r.SettlementType,
        r.CashTiming,
        r.PlatformDirectFeeAmount,
        r.IsDirectFeePaid,
        r.IsDirectCashSettled,
        r.DirectCashSettledAt
    );
}
