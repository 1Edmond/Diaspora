using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record MarkApplicationNoShowCommand(Guid JobApplicationId, Guid DecidedBy) : ICommand<Result>;