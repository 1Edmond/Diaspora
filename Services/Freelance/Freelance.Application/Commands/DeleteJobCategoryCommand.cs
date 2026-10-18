using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record DeleteJobCategoryCommand(Guid Id) : ICommand<Result>;
