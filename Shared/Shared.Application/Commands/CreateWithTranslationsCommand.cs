using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Shared.Application.Commands;

public record CreateWithTranslationsCommand<TOwner, TEntityDto>(
    TEntityDto EntityDto,
    Dictionary<string, Dictionary<string, string>> Translations
) : ICommand<Result<Guid>> where TOwner : class, ITranslatable;