using Shared.Application.Commands;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Procedure.Application.Commands;

public record SetProcedureTranslationCommand(
    Guid EntityId,
    string CultureCode,
    Dictionary<string, string> Fields
) : SetTranslationCommand<Procedure.Domain.Entities.Procedure>(EntityId, CultureCode, Fields);