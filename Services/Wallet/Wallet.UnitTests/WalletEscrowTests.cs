using FluentAssertions;
using System;
using Wallet.Domain.Entities;
using Wallet.Domain.Enums;
using Xunit;

namespace Wallet.UnitTests;

public class WalletEscrowTests
{
    [Fact]
    public void HoldForMarketplace_WhenValid_ShouldDebitBalanceAndCreditHeldBalance()
    {
        // Arrange
        var wallet = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        wallet.Deposit(100m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();

        // Act
        var result = wallet.HoldForMarketplace(serviceRequestId, 40m, "EUR", "Escrow hold");

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(60m);
        wallet.HeldBalance.Should().Be(40m);
        (wallet.Balance + wallet.HeldBalance).Should().Be(100m);
        result.Value.Type.Should().Be(TransactionType.MarketplacePaymentHold);
        result.Value.Amount.Should().Be(40m);
    }

    [Fact]
    public void HoldForMarketplace_DoubleCall_ShouldBeStrictlyIdempotent()
    {
        // Arrange
        var wallet = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        wallet.Deposit(100m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();

        // Act
        var firstResult = wallet.HoldForMarketplace(serviceRequestId, 40m, "EUR");
        var secondResult = wallet.HoldForMarketplace(serviceRequestId, 40m, "EUR");

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.Id.Should().Be(firstResult.Value.Id);
        wallet.Balance.Should().Be(60m);
        wallet.HeldBalance.Should().Be(40m);
        (wallet.Balance + wallet.HeldBalance).Should().Be(100m);
        wallet.Transactions.Count.Should().Be(2); // 1 deposit + 1 hold
    }

    [Fact]
    public void HoldForMarketplace_WhenInsufficientBalance_ShouldFail()
    {
        // Arrange
        var wallet = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        wallet.Deposit(30m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();

        // Act
        var result = wallet.HoldForMarketplace(serviceRequestId, 50m, "EUR");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient balance");
        wallet.Balance.Should().Be(30m);
        wallet.HeldBalance.Should().Be(0m);
    }

    [Fact]
    public void HoldForMarketplace_WhenCurrencyMismatch_ShouldFail()
    {
        // Arrange
        var wallet = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        wallet.Deposit(100m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();

        // Act
        var result = wallet.HoldForMarketplace(serviceRequestId, 50m, "USD");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Currency mismatch");
        wallet.Balance.Should().Be(100m);
        wallet.HeldBalance.Should().Be(0m);
    }

    [Fact]
    public void HoldForMarketplace_WhenAmountIsZeroOrNegative_ShouldFail()
    {
        // Arrange
        var wallet = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        wallet.Deposit(100m, "Initial deposit");

        // Act
        var resultZero = wallet.HoldForMarketplace(Guid.NewGuid(), 0m, "EUR");
        var resultNegative = wallet.HoldForMarketplace(Guid.NewGuid(), -10m, "EUR");

        // Assert
        resultZero.IsFailure.Should().BeTrue();
        resultNegative.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReleaseMarketplaceHold_WhenValid_ShouldTransferFundsToSeller()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        buyer.Deposit(100m, "Initial deposit");
        seller.Deposit(20m, "Seller initial deposit");

        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 40m, "EUR", sellerProfileId: seller.ExternalProfileId);

        // Act
        var result = buyer.ReleaseMarketplaceHold(serviceRequestId, seller);

        // Assert
        result.IsSuccess.Should().BeTrue();
        buyer.Balance.Should().Be(60m);
        buyer.HeldBalance.Should().Be(0m);
        seller.Balance.Should().Be(60m); // 20 + 40
        seller.HeldBalance.Should().Be(0m);

        // Conservation des fonds globaux (100 + 20 = 120 = 60 + 60)
        (buyer.Balance + buyer.HeldBalance + seller.Balance + seller.HeldBalance).Should().Be(120m);
    }

    [Fact]
    public void ReleaseMarketplaceHold_DoubleCall_ShouldBeStrictlyIdempotent()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        buyer.Deposit(100m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 40m, "EUR", sellerProfileId: seller.ExternalProfileId);

        // Act
        var first = buyer.ReleaseMarketplaceHold(serviceRequestId, seller);
        var second = buyer.ReleaseMarketplaceHold(serviceRequestId, seller);

        // Assert
        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Value.Id.Should().Be(first.Value.Id);
        buyer.Balance.Should().Be(60m);
        buyer.HeldBalance.Should().Be(0m);
        seller.Balance.Should().Be(40m);
        (buyer.Balance + buyer.HeldBalance + seller.Balance + seller.HeldBalance).Should().Be(100m);
    }

    [Fact]
    public void RefundMarketplaceHold_WhenValid_ShouldRestoreBuyerBalance()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        buyer.Deposit(100m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 40m, "EUR");

        // Act
        var result = buyer.RefundMarketplaceHold(serviceRequestId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        buyer.Balance.Should().Be(100m);
        buyer.HeldBalance.Should().Be(0m);
        (buyer.Balance + buyer.HeldBalance).Should().Be(100m);
    }

    [Fact]
    public void RefundMarketplaceHold_DoubleCall_ShouldBeStrictlyIdempotent()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "EUR").Value;
        buyer.Deposit(100m, "Initial deposit");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 40m, "EUR");

        // Act
        var first = buyer.RefundMarketplaceHold(serviceRequestId);
        var second = buyer.RefundMarketplaceHold(serviceRequestId);

        // Assert
        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Value.Id.Should().Be(first.Value.Id);
        buyer.Balance.Should().Be(100m);
        buyer.HeldBalance.Should().Be(0m);
        (buyer.Balance + buyer.HeldBalance).Should().Be(100m);
    }

    [Fact]
    public void FundsConservationInvariant_AcrossRandomOperations_ShouldAlwaysHold()
    {
        // Arrange
        var initialBuyerBalance = 1000m;
        var initialSellerBalance = 500m;
        var totalInitialFunds = initialBuyerBalance + initialSellerBalance;

        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;

        buyer.Deposit(initialBuyerBalance, "Deposit Buyer");
        seller.Deposit(initialSellerBalance, "Deposit Seller");

        var rng = new Random(42);
        var requests = new System.Collections.Generic.List<Guid>();

        for (int i = 0; i < 50; i++)
        {
            var op = rng.Next(0, 3);
            if (op == 0) // Hold
            {
                var reqId = Guid.NewGuid();
                var amount = (decimal)rng.Next(10, 100);
                if (buyer.Balance >= amount)
                {
                    var res = buyer.HoldForMarketplace(reqId, amount, "XOF", sellerProfileId: seller.ExternalProfileId);
                    if (res.IsSuccess)
                        requests.Add(reqId);
                }
            }
            else if (op == 1 && requests.Count > 0) // Release
            {
                var reqId = requests[rng.Next(0, requests.Count)];
                buyer.ReleaseMarketplaceHold(reqId, seller);
            }
            else if (op == 2 && requests.Count > 0) // Refund
            {
                var reqId = requests[rng.Next(0, requests.Count)];
                buyer.RefundMarketplaceHold(reqId);
            }

            // Invariant strict à chaque étape : somme de tous les soldes disponibles et séquestrés = fonds initiaux
            var currentTotal = buyer.Balance + buyer.HeldBalance + seller.Balance + seller.HeldBalance;
            currentTotal.Should().Be(totalInitialFunds, "Total funds must be conserved at all times");
        }
    }
}
