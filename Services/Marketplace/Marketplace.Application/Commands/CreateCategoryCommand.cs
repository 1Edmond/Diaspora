using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record CreateCategoryCommand(
    string Name,
    string? Description,
    string? IconUrl,
    int DisplayOrder,
    Dictionary<string, Dictionary<string, string>>? Translations = null,
    List<Guid>? AllowedProfileTypeIds = null
) : ICommand<Result<Guid>>;