using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Wallet;

public sealed record MarketplacePaymentCompletedIntegrationEvent(
    Guid BuyerWalletId,
    Guid SellerWalletId,
    Guid TransactionId,
    Guid ServiceId,
    decimal Amount,
    string Currency
) : IntegrationEvent;