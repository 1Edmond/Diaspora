using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Enums;
using Wallet.Domain.Events;

namespace Wallet.Domain.Entities;

public sealed class Wallet : AggregateRoot
{
    public Guid ExternalProfileId { get; private set; }
    public Guid UserId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public decimal HeldBalance { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    private Wallet(Guid id, Guid externalProfileId, Guid userId, string currency)
        : base(id)
    {
        ExternalProfileId = externalProfileId;
        UserId = userId;
        Currency = currency.ToUpperInvariant();
        Balance = 0m;
        HeldBalance = 0m;
        IsActive = true;
    }

    public static Result<Wallet> Create(
        Guid externalProfileId,
        Guid userId,
        string currency)
    {
        if (externalProfileId == Guid.Empty)
            return Result.Failure<Wallet>("ExternalProfileId is required.");
        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Wallet>("Currency is required.");

        var wallet = new Wallet(Guid.NewGuid(), externalProfileId, userId, currency);

        wallet.AddDomainEvent(new WalletCreatedDomainEvent(
            wallet.Id, externalProfileId, userId, wallet.Currency));

        return Result.Success(wallet);
    }

    // ── Deposit ───────────────────────────────────────────────────────────────

    public Result<Transaction> Deposit(decimal amount, string description, Guid? referenceId = null)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<Transaction>("Deposit amount must be positive.");

        Balance += amount;

        var tx = new Transaction(
            Id, TransactionType.Deposit, TransactionDirection.Credit,
            amount, 0m, Balance, Currency, description, referenceId, "Deposit");

        _transactions.Add(tx);

        AddDomainEvent(new DepositCompletedDomainEvent(Id, tx.Id, amount, Currency, Balance));

        return Result.Success(tx);
    }

    // ── Withdrawal ───────────────────────────────────────────────────────────

    public Result<Transaction> RequestWithdrawal(decimal amount, string description, Guid? referenceId = null)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<Transaction>("Withdrawal amount must be positive.");
        if (Balance < amount)
            return Result.Failure<Transaction>("Insufficient balance.");

        Balance -= amount;

        var tx = new Transaction(
            Id, TransactionType.Withdrawal, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description, referenceId, "Withdrawal",
            status: TransactionStatus.Pending);

        _transactions.Add(tx);

        AddDomainEvent(new WithdrawalRequestedDomainEvent(Id, tx.Id, amount, Currency));

