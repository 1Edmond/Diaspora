using KYC.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Queries;

public record GetPendingKYCVerificationsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : PagedQuery<Result<PagedList<KYCVerificationDto>>>(PageNumber, PageSize);

