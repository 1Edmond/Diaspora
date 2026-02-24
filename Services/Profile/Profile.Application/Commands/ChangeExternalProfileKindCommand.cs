using Profile.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record ChangeExternalProfileKindCommand(
    Guid ExternalProfileId,
    Guid UserId,
    ExternalProfileKind NewKind
) : ICommand<Result>;


public sealed record ChangeKindRequest(ExternalProfileKind NewKind);
