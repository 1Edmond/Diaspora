using Shared.Application.Abstractions;
using Shared.Application.Commands;
using Shared.Domain.Commons;
using Shared.Domain.Constants;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Domain.Translations;
using System;
using System.Collections.Generic;

namespace Shared.Application.Handlers.Commands;

public abstract class UpdateWithTranslationsCommandHandler<TOwner, TEntityDto>
    where TOwner : AggregateRoot, Shared.Domain.Interfaces.ITranslatable
{
    protected readonly IRepository<TOwner> OwnerRepo;
    protected readonly ITranslationRepository<TOwner> TranslationRepo;
    protected readonly IUnitOfWork Uow;

    protected UpdateWithTranslationsCommandHandler(
        IRepository<TOwner> ownerRepo,
        ITranslationRepository<TOwner> translationRepo,
        IUnitOfWork uow)
    {
        OwnerRepo = ownerRepo;
        TranslationRepo = translationRepo;
        Uow = uow;
    }

    protected abstract void UpdateEntity(TOwner entity, TEntityDto dto);

    public async Task<Result> Handle(UpdateWithTranslationsCommand<TOwner, TEntityDto> command, CancellationToken cancellationToken)
    {
        var entity = await OwnerRepo.GetByIdAsync(command.EntityId);
        if (entity is null)
            return Result.Failure("Entity not found");

        if (command.EntityDto is not TEntityDto dto)
            return Result.Failure($"Invalid DTO type. Expected {typeof(TEntityDto).Name}.");

        UpdateEntity(entity, dto);

        foreach (var (cultureCode, fields) in command.Translations)
        {
            var normalizedCulture = cultureCode.Trim().ToLowerInvariant();
            var existing = await TranslationRepo.GetByOwnerIdAndCultureAsync(entity.Id, normalizedCulture);
            if (existing is null)
            {
                var translation = Translation<TOwner>.Create(entity.Id, normalizedCulture, fields);
                await TranslationRepo.AddAsync(translation);
            }
            else
            {
                foreach (var (field, value) in fields)
                    existing.SetField(field, value);
            }
        }

        if (command.TranslationsToRemove != null)
        {
            foreach (var cultureCode in command.TranslationsToRemove)
            {
                var normalizedCulture = cultureCode.Trim().ToLowerInvariant();
                var existing = await TranslationRepo.GetByOwnerIdAndCultureAsync(entity.Id, normalizedCulture);
                if (existing is not null)
                    await TranslationRepo.DeleteAsync(existing);
            }
        }

        await Uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}