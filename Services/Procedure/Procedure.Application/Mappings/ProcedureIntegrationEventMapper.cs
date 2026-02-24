using Procedure.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Procedure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Mappings;

public sealed class ProcedureIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        UserProcedureStartedDomainEvent e => new UserProcedureStartedIntegrationEvent(
            e.UserProcedureId, e.ProfileId, e.ProcedureId, e.ProcedureTitle),

        UserProcedureCompletedDomainEvent e => new UserProcedureCompletedIntegrationEvent(
            e.UserProcedureId, e.ProfileId, e.ProcedureId, e.ProcedureTitle,
            e.CompletedByUserId, e.CompletedByRole),

        UserProcedureSkippedDomainEvent e => new UserProcedureSkippedIntegrationEvent(
            e.UserProcedureId, e.ProfileId, e.ProcedureId, e.ProcedureTitle),

        _ => null
    };
}