        return Result.Success(tx);
    }

    public Result ConfirmWithdrawal(Transaction transaction)
    {
        if (transaction.WalletId != Id)
            return Result.Failure("Transaction does not belong to this wallet.");
        if (transaction.Type != TransactionType.Withdrawal)
            return Result.Failure("Transaction is not a withdrawal.");

        transaction.MarkCompleted();
        AddDomainEvent(new WithdrawalCompletedDomainEvent(Id, transaction.Id, transaction.Amount, Currency));

        return Result.Success();
    }

    public Result FailWithdrawal(Transaction transaction)
    {
        if (transaction.WalletId != Id)
            return Result.Failure("Transaction does not belong to this wallet.");
        if (transaction.Type != TransactionType.Withdrawal)
            return Result.Failure("Transaction is not a withdrawal.");

        Balance += transaction.Amount;
        transaction.MarkFailed();
        AddDomainEvent(new WithdrawalFailedDomainEvent(Id, transaction.Id, transaction.Amount, Currency));

        return Result.Success();
    }



  
    public Result<(Transaction transferTx, Transaction feeTx)> DebitForTransfer(
        decimal amount,
        FeeCalculation fee,
        Guid receiverWalletId,
        string description)
    {
        if (!IsActive)
            return Result.Failure<(Transaction, Transaction)>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<(Transaction, Transaction)>("Transfer amount must be positive.");
        if (Balance < fee.TotalAmount)
            return Result.Failure<(Transaction, Transaction)>(
                $"Insufficient balance. Required: {fee.TotalAmount} {Currency}, Available: {Balance} {Currency}.");

        Balance -= amount;

        var transferTx = new Transaction(
            Id, TransactionType.TransferOut, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description,
            receiverWalletId, "WalletTransfer");

        _transactions.Add(transferTx);

        Balance -= fee.FeeAmount;

        var feeTx = new Transaction(
            Id, TransactionType.TransferFee, TransactionDirection.Debit,
            fee.FeeAmount, 0m, Balance, Currency,
            $"Transfer fee for {amount} {Currency}",
            transferTx.Id, "TransferFee");

        _transactions.Add(feeTx);

        return Result.Success((transferTx, feeTx));
    }

    // ── Transfer (crédit du côté destinataire) ────────────────────────────────

    public Result<Transaction> CreditFromTransfer(
        decimal amount,
        Guid senderWalletId,
        string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");

        Balance += amount;

        var tx = new Transaction(
            Id, TransactionType.TransferIn, TransactionDirection.Credit,
            amount, 0m, Balance, Currency, description,
            senderWalletId, "WalletTransfer");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    // ── Procedure Payment ─────────────────────────────────────────────────────

    public Result<Transaction> PayProcedure(decimal amount, Guid procedureId, string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<Transaction>("Amount must be positive.");
        if (Balance < amount)
            return Result.Failure<Transaction>(
                $"Insufficient balance. Required: {amount} {Currency}, Available: {Balance} {Currency}.");

        Balance -= amount;

        var tx = new Transaction(
            Id, TransactionType.ProcedurePayment, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description,
            procedureId, "Procedure");

        _transactions.Add(tx);

        AddDomainEvent(new ProcedurePaymentCompletedDomainEvent(
            Id, tx.Id, procedureId, amount, Currency));

        return Result.Success(tx);
    }

    // ── Marketplace Payment (débit acheteur) ──────────────────────────────────

    public new void AddDomainEvent(IDomainEvent domainEvent) => base.AddDomainEvent(domainEvent);
    public Result<Transaction> DebitForMarketplace(
        decimal amount,
        Guid serviceId,
        Guid sellerWalletId,
        string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<Transaction>("Amount must be positive.");
        if (Balance < amount)
            return Result.Failure<Transaction>(
                $"Insufficient balance. Required: {amount} {Currency}, Available: {Balance} {Currency}.");

        Balance -= amount;

        var tx = new Transaction(
            Id, TransactionType.MarketplacePayment, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description,
            serviceId, "MarketplaceService");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    // ── Marketplace Receipt (crédit vendeur) ──────────────────────────────────

    public Result<Transaction> CreditFromMarketplace(
        decimal amount,
        Guid serviceId,
        string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");

        Balance += amount;

        var tx = new Transaction(
            Id, TransactionType.MarketplaceReceipt, TransactionDirection.Credit,
            amount, 0m, Balance, Currency, description,
            serviceId, "MarketplaceService");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    // ── Freelance — Escrow (HoldFundsUpfront) ──────────────────────────────────

    public Result<Transaction> HoldForFreelance(decimal amount, Guid jobApplicationId, string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<Transaction>("Amount must be positive.");
        if (Balance < amount)
            return Result.Failure<Transaction>(
                $"Insufficient balance. Required: {amount} {Currency}, Available: {Balance} {Currency}.");

        Balance -= amount;
        HeldBalance += amount;

        var tx = new Transaction(
            Id, TransactionType.FreelanceEscrowHold, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description,
            jobApplicationId, "JobApplication");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    /// <summary>Libère le hold côté employeur (appelé sur le wallet de l'employeur, à la complétion).</summary>
    public Result<Transaction> ReleaseFreelanceHold(decimal amount, Guid jobApplicationId, string description)
    {
        if (HeldBalance < amount)
            return Result.Failure<Transaction>("Held balance is lower than the amount to release.");

        HeldBalance -= amount;

        var tx = new Transaction(
            Id, TransactionType.FreelanceEscrowRelease, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description,
            jobApplicationId, "JobApplication");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    /// <summary>Rembourse le hold vers le solde disponible de l'employeur (annulation après acceptation).</summary>
    public Result<Transaction> RefundFreelanceHold(decimal amount, Guid jobApplicationId, string description)
    {
        if (HeldBalance < amount)
            return Result.Failure<Transaction>("Held balance is lower than the amount to refund.");

        HeldBalance -= amount;
        Balance += amount;

        var tx = new Transaction(
            Id, TransactionType.FreelanceEscrowRefund, TransactionDirection.Credit,
            amount, 0m, Balance, Currency, description,
            jobApplicationId, "JobApplication");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    // ── Freelance — Paiement direct (PayOnCompletion) ───────────────────────────

    public Result<Transaction> DebitForFreelance(decimal amount, Guid jobApplicationId, string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");
        if (amount <= 0)
            return Result.Failure<Transaction>("Amount must be positive.");
        if (Balance < amount)
            return Result.Failure<Transaction>(
                $"Insufficient balance. Required: {amount} {Currency}, Available: {Balance} {Currency}.");

        Balance -= amount;

        var tx = new Transaction(
            Id, TransactionType.FreelancePayment, TransactionDirection.Debit,
            amount, 0m, Balance, Currency, description,
            jobApplicationId, "JobApplication");

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    public Result<Transaction> CreditFromFreelance(decimal amount, Guid jobApplicationId, string description)
    {
        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");

        Balance += amount;

        var tx = new Transaction(
            Id, TransactionType.FreelanceReceipt, TransactionDirection.Credit,
            amount, 0m, Balance, Currency, description,
            jobApplicationId, "JobApplication");

        _transactions.Add(tx);

        return Result.Success(tx);
    }
}