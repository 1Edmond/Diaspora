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

public abstract class CreateWithTranslationsCommandHandler<TOwner, TEntityDto>
    where TOwner : AggregateRoot, Shared.Domain.Interfaces.ITranslatable
{
    protected readonly IRepository<TOwner> OwnerRepo;
    protected readonly ITranslationRepository<TOwner> TranslationRepo;
    protected readonly IUnitOfWork Uow;

    protected CreateWithTranslationsCommandHandler(
        IRepository<TOwner> ownerRepo,
        ITranslationRepository<TOwner> translationRepo,
        IUnitOfWork uow)
    {
        OwnerRepo = ownerRepo;
        TranslationRepo = translationRepo;
        Uow = uow;
    }

    protected abstract TOwner CreateEntity(TEntityDto dto);
    protected abstract Dictionary<string, string> GetTranslatableFields(TOwner entity);

    public async Task<Result<Guid>> Handle(CreateWithTranslationsCommand<TOwner, TEntityDto> command, CancellationToken cancellationToken)
    {
        if (command.EntityDto is not TEntityDto dto)
            return Result.Failure<Guid>($"Invalid DTO type. Expected {typeof(TEntityDto).Name}.");

        var entity = CreateEntity(dto);
        await OwnerRepo.AddAsync(entity);

        var defaultCulture = SharedConstants.DefaultCulture;
        var processedCultures = new HashSet<string>();

        foreach (var (cultureCode, fields) in command.Translations)
        {
            var normalizedCulture = cultureCode.Trim().ToLowerInvariant();
            if (processedCultures.Add(normalizedCulture))
            {
                var translation = Translation<TOwner>.Create(entity.Id, normalizedCulture, fields);
                await TranslationRepo.AddAsync(translation);
            }
        }

        if (!processedCultures.Contains(defaultCulture))
        {
            var baseFields = GetTranslatableFields(entity);
            var defaultTranslation = Translation<TOwner>.Create(entity.Id, defaultCulture, baseFields);
            await TranslationRepo.AddAsync(defaultTranslation);
        }

        await Uow.SaveChangesAsync(cancellationToken);
        return Result.Success(entity.Id);
    }
}