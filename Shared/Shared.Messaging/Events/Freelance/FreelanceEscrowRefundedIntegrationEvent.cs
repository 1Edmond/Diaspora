using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public sealed record FreelanceEscrowRefundedIntegrationEvent(
    Guid EmployerWalletId,
    Guid TransactionId,
    Guid JobApplicationId,
    decimal Amount,
    string Currency
) : IntegrationEvent;