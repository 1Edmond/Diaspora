using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Procedure;

public sealed record UserProcedureCompletedIntegrationEvent(
    Guid UserProcedureId,
    Guid ProfileId,
    Guid ProcedureId,
    string ProcedureTitle,
    string CompletedByUserId,
    string CompletedByRole
) : IntegrationEvent;