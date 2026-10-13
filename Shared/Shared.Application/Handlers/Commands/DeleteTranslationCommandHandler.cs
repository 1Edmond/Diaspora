using Shared.Application.Abstractions;
using Shared.Application.Commands;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Domain.Translations;
using System;

namespace Shared.Application.Handlers.Commands;

public class DeleteTranslationCommandHandler<TOwner>(
    ITranslationRepository<TOwner> translationRepo,
    IUnitOfWork uow)
    where TOwner : AggregateRoot, Shared.Domain.Interfaces.ITranslatable
{
    private readonly ITranslationRepository<TOwner> _translationRepo = translationRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result> Handle(DeleteTranslationCommand<TOwner> command, CancellationToken cancellationToken)
    {
        var normalizedCulture = command.CultureCode.Trim().ToLowerInvariant();
        var translation = await _translationRepo.GetByOwnerIdAndCultureAsync(command.EntityId, normalizedCulture);
        if (translation is null)
            return Result.Failure("Translation not found");

        await _translationRepo.DeleteAsync(translation);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}