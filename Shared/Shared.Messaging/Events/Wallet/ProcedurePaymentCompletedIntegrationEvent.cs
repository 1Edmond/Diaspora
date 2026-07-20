using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Wallet;

public sealed record ProcedurePaymentCompletedIntegrationEvent(
    Guid WalletId,
    Guid TransactionId,
    Guid ProcedureId,
    decimal Amount,
    string Currency
) : IntegrationEvent;