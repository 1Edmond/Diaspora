using Shared.Messaging.Constants;
using Shared.Messaging.Events.Freelance;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Freelance.Infrastructure.Extensions;

public static class WolverineFreelanceExtensions
{
    public static WolverineOptions ConfigureFreelanceDomainQueues(this WolverineOptions options)
    {
        options.PublishMessage<JobPostingPublishedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.FreelanceEventsExchange);

        options.PublishMessage<JobMatchFoundIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.FreelanceEventsExchange);

        options.PublishMessage<JobApplicationCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.FreelanceEventsExchange);

        options.PublishMessage<JobApplicationAcceptedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.FreelanceEventsExchange);

        options.PublishMessage<JobApplicationRejectedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.FreelanceEventsExchange);

        options.PublishMessage<JobApplicationCompletedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.FreelanceEventsExchange);

        return options;
    }
}