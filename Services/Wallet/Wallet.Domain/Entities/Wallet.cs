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
    public static readonly Guid PlatformProfileId = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid PlatformUserId = new("00000000-0000-0000-0000-000000000001");

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

    // ── Marketplace — Escrow (Hold / Release / Refund) ────────────────────────

    public new void AddDomainEvent(IDomainEvent domainEvent) => base.AddDomainEvent(domainEvent);

    public Result<Transaction> HoldForMarketplace(
        Guid serviceRequestId,
        decimal amount,
        string currency,
        string description = "",
        Guid? sellerProfileId = null)
    {
        var existingTx = _transactions.FirstOrDefault(t =>
            t.ReferenceId == serviceRequestId &&
            t.Type == TransactionType.MarketplacePaymentHold);

        if (existingTx is not null)
            return Result.Success(existingTx);

        if (!IsActive)
            return Result.Failure<Transaction>("Wallet is not active.");

        if (amount <= 0)
            return Result.Failure<Transaction>("Amount must be positive.");

        if (!string.Equals(Currency, currency, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<Transaction>(
                $"Currency mismatch. Wallet currency: {Currency}, requested: {currency}.");

        if (Balance < amount)
            return Result.Failure<Transaction>(
                $"Insufficient balance. Required: {amount} {Currency}, Available: {Balance} {Currency}.");

        Balance -= amount;
        HeldBalance += amount;

        var referenceType = sellerProfileId.HasValue
            ? $"SellerProfile:{sellerProfileId.Value}"
            : "ServiceRequest";

        var tx = new Transaction(
            Id,
            TransactionType.MarketplacePaymentHold,
            TransactionDirection.Debit,
            amount,
            0m,
            Balance,
            Currency,
            string.IsNullOrWhiteSpace(description) ? $"Hold for Marketplace service request {serviceRequestId}" : description,
            serviceRequestId,
            referenceType);

        _transactions.Add(tx);

        return Result.Success(tx);
    }

    public Result<Transaction> ReleaseMarketplaceHold(
        Guid serviceRequestId,
        Wallet sellerWallet,
        decimal amount = 0m,
        decimal commissionAmount = 0m,
        Wallet? platformWallet = null,
        string description = "")
    {
        var existingTx = _transactions.FirstOrDefault(t =>
            t.ReferenceId == serviceRequestId &&
            t.Type == TransactionType.MarketplacePaymentRelease);

        if (existingTx is not null)
            return Result.Success(existingTx);

        if (amount <= 0)
        {
            var holdTx = _transactions.FirstOrDefault(t =>
                t.ReferenceId == serviceRequestId &&
                t.Type == TransactionType.MarketplacePaymentHold);

            if (holdTx is null)
                return Result.Failure<Transaction>("No hold found for this service request.");

            amount = holdTx.Amount;
        }

        if (commissionAmount < 0m)
            return Result.Failure<Transaction>("Commission amount cannot be negative.");

        if (commissionAmount > amount)
            return Result.Failure<Transaction>("Commission amount cannot exceed the total release amount.");

        if (commissionAmount > 0m && platformWallet is null)
            return Result.Failure<Transaction>("Platform wallet is required when commission amount is greater than zero.");

        if (commissionAmount > 0m && platformWallet is not null)
        {
            if (!platformWallet.IsActive)
                return Result.Failure<Transaction>("Platform wallet is not active.");

            if (!string.Equals(Currency, platformWallet.Currency, StringComparison.OrdinalIgnoreCase))
                return Result.Failure<Transaction>(
                    $"Currency mismatch between buyer ({Currency}) and platform wallet ({platformWallet.Currency}).");
        }

        if (HeldBalance < amount)
            return Result.Failure<Transaction>("Held balance is lower than the amount to release.");

        if (!sellerWallet.IsActive)
            return Result.Failure<Transaction>("Seller wallet is not active.");

        if (!string.Equals(Currency, sellerWallet.Currency, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<Transaction>(
                $"Currency mismatch between buyer ({Currency}) and seller ({sellerWallet.Currency}).");

        var netSellerAmount = amount - commissionAmount;
        if (netSellerAmount < 0m)
            return Result.Failure<Transaction>("Net seller amount cannot be negative.");

        if (netSellerAmount + commissionAmount != amount)
            return Result.Failure<Transaction>("Sum of net seller amount and commission amount must equal total amount.");

        HeldBalance -= amount;

        var buyerTx = new Transaction(
            Id,
            TransactionType.MarketplacePaymentRelease,
            TransactionDirection.Debit,
            amount,
            0m,
            Balance,
            Currency,
            string.IsNullOrWhiteSpace(description) ? $"Release hold for Marketplace service request {serviceRequestId}" : description,
            serviceRequestId,
            "ServiceRequest");

        _transactions.Add(buyerTx);

        var sellerExistingTx = sellerWallet._transactions.FirstOrDefault(t =>
            t.ReferenceId == serviceRequestId &&
            t.Type == TransactionType.MarketplacePaymentRelease);

        if (sellerExistingTx is null)
        {
            sellerWallet.Balance += netSellerAmount;
            var sellerTx = new Transaction(
                sellerWallet.Id,
                TransactionType.MarketplacePaymentRelease,
                TransactionDirection.Credit,
                netSellerAmount,
                0m,
                sellerWallet.Balance,
                sellerWallet.Currency,
                string.IsNullOrWhiteSpace(description) ? $"Payment received for Marketplace service request {serviceRequestId}" : description,
                serviceRequestId,
                "ServiceRequest");

            sellerWallet._transactions.Add(sellerTx);
        }

        if (commissionAmount > 0m && platformWallet is not null)
        {
            var platformExistingTx = platformWallet._transactions.FirstOrDefault(t =>
                t.ReferenceId == serviceRequestId &&
                t.Type == TransactionType.MarketplacePlatformCommission);

            if (platformExistingTx is null)
            {
                platformWallet.Balance += commissionAmount;
                var platformTx = new Transaction(
                    platformWallet.Id,
                    TransactionType.MarketplacePlatformCommission,
                    TransactionDirection.Credit,
                    commissionAmount,
                    0m,
                    platformWallet.Balance,
                    platformWallet.Currency,
                    $"Platform commission for Marketplace service request {serviceRequestId}",
                    serviceRequestId,
                    "MarketplaceCommission");

                platformWallet._transactions.Add(platformTx);
            }
        }

        return Result.Success(buyerTx);
    }

    public Result<Transaction> RefundMarketplaceHold(
        Guid serviceRequestId,
        decimal amount = 0m,
        string description = "")
    {
        var existingTx = _transactions.FirstOrDefault(t =>
            t.ReferenceId == serviceRequestId &&
            t.Type == TransactionType.MarketplacePaymentRefund);

        if (existingTx is not null)
            return Result.Success(existingTx);

        if (amount <= 0)
        {
            var holdTx = _transactions.FirstOrDefault(t =>
                t.ReferenceId == serviceRequestId &&
                t.Type == TransactionType.MarketplacePaymentHold);

            if (holdTx is null)
                return Result.Failure<Transaction>("No hold found for this service request.");

            amount = holdTx.Amount;
        }

        if (HeldBalance < amount)
            return Result.Failure<Transaction>("Held balance is lower than the amount to refund.");

        HeldBalance -= amount;
        Balance += amount;

        var tx = new Transaction(
            Id,
            TransactionType.MarketplacePaymentRefund,
            TransactionDirection.Credit,
            amount,
            0m,
            Balance,
            Currency,
            string.IsNullOrWhiteSpace(description) ? $"Refund hold for Marketplace service request {serviceRequestId}" : description,
            serviceRequestId,
            "ServiceRequest");

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