using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record RejectApplicationCommand(Guid JobApplicationId, Guid DecidedBy, string? Reason) : ICommand<Result>;