using Profile.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record ChangeProfileKindCommand(
    Guid ProfileId,
    Guid UserId,
    ProfileKind NewKind
) : ICommand<Result>;


public sealed record ChangeKindRequest(ProfileKind NewKind);
