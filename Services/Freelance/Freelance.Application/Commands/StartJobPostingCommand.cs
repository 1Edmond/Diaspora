using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record StartJobPostingCommand(Guid Id, Guid RequestedBy) : ICommand<Result>;