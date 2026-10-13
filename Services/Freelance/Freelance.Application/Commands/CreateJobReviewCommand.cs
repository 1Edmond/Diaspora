using Freelance.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record CreateJobReviewCommand(
    Guid JobApplicationId,
    Guid RaterId,
    Guid SubjectId,
    RaterRole RaterRole,
    int Rating,
    int Punctuality,
    int Quality,
    int Communication,
    string? Comment
) : ICommand<Result<Guid>>;