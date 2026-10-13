using Procedure.Application.DTOs;
using Shared.Application.Commands;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Procedure.Application.Commands;

public record CreateProcedureWithTranslationsCommand(
    ProcedureDto EntityDto,
    Dictionary<string, Dictionary<string, string>> Translations
) : CreateWithTranslationsCommand<Procedure.Domain.Entities.Procedure, ProcedureDto>(EntityDto, Translations);