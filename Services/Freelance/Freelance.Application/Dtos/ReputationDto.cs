using Freelance.Domain.Enums;
using System;

namespace Freelance.Application.Dtos;

public record ReputationDto(
    Guid SubjectId,
    ReputationRole Role,
    double AverageRating,
    double AveragePunctuality,
    double AverageQuality,
    double AverageCommunication,
    int TotalRatings,
    int TotalJobsCompleted
);