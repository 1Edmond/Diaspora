using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Commands;

public record RequestMoreInfoCommand(
    Guid VerificationId,
    Guid ReviewerId,
    string Message
) : ICommand<Result>;
