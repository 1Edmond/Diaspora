using Freelance.Domain.Enums;
using System;

namespace Freelance.Application.Dtos;

public record JobReviewDto(
    Guid Id,
    Guid JobApplicationId,
    Guid RaterId,
    string RaterName,
    Guid SubjectId,
    string SubjectName,
    RaterRole RaterRole,
    int Rating,
    int Punctuality,
    int Quality,
    int Communication,
    string? Comment,
    DateTime CreatedAt
);