using IdentityService.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Queries;

public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    UserStatus? Status = null,
    bool? EmailVerified = null,
    bool IncludeDeleted = false
) : PagedQuery<Result<PagedList<UserDto>>>(PageNumber, PageSize);

