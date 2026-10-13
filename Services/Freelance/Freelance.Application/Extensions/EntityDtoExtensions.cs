using Freelance.Application.Dtos;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freelance.Application.Extensions;

public static class EntityDtoExtensions
{
    public static JobCategoryDto MapToDto(this JobCategory c) => new(
        c.Id, c.Name, c.Description, c.IsActive, c.DisplayOrder
    );

    public static JobTemplateDto MapToDto(this JobTemplate t) => new(
        t.Id, t.EmployerId, t.Name, t.Description, t.CategoryId,
        t.Category?.Name ?? string.Empty, t.Capacity, t.Amount, t.Currency,
        t.PaymentType, t.PaymentTiming, t.CheckInMethod, t.GeofenceRadiusMeters,
        t.RequiresKycVerification, t.RequiredSkills, t.RequiredDocuments, t.CreatedAt
    );

    public static JobPostingDto MapToDto(this JobPosting p) => new(
        p.Id, p.EmployerId, string.Empty, p.TemplateId, p.CategoryId, p.Category?.Name ?? string.Empty,
        p.Title, p.Description, p.Capacity, p.AcceptedCount, p.Amount, p.Currency,
        p.PaymentType, p.PaymentTiming, p.CheckInMethod, p.GeofenceRadiusMeters,
        p.RequiresKycVerification, p.EventStartAt, p.EventEndAt, p.City, p.Country,
        p.Latitude, p.Longitude, p.IsRemote, p.RequiredSkills, p.RequiredDocuments,
        p.RegistrationDeadline, p.Status, p.CreatedAt
    );

    public static JobPostingSummaryDto MapToSummaryDto(this JobPosting p, double? distanceKm = null) => new(
        p.Id, p.EmployerId, string.Empty, p.CategoryId, p.Category?.Name ?? string.Empty,
        p.Title, p.Amount, p.Currency, p.PaymentType, p.PaymentTiming,
        p.EventStartAt, p.EventEndAt, p.City, p.Country, distanceKm,
        p.Capacity, p.AcceptedCount, p.Status, p.CreatedAt
    );

    public static JobApplicationDto MapToDto(this JobApplication a) => new(
        a.Id, a.JobPostingId, a.Posting?.Title ?? string.Empty, a.WorkerId, string.Empty,
        a.Posting?.EmployerId ?? Guid.Empty, string.Empty, a.Message, a.Status,
        a.DecidedAt, a.DecidedBy, a.RejectionReason, a.ChatThreadId, a.EscrowTransactionId, a.CreatedAt
    );

    public static JobCheckInDto MapToDto(this JobCheckIn c) => new(
        c.Id, c.JobApplicationId, string.Empty, c.JobApplication?.WorkerId ?? Guid.Empty, string.Empty,
        c.Method, c.Status, c.CheckedInAt, c.CheckedOutAt, c.Latitude, c.Longitude, c.Notes, c.CreatedAt
    );

    public static JobReviewDto MapToDto(this JobReview r) => new(
        r.Id, r.JobApplicationId, r.RaterId, string.Empty, r.SubjectId, string.Empty,
        r.RaterRole, r.Rating, r.Punctuality, r.Quality, r.Communication, r.Comment, r.CreatedAt
    );

    public static ReputationDto MapToDto(this FreelanceReputation r) => new(
        r.SubjectId, r.Role, r.AverageRating, r.AveragePunctuality, r.AverageQuality,
        r.AverageCommunication, r.TotalRatings, r.TotalJobsCompleted
    );

    public static WorkerJobPreferenceDto MapToDto(this WorkerJobPreference p) => new(
        p.Id, p.WorkerId, p.PreferredCategoryIds, p.PreferredCity, p.PreferredCountry,
        p.PreferredLatitude, p.PreferredLongitude, p.PreferredRadiusKm, p.OnlyRemote,
        p.EmailNotificationsEnabled, p.CreatedAt, p.UpdatedAt
    );
}