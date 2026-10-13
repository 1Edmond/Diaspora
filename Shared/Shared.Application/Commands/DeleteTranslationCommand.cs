using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;

namespace Shared.Application.Commands;

public record DeleteTranslationCommand<TOwner>(
    Guid EntityId,
    string CultureCode
) : ICommand<Result> where TOwner : class, Shared.Domain.Interfaces.ITranslatable;