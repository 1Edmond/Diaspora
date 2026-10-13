using Freelance.Application.Dtos;
using Freelance.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetJobPostingsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? SearchTerm = null,
    PaymentType? PaymentType = null,
    PaymentTiming? PaymentTiming = null,
    JobPostingStatus? Status = null,
    DateTime? EventStartFrom = null,
    DateTime? EventStartTo = null,
    string? City = null,
    string? Country = null,
    double? UserLatitude = null,
    double? UserLongitude = null,
    double? MaxDistanceKm = null,
    bool? IsRemote = null,
    IEnumerable<string>? RequiredSkills = null,
    bool ActiveOnly = true
) : PagedQuery<Result<PagedList<JobPostingSummaryDto>>>(PageNumber, PageSize);