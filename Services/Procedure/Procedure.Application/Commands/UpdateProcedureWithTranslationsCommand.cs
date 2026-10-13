using Procedure.Application.DTOs;
using Shared.Application.Commands;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Procedure.Application.Commands;

public record UpdateProcedureWithTranslationsCommand(
    Guid EntityId,
    ProcedureDto EntityDto,
    Dictionary<string, Dictionary<string, string>> Translations,
    HashSet<string>? TranslationsToRemove = null
) : UpdateWithTranslationsCommand<Procedure.Domain.Entities.Procedure, ProcedureDto>(EntityId, EntityDto, Translations, TranslationsToRemove);