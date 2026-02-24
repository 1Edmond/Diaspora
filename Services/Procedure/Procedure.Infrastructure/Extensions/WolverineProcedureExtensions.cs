using Shared.Messaging.Constants;
using Shared.Messaging.Events.Procedure;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Procedure.Infrastructure.Extensions;

public static class WolverineProcedureExtensions
{
    public static void ConfigureProcedureDomainQueues(this WolverineOptions options)
    {
        options.PublishMessage<UserProcedureStartedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProcedureEventsExchange);

        options.PublishMessage<UserProcedureCompletedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProcedureEventsExchange);

        options.PublishMessage<UserProcedureSkippedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProcedureEventsExchange);
    }
}
