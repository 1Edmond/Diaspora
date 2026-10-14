using Shared.Messaging.Constants;
using Shared.Messaging.Events.Profile;
using Shared.Messaging.Events.Wallet;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Wallet.Infrastructure.Extensions;

public static class WolverineProfileExtensions
{
    /// <summary>
    /// Configure les exchanges et queues RabbitMQ spécifiques au Profile Service.
    /// Appelé dans SetCoreInfrastructureServices.
    /// </summary>
    public static void ConfigureWalletDomainQueues(this WolverineOptions options)
    {
        // Exchange sortant — Profile Service publie ses events vers les autres services
        options.PublishMessage<DepositCompletedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<MarketplacePaymentHeldIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<MarketplacePaymentFailedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<MarketplacePaymentReleasedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<MarketplacePaymentRefundedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<ProcedurePaymentCompletedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<TransferCompletedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
        options.PublishMessage<WalletCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.WalletEventsExchange);
    }
}
