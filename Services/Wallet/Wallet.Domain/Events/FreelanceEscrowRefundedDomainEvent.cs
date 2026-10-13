using Shared.Domain.Interfaces;
using System;

namespace Wallet.Domain.Events;

public sealed record FreelanceEscrowRefundedDomainEvent(
    Guid EmployerWalletId,
    Guid TransactionId,
    Guid JobApplicationId,
    decimal Amount,
    string Currency
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}