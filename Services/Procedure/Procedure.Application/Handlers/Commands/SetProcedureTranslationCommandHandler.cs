using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Handlers.Commands;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;

namespace Procedure.Application.Handlers.Commands;

public class SetProcedureTranslationCommandHandler(
    IProcedureRepository procedureRepo,
    ITranslationRepository<Procedure.Domain.Entities.Procedure> translationRepo,
    IUnitOfWork uow)
    : SetTranslationCommandHandler<Procedure.Domain.Entities.Procedure>(procedureRepo, translationRepo, uow)
{
}