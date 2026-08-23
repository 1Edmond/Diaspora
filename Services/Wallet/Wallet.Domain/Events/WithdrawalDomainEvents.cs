using Shared.Domain.Interfaces;

namespace Wallet.Domain.Events;

public sealed record WithdrawalRequestedDomainEvent(
    Guid WalletId, Guid TransactionId, decimal Amount, string Currency) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}

public sealed record WithdrawalCompletedDomainEvent(
    Guid WalletId, Guid TransactionId, decimal Amount, string Currency) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}

public sealed record WithdrawalFailedDomainEvent(
    Guid WalletId, Guid TransactionId, decimal Amount, string Currency) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}
