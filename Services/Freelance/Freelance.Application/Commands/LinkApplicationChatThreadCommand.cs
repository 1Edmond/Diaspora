using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record LinkApplicationChatThreadCommand(
    Guid JobApplicationId,
    Guid RequestedBy,
    Guid ChatThreadId
) : ICommand<Result>;