using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl,
    int DisplayOrder
) : ICommand<Result>;
