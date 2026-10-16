using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Commands;

public record SubmitKYCVerificationCommand(
    Guid VerificationId,
    KYCLevel? Level = null
) : ICommand<Result>;
