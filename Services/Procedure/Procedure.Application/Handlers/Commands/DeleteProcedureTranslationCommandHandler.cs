using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Handlers.Commands;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;

namespace Procedure.Application.Handlers.Commands;

public class DeleteProcedureTranslationCommandHandler(
    ITranslationRepository<Procedure.Domain.Entities.Procedure> translationRepo,
    IUnitOfWork uow)
    : DeleteTranslationCommandHandler<Procedure.Domain.Entities.Procedure>(translationRepo, uow)
{
}