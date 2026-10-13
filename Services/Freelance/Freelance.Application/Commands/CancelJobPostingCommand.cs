using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record CancelJobPostingCommand(Guid Id, Guid RequestedBy) : ICommand<Result>;