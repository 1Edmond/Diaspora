using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Enums;

namespace Wallet.Domain.Entities;

public sealed class Transaction : AggregateRoot
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionDirection Direction { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Fee { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public TransactionStatus Status { get; private set; }

    private Transaction() { }

    public Transaction(
        Guid walletId,
        TransactionType type,
        TransactionDirection direction,
        decimal amount,
        decimal fee,
        decimal balanceAfter,
        string currency,
        string description,
        Guid? referenceId = null,
        string? referenceType = null,
        TransactionStatus status = TransactionStatus.Completed) : base(Guid.NewGuid())
    {
        WalletId = walletId;
        Type = type;
        Direction = direction;
        Amount = amount;
        Fee = fee;
        BalanceAfter = balanceAfter;
        Currency = currency;
        Description = description;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        Status = status;
    }

    public void MarkCompleted() => Status = TransactionStatus.Completed;

    public void MarkFailed() => Status = TransactionStatus.Failed;
}

