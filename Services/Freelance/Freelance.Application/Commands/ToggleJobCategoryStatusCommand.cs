using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record ToggleJobCategoryStatusCommand(Guid Id, bool IsActive) : ICommand<Result>;
