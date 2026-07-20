using Shared.Messaging.Constants;
using Shared.Messaging.Events.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Chat.Infrastructure.Extensions;

public static class WolverineChatExtensions
{
    public static void ConfigureChatDomainQueues(this WolverineOptions options)
    {
        options.PublishMessage<MessageSentIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ChatEventsExchange);

        options.PublishMessage<CallStartedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ChatEventsExchange);

        options.PublishMessage<CallEndedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ChatEventsExchange);
    }
}