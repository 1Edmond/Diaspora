using Marketplace.Domain.Enums;

namespace Marketplace.Application.Requests;

public record ResolveDisputeRequest(
    DisputeStatus FinalStatus,
    string ResolutionNotes,
    decimal BuyerRefundPercentage = 0m,
    decimal SellerPayoutPercentage = 0m
);
