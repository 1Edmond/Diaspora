using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record AcceptApplicationCommand(Guid JobApplicationId, Guid DecidedBy, Guid? EscrowTransactionId) : ICommand<Result>;