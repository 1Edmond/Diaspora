using Freelance.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetJobTemplatesQuery(
    Guid EmployerId,
    int PageNumber = 1,
    int PageSize = 10
) : PagedQuery<Result<PagedList<JobTemplateDto>>>(PageNumber, PageSize);