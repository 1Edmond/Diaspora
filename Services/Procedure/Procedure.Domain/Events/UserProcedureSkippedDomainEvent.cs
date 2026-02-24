using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Events;

public sealed record UserProcedureSkippedDomainEvent(
    Guid UserProcedureId,
    Guid ProfileId,
    Guid ProcedureId,
    string ProcedureTitle
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}