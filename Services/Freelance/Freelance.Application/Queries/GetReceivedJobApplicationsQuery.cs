using Freelance.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetReceivedJobApplicationsQuery(
    Guid EmployerId,
    int PageNumber = 1,
    int PageSize = 10,
    bool IsAdmin = false
) : PagedQuery<Result<PagedList<JobApplicationDto>>>(PageNumber, PageSize);