using Shared.Application.Abstractions;
using Shared.Application.Dtos;
using Shared.Application.Queries;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Domain.Translations;
using System;
using System.Collections.Generic;

namespace Shared.Application.Handlers.Queries;

public class GetTranslationsQueryHandler<TOwner>(
    ITranslationRepository<TOwner> translationRepo)
    where TOwner : AggregateRoot, Shared.Domain.Interfaces.ITranslatable
{
    public async Task<Result<IReadOnlyCollection<TranslationDto>>> Handle(GetTranslationsQuery<TOwner> query, CancellationToken cancellationToken)
    {
        var translations = await translationRepo.GetByOwnerIdAsync(query.EntityId);
        var dtos = translations.Select(t => new TranslationDto(
            t.CultureCode,
            t.Fields,
            t.CreatedAt,
            t.UpdatedAt
        )).ToList();
        return Result.Success((IReadOnlyCollection<TranslationDto>)dtos);
    }
}