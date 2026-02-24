using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Procedure;

public sealed record UserProcedureStartedIntegrationEvent(
    Guid UserProcedureId,
    Guid ProfileId,
    Guid ProcedureId,
    string ProcedureTitle
) : IntegrationEvent;