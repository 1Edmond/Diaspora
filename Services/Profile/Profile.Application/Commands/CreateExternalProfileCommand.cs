using Profile.Application.Responses.Commands;
using Profile.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record CreateExternalProfileCommand(
    Guid UserId,
    Guid ExternalProfileTypeId,
    ExternalProfileKind Kind
) : ICommand<Result<CreateExternalProfileResponse>>;

public sealed record CreateExternalProfileRequest(
    Guid ExternalProfileTypeId,
    ExternalProfileKind Kind
);
