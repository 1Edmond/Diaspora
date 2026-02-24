using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record ActivateExternalProfileCommand(
    Guid ExternalProfileId,
    Guid KycVerificationId,
    string CountryCode
) : ICommand<Result>;