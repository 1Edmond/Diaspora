using Profile.Application.Responses.Commands;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record CreateInternalProfileCommand(
    Guid UserId,
    Guid InternalProfileTypeId,
    PhoneNumber PhoneNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth
) : ICommand<Result<CreateInternalProfileResponse>>;