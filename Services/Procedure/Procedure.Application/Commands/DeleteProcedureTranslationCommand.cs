using Shared.Application.Commands;
using Shared.Domain.Interfaces;
using System;

namespace Procedure.Application.Commands;

public record DeleteProcedureTranslationCommand(
    Guid EntityId,
    string CultureCode
) : DeleteTranslationCommand<Procedure.Domain.Entities.Procedure>(EntityId, CultureCode);