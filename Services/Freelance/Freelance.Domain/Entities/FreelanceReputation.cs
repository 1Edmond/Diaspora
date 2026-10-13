using Freelance.Domain.Enums;
using Shared.Domain.Primitives;
using System;

namespace Freelance.Domain.Entities;

/// <summary>
/// Projection maintenue à chaque nouvel avis (JobReview). Une ligne par (SubjectId, Role) :
/// un même utilisateur peut avoir une réputation Worker et une réputation Employer distinctes.
/// </summary>
public class FreelanceReputation : AggregateRoot
{
    public Guid SubjectId { get; private set; }
    public ReputationRole Role { get; private set; }

    public double AverageRating { get; private set; }
    public double AveragePunctuality { get; private set; }
    public double AverageQuality { get; private set; }
    public double AverageCommunication { get; private set; }
    public int TotalRatings { get; private set; }
    public int TotalJobsCompleted { get; private set; }

    private FreelanceReputation() { }

    public static FreelanceReputation CreateEmpty(Guid subjectId, ReputationRole role)
    {
        return new FreelanceReputation
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            Role = role,
            AverageRating = 0,
            AveragePunctuality = 0,
            AverageQuality = 0,
            AverageCommunication = 0,
            TotalRatings = 0,
            TotalJobsCompleted = 0
        };
    }

    public void ApplyNewReview(double rating, int punctuality, int quality, int communication)
    {
        var newTotal = TotalRatings + 1;

        AverageRating = ((AverageRating * TotalRatings) + rating) / newTotal;
        AveragePunctuality = ((AveragePunctuality * TotalRatings) + punctuality) / newTotal;
        AverageQuality = ((AverageQuality * TotalRatings) + quality) / newTotal;
        AverageCommunication = ((AverageCommunication * TotalRatings) + communication) / newTotal;

        TotalRatings = newTotal;
    }

    public void IncrementJobsCompleted() => TotalJobsCompleted++;
}