using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Freelance;
using System;
using System.Collections.Generic;
using System.Text;
using Freelance.Domain.Events;

namespace Freelance.Application.Mappings;

public sealed class FreelanceIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            JobPostingPublishedEvent e => new JobPostingPublishedIntegrationEvent(
                e.JobPostingId, e.EmployerId, e.CategoryId, e.Title, e.City, e.Country, e.RequiredSkills),

            JobMatchFoundEvent e => new JobMatchFoundIntegrationEvent(
                e.JobPostingId, e.WorkerId, e.EmployerId, e.CategoryId, e.Title),

            JobApplicationCreatedEvent e => new JobApplicationCreatedIntegrationEvent(
                e.JobApplicationId, e.JobPostingId, e.WorkerId, e.EmployerId),

            JobApplicationAcceptedEvent e => new JobApplicationAcceptedIntegrationEvent(
                e.JobApplicationId, e.JobPostingId, e.WorkerId, e.DecidedBy),

            JobApplicationRejectedEvent e => new JobApplicationRejectedIntegrationEvent(
                e.JobApplicationId, e.JobPostingId, e.WorkerId, e.DecidedBy, e.Reason),

            JobApplicationCompletedEvent e => new JobApplicationCompletedIntegrationEvent(
                e.JobApplicationId, e.JobPostingId, e.WorkerId, e.EmployerId),

            _ => null
        };
}