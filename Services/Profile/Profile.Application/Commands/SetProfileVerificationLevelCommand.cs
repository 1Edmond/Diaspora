using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;

namespace Profile.Application.Commands;

public sealed record SetProfileVerificationLevelCommand(
    Guid ProfileId,
    VerificationLevel Level,
    string AdminId
) : ICommand<Result>;
