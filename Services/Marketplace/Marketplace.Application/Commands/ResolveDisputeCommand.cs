using Marketplace.Domain.Enums;
using System;

namespace Marketplace.Application.Commands;

public record ResolveDisputeCommand(
    Guid DisputeId,
    Guid AdminUserId,
    DisputeStatus FinalStatus,
    string ResolutionNotes,
    decimal BuyerRefundPercentage = 0m,
    decimal SellerPayoutPercentage = 0m
);
