using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;

namespace Freelance.Application.Commands;

public record UpdateJobCategoryCommand(
    Guid Id,
    string Name,
    string? Description = null,
    bool IsActive = true,
    int DisplayOrder = 0,
    List<Guid>? AllowedProfileTypeIds = null
) : ICommand<Result>;
