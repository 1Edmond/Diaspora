using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Commands;

public record ConfirmHandoverCommand(
    Guid RequestId,
    Guid RequestedBy,
    string Otp,
    string? ProofPhotoUrl = null
) : ICommand<Result>;
