using Profile.Application.Responses.Commands;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record CreateInternalProfileCommand(
    Guid UserId,
    Guid InternalProfileTypeId,
    string PhoneNumber,
    string FirstName,
    string LastName,
    DateTime DateOfBirth
) : ICommand<Result<CreateInternalProfileResponse>>;