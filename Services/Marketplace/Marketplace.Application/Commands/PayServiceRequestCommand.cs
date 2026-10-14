using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Commands;

public sealed record PayServiceRequestCommand(
    Guid Id,
    Guid CurrentUserId
) : ICommand<Result>;
