using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record CompleteApplicationCommand(Guid JobApplicationId, Guid WorkerId) : ICommand<Result>;