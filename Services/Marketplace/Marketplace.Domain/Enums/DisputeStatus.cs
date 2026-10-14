namespace Marketplace.Domain.Enums;

public enum DisputeStatus
{
    Opened = 0,
    UnderReview = 1,
    ResolvedBuyerRefunded = 2,
    ResolvedSellerPaid = 3,
    ResolvedSplit = 4,
    Dismissed = 5
}
