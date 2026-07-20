using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Domain.Events;

public sealed record MarketplacePaymentCompletedDomainEvent(
    Guid BuyerWalletId,
    Guid SellerWalletId,
    Guid TransactionId,
    Guid ServiceId,
    decimal Amount,
    string Currency
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}
