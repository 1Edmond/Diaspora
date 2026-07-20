using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Wallet;

public sealed record DepositCompletedIntegrationEvent(
    Guid WalletId,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    decimal NewBalance
) : IntegrationEvent;