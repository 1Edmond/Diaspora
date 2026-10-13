using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public sealed record FreelancePaymentCompletedIntegrationEvent(
    Guid EmployerWalletId,
    Guid WorkerWalletId,
    Guid TransactionId,
    Guid JobApplicationId,
    decimal Amount,
    string Currency
) : IntegrationEvent;