using Profile.Application.Responses.Commands;
using Profile.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record CreateProfileCommand(
    Guid UserId,
    Guid ProfileTypeId,
    PhoneNumber PhoneNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileKind Kind
) : ICommand<Result<CreateProfileResponse>>;

public sealed record CreateProfileRequest(
    Guid ProfileTypeId,
    ProfileKind Kind
);
