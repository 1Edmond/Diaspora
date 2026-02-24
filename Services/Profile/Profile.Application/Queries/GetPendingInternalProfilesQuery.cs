using Profile.Application.DTOs;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Queries;

public sealed record GetPendingInternalProfilesQuery(
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<InternalProfileDto>>>(PageNumber, PageSize);

