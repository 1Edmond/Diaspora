using IdentityService.Application.Dtos;
using Shared.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Queries;

public record GetUserQuery(Guid UserId) : IQuery<UserDto>;