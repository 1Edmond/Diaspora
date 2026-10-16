using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record ActivateProfileCommand(
    Guid ProfileId,
    VerificationLevel? ApprovedLevel = null
) : ICommand<Result>;