using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Shared.Application.Commands;

public record UpdateWithTranslationsCommand<TOwner, TEntityDto>(
    Guid EntityId,
    TEntityDto EntityDto,
    Dictionary<string, Dictionary<string, string>> Translations,
    HashSet<string>? TranslationsToRemove = null
) : ICommand<Result> where TOwner : class, Shared.Domain.Interfaces.ITranslatable;