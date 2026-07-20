using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Wallet;

public sealed record TransferCompletedIntegrationEvent(
    Guid SenderWalletId,
    Guid ReceiverWalletId,
    Guid TransactionId,
    decimal Amount,
    decimal Fee,
    string Currency
) : IntegrationEvent;