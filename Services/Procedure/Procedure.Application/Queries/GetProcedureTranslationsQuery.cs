using Procedure.Application.DTOs;
using Shared.Application.Queries;
using Shared.Domain.Interfaces;
using System;

namespace Procedure.Application.Queries;

public record GetProcedureTranslationsQuery(
    Guid EntityId
) : GetTranslationsQuery<Procedure.Domain.Entities.Procedure>(EntityId);