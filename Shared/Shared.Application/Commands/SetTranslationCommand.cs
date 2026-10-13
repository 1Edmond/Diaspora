using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Shared.Application.Commands;

public record SetTranslationCommand<TOwner>(
    Guid EntityId,
    string CultureCode,
    Dictionary<string, string> Fields
) : ICommand<Result> where TOwner : class, Shared.Domain.Interfaces.ITranslatable;