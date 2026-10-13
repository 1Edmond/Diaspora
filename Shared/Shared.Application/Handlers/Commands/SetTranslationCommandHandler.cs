using Shared.Application.Abstractions;
using Shared.Application.Commands;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Domain.Translations;
using System;
using System.Collections.Generic;

namespace Shared.Application.Handlers.Commands;

public class SetTranslationCommandHandler<TOwner>(
    IRepository<TOwner> ownerRepo,
    ITranslationRepository<TOwner> translationRepo,
    IUnitOfWork uow)
    where TOwner : AggregateRoot, Shared.Domain.Interfaces.ITranslatable
{
    private readonly IRepository<TOwner> _ownerRepo = ownerRepo;
    private readonly ITranslationRepository<TOwner> _translationRepo = translationRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result> Handle(SetTranslationCommand<TOwner> command, CancellationToken cancellationToken)
    {
        var entity = await _ownerRepo.GetByIdAsync(command.EntityId);
        if (entity is null)
            return Result.Failure("Entity not found");

        var normalizedCulture = command.CultureCode.Trim().ToLowerInvariant();
        var translation = await _translationRepo.GetByOwnerIdAndCultureAsync(entity.Id, normalizedCulture);

        if (translation is null)
        {
            var newTranslation = Translation<TOwner>.Create(entity.Id, normalizedCulture, command.Fields);
            await _translationRepo.AddAsync(newTranslation);
        }
        else
        {
            foreach (var (field, value) in command.Fields)
                translation.SetField(field, value);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}