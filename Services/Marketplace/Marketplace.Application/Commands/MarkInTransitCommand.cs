using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Commands;

public record MarkInTransitCommand(
    Guid RequestId,
    Guid RequestedBy
) : ICommand<Result>;
