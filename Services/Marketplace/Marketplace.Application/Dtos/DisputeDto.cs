using Marketplace.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Marketplace.Application.DTOs;

public sealed record DisputeDto(
    Guid Id,
    Guid ServiceRequestId,
    Guid InitiatorId,
    string? InitiatorName,
    Guid DefendantId,
    string? DefendantName,
    string Reason,
    string Description,
    DisputeStatus Status,
    IReadOnlyCollection<string> EvidenceUrls,
    Guid? ResolvedBy,
    DateTime? ResolvedAt,
    string? ResolutionNotes,
    decimal? BuyerRefundPercentage,
    decimal? SellerPayoutPercentage,
    decimal ServiceRequestAgreedPrice,
    string ServiceRequestCurrency,
    PaymentStatus ServiceRequestPaymentStatus,
    DateTime CreatedAt
);
