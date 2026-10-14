using FluentAssertions;
using NSubstitute;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Interfaces;
using Wallet.Domain.Entities;
using Wallet.Domain.Enums;
using Wallet.Infrastructure.Consumers;
using Wolverine;
using Xunit;

namespace Wallet.UnitTests;

public class MarketplaceCommissionEscrowTests
{
    [Fact]
    public void ReleaseMarketplaceHold_WithPlatformCommission_ShouldSplitFundsStrictly()
    {
        // Arrange : Séquestre de 10 000 XOF avec 10% de commission (1 000 XOF)
        var totalAmount = 10_000m;
        var commissionAmount = 1_000m;
        var netSellerExpected = 9_000m;

        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var platform = Domain.Entities.Wallet.Create(Domain.Entities.Wallet.PlatformProfileId, Domain.Entities.Wallet.PlatformUserId, "XOF").Value;

        buyer.Deposit(totalAmount, "Dépôt initial acheteur");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, totalAmount, "XOF", sellerProfileId: seller.ExternalProfileId);

        var totalInitialFunds = buyer.Balance + buyer.HeldBalance + seller.Balance + seller.HeldBalance + platform.Balance + platform.HeldBalance;
        totalInitialFunds.Should().Be(totalAmount);

        // Act
        var result = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: totalAmount,
            commissionAmount: commissionAmount,
            platformWallet: platform,
            description: "Libération avec 10% commission");

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 1. Acheteur : HeldBalance décrémenté de 10 000 XOF, Balance inchangée (0 XOF)
        buyer.HeldBalance.Should().Be(0m);
        buyer.Balance.Should().Be(0m);

        // 2. Vendeur : Balance créditée du net (9 000 XOF)
        seller.Balance.Should().Be(netSellerExpected);
        seller.HeldBalance.Should().Be(0m);

        // 3. Plateforme : Balance créditée de la commission (1 000 XOF)
        platform.Balance.Should().Be(commissionAmount);
        platform.HeldBalance.Should().Be(0m);

        // 4. Invariant strict de conservation monétaire : total des fonds constant
        var totalFinalFunds = buyer.Balance + buyer.HeldBalance + seller.Balance + seller.HeldBalance + platform.Balance + platform.HeldBalance;
        totalFinalFunds.Should().Be(totalInitialFunds, "Aucun centime ne doit être créé ni détruit lors du split de commission");

        // 5. Vérification des écritures comptables
        var buyerTx = buyer.Transactions.Last(t => t.Type == TransactionType.MarketplacePaymentRelease);
        buyerTx.Amount.Should().Be(totalAmount);
        buyerTx.Direction.Should().Be(TransactionDirection.Debit);

        var sellerTx = seller.Transactions.Last(t => t.Type == TransactionType.MarketplacePaymentRelease);
        sellerTx.Amount.Should().Be(netSellerExpected);
        sellerTx.Direction.Should().Be(TransactionDirection.Credit);

        var platformTx = platform.Transactions.Last(t => t.Type == TransactionType.MarketplacePlatformCommission);
        platformTx.Amount.Should().Be(commissionAmount);
        platformTx.Direction.Should().Be(TransactionDirection.Credit);
    }

    [Fact]
    public void ReleaseMarketplaceHold_WithPlatformCommission_Replay_ShouldBeStrictlyIdempotent()
    {
        // Arrange
        var totalAmount = 10_000m;
        var commissionAmount = 1_000m;

        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var platform = Domain.Entities.Wallet.Create(Domain.Entities.Wallet.PlatformProfileId, Domain.Entities.Wallet.PlatformUserId, "XOF").Value;

        buyer.Deposit(totalAmount, "Dépôt initial acheteur");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, totalAmount, "XOF", sellerProfileId: seller.ExternalProfileId);

        // Première exécution
        var firstResult = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: totalAmount,
            commissionAmount: commissionAmount,
            platformWallet: platform);

        firstResult.IsSuccess.Should().BeTrue();

        var buyerTransactionsCountBefore = buyer.Transactions.Count;
        var sellerTransactionsCountBefore = seller.Transactions.Count;
        var platformTransactionsCountBefore = platform.Transactions.Count;

        // Act : Rejeu de la libération (idempotence)
        var secondResult = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: totalAmount,
            commissionAmount: commissionAmount,
            platformWallet: platform);

        // Assert
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.Id.Should().Be(firstResult.Value.Id);

        // Les soldes ne doivent pas être doublés
        buyer.HeldBalance.Should().Be(0m);
        buyer.Balance.Should().Be(0m);
        seller.Balance.Should().Be(9_000m);
        platform.Balance.Should().Be(1_000m);

        // Conservation des fonds
        (buyer.Balance + buyer.HeldBalance + seller.Balance + seller.HeldBalance + platform.Balance + platform.HeldBalance)
            .Should().Be(totalAmount);

        // Aucune transaction supplémentaire créée
        buyer.Transactions.Count.Should().Be(buyerTransactionsCountBefore);
        seller.Transactions.Count.Should().Be(sellerTransactionsCountBefore);
        platform.Transactions.Count.Should().Be(platformTransactionsCountBefore);
    }

    [Fact]
    public void ReleaseMarketplaceHold_WhenCommissionExceedsAmount_ShouldFail()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var platform = Domain.Entities.Wallet.Create(Domain.Entities.Wallet.PlatformProfileId, Domain.Entities.Wallet.PlatformUserId, "XOF").Value;

        buyer.Deposit(5_000m, "Dépôt initial");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 5_000m, "XOF", sellerProfileId: seller.ExternalProfileId);

        // Act : Commission supérieure au montant
        var result = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: 5_000m,
            commissionAmount: 6_000m,
            platformWallet: platform);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Commission");
        buyer.HeldBalance.Should().Be(5_000m);
        seller.Balance.Should().Be(0m);
        platform.Balance.Should().Be(0m);
    }

    [Fact]
    public void ReleaseMarketplaceHold_WhenCommissionIsNegative_ShouldFail()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var platform = Domain.Entities.Wallet.Create(Domain.Entities.Wallet.PlatformProfileId, Domain.Entities.Wallet.PlatformUserId, "XOF").Value;

        buyer.Deposit(5_000m, "Dépôt initial");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 5_000m, "XOF", sellerProfileId: seller.ExternalProfileId);

        // Act
        var result = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: 5_000m,
            commissionAmount: -100m,
            platformWallet: platform);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Commission");
    }

    [Fact]
    public void ReleaseMarketplaceHold_WhenCommissionPositiveAndPlatformWalletNull_ShouldFail()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;

        buyer.Deposit(5_000m, "Dépôt initial");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 5_000m, "XOF", sellerProfileId: seller.ExternalProfileId);

        // Act : Commission positive sans wallet plateforme
        var result = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: 5_000m,
            commissionAmount: 500m,
            platformWallet: null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Platform wallet is required");
    }

    [Fact]
    public void ReleaseMarketplaceHold_WhenPlatformCurrencyMismatch_ShouldFail()
    {
        // Arrange
        var buyer = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "XOF").Value;
        var platform = Domain.Entities.Wallet.Create(Domain.Entities.Wallet.PlatformProfileId, Domain.Entities.Wallet.PlatformUserId, "EUR").Value;

        buyer.Deposit(5_000m, "Dépôt initial");
        var serviceRequestId = Guid.NewGuid();
        buyer.HoldForMarketplace(serviceRequestId, 5_000m, "XOF", sellerProfileId: seller.ExternalProfileId);

        // Act
        var result = buyer.ReleaseMarketplaceHold(
            serviceRequestId,
            seller,
            amount: 5_000m,
            commissionAmount: 500m,
            platformWallet: platform);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Currency mismatch");
    }

    [Fact]
    public async Task Consumer_PaymentReleaseRequested_WithCommission_ShouldCreditSellerAndPlatform()
    {
        // Arrange
        var walletRepository = NSubstitute.Substitute.For<IWalletRepository>();
        var transactionRepository = NSubstitute.Substitute.For<ITransactionRepository>();
        var messageBus = NSubstitute.Substitute.For<IMessageBus>();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<MarketplacePaymentReleaseRequestedConsumer>>();

        var consumer = new MarketplacePaymentReleaseRequestedConsumer(
            walletRepository, transactionRepository, messageBus, logger);

        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        var buyer = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(sellerProfileId, Guid.NewGuid(), "XOF").Value;
        var platform = Domain.Entities.Wallet.Create(Domain.Entities.Wallet.PlatformProfileId, Domain.Entities.Wallet.PlatformUserId, "XOF").Value;

        buyer.Deposit(10_000m, "Dépôt initial");
        var holdTx = buyer.HoldForMarketplace(serviceRequestId, 10_000m, "XOF", sellerProfileId: sellerProfileId).Value;

        walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>()).Returns(buyer);
        walletRepository.GetByExternalProfileIdAsync(sellerProfileId, Arg.Any<CancellationToken>()).Returns(seller);
        walletRepository.GetPlatformRevenueWalletAsync("XOF", Arg.Any<CancellationToken>()).Returns(platform);
        transactionRepository.GetHoldTransactionByReferenceIdAsync(serviceRequestId, Arg.Any<CancellationToken>()).Returns(holdTx);

        var @event = new MarketplacePaymentReleaseRequestedIntegrationEvent(
            serviceRequestId,
            buyerProfileId,
            sellerProfileId,
            CommissionAmount: 1_000m);

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        buyer.HeldBalance.Should().Be(0m);
        buyer.Balance.Should().Be(0m);
        seller.Balance.Should().Be(9_000m);
        platform.Balance.Should().Be(1_000m);

        await walletRepository.Received(1).UpdateAsync(buyer, Arg.Any<CancellationToken>());
        await walletRepository.Received(1).UpdateAsync(seller, Arg.Any<CancellationToken>());
        await walletRepository.Received(1).UpdateAsync(platform, Arg.Any<CancellationToken>());

        await messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentReleasedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId));
    }

    [Fact]
    public async Task Consumer_PaymentReleaseRequested_WithCommission_WhenPlatformWalletDoesNotExist_ShouldAutoCreateAndCredit()
    {
        // Arrange
        var walletRepository = NSubstitute.Substitute.For<IWalletRepository>();
        var transactionRepository = NSubstitute.Substitute.For<ITransactionRepository>();
        var messageBus = NSubstitute.Substitute.For<IMessageBus>();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<MarketplacePaymentReleaseRequestedConsumer>>();

        var consumer = new MarketplacePaymentReleaseRequestedConsumer(
            walletRepository, transactionRepository, messageBus, logger);

        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        var buyer = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "XOF").Value;
        var seller = Domain.Entities.Wallet.Create(sellerProfileId, Guid.NewGuid(), "XOF").Value;

        buyer.Deposit(10_000m, "Dépôt initial");
        var holdTx = buyer.HoldForMarketplace(serviceRequestId, 10_000m, "XOF", sellerProfileId: sellerProfileId).Value;

        walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>()).Returns(buyer);
        walletRepository.GetByExternalProfileIdAsync(sellerProfileId, Arg.Any<CancellationToken>()).Returns(seller);
        walletRepository.GetPlatformRevenueWalletAsync("XOF", Arg.Any<CancellationToken>()).Returns((Domain.Entities.Wallet?)null);
        transactionRepository.GetHoldTransactionByReferenceIdAsync(serviceRequestId, Arg.Any<CancellationToken>()).Returns(holdTx);

        var @event = new MarketplacePaymentReleaseRequestedIntegrationEvent(
            serviceRequestId,
            buyerProfileId,
            sellerProfileId,
            CommissionAmount: 1_000m);

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        buyer.HeldBalance.Should().Be(0m);
        seller.Balance.Should().Be(9_000m);

        // Le wallet plateforme a été auto-provisionné via AddAsync puis UpdateAsync
        await walletRepository.Received(1).AddAsync(
            Arg.Is<Domain.Entities.Wallet>(w => w.ExternalProfileId == Domain.Entities.Wallet.PlatformProfileId && w.Currency == "XOF"),
            Arg.Any<CancellationToken>());
        await walletRepository.Received(1).UpdateAsync(
            Arg.Is<Domain.Entities.Wallet>(w => w.ExternalProfileId == Domain.Entities.Wallet.PlatformProfileId && w.Balance == 1_000m),
            Arg.Any<CancellationToken>());

        await messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentReleasedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId));
    }
}
