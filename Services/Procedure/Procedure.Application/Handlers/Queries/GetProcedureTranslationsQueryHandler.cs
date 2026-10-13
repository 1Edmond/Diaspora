using Procedure.Application.DTOs;
using Procedure.Application.Queries;
using Shared.Application.Handlers.Queries;
using Shared.Application.Dtos;
using Shared.Domain.Interfaces;
using Shared.Application.Abstractions;

namespace Procedure.Application.Handlers.Queries;

public class GetProcedureTranslationsQueryHandler(
    ITranslationRepository<Procedure.Domain.Entities.Procedure> translationRepo)
    : GetTranslationsQueryHandler<Procedure.Domain.Entities.Procedure>(translationRepo)
{
}