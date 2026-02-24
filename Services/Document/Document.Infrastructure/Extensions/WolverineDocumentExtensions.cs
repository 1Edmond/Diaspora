using Shared.Messaging.Constants;
using Shared.Messaging.Events.Document;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Document.Infrastructure.Extensions;

public static class WolverineDocumentExtensions
{
    public static void ConfigureDocumentDomainQueues(this WolverineOptions options)
    {
        options.PublishMessage<DocumentUploadedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.DocumentEventsExchange);

        options.PublishMessage<DocumentValidatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.DocumentEventsExchange);

        options.PublishMessage<DocumentRejectedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.DocumentEventsExchange);

        options.PublishMessage<DocumentExpiredIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.DocumentEventsExchange);

        options.PublishMessage<DocumentRenewalAlertIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.DocumentEventsExchange);
    }
}