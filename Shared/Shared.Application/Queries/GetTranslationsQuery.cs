using Shared.Application.Abstractions;
using Shared.Application.Dtos;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Shared.Application.Queries;

public record GetTranslationsQuery<TOwner>(
    Guid EntityId
) : IQuery<Result<IReadOnlyCollection<TranslationDto>>> where TOwner : class, Shared.Domain.Interfaces.ITranslatable;