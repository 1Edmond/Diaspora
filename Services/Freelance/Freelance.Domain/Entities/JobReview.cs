using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;

namespace Freelance.Domain.Entities;

public class JobReview : AggregateRoot
{
    public Guid JobApplicationId { get; private set; }
    public Guid RaterId { get; private set; }
    public Guid SubjectId { get; private set; }
    public RaterRole RaterRole { get; private set; }

    public int Rating { get; private set; }        // 1-5 global
    public int Punctuality { get; private set; }   // 1-5
    public int Quality { get; private set; }       // 1-5
    public int Communication { get; private set; } // 1-5

    public string? Comment { get; private set; }

    private JobReview() { }

    public static Result<JobReview> Create(
        Guid jobApplicationId, Guid raterId, Guid subjectId, RaterRole raterRole,
        int rating, int punctuality, int quality, int communication, string? comment)
    {
        if (rating < 1 || rating > 5)
            return Result.Failure<JobReview>("Rating must be between 1 and 5.");
        if (punctuality < 1 || punctuality > 5)
            return Result.Failure<JobReview>("Punctuality must be between 1 and 5.");
        if (quality < 1 || quality > 5)
            return Result.Failure<JobReview>("Quality must be between 1 and 5.");
        if (communication < 1 || communication > 5)
            return Result.Failure<JobReview>("Communication must be between 1 and 5.");

        return Result.Success(new JobReview
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            RaterId = raterId,
            SubjectId = subjectId,
            RaterRole = raterRole,
            Rating = rating,
            Punctuality = punctuality,
            Quality = quality,
            Communication = communication,
            Comment = comment
        });
    }
}