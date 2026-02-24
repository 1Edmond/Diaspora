using KYC.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Queries;

public record GetKYCVerificationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    KYCStatus? Status = null,
    KYCLevel? Level = null,
    string? SearchTerm = null,
    DateTime? SubmittedFrom = null,
    DateTime? SubmittedTo = null
) : PagedQuery<Result<PagedList<KYCVerificationDto>>>(PageNumber, PageSize);
