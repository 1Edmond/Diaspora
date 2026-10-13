using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record ApplyToJobCommand(Guid JobPostingId, Guid WorkerId, string? Message) : ICommand<Result<Guid>>;