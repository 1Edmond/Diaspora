using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using System;

namespace Marketplace.Domain.Entities;

public class CategoryPolicy : Entity
{
    public Guid CategoryId { get; private set; }
    public VerificationLevel MinPublishLevel { get; private set; } = VerificationLevel.L0_Registered;
    public VerificationLevel MinBookLevel { get; private set; } = VerificationLevel.L0_Registered;
    public decimal? MaxWeightKg { get; private set; }
    public decimal? MaxValueAmount { get; private set; }
    public bool RequiresEscrow { get; private set; } = true;
    public bool RequiresPreModeration { get; private set; } = false;
    public decimal DefaultCommissionRate { get; private set; } = 0.10m; // 10%

    public Category Category { get; private set; } = null!;

    private CategoryPolicy() { }

    public static CategoryPolicy Create(
        Guid categoryId,
        VerificationLevel minPublishLevel = VerificationLevel.L0_Registered,
        VerificationLevel minBookLevel = VerificationLevel.L0_Registered,
        decimal? maxWeightKg = null,
        decimal? maxValueAmount = null,
        bool requiresEscrow = true,
        bool requiresPreModeration = false,
        decimal defaultCommissionRate = 0.10m)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required.", nameof(categoryId));
        if (defaultCommissionRate < 0 || defaultCommissionRate > 1)
            throw new ArgumentOutOfRangeException(nameof(defaultCommissionRate), "Commission rate must be between 0 and 1.");

        return new CategoryPolicy
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            MinPublishLevel = minPublishLevel,
            MinBookLevel = minBookLevel,
            MaxWeightKg = maxWeightKg,
            MaxValueAmount = maxValueAmount,
            RequiresEscrow = requiresEscrow,
            RequiresPreModeration = requiresPreModeration,
            DefaultCommissionRate = defaultCommissionRate
        };
    }

    public void UpdatePolicy(
        VerificationLevel minPublishLevel,
        VerificationLevel minBookLevel,
        decimal? maxWeightKg,
        decimal? maxValueAmount,
        bool requiresEscrow,
        bool requiresPreModeration,
        decimal defaultCommissionRate)
    {
        if (defaultCommissionRate < 0 || defaultCommissionRate > 1)
            throw new ArgumentOutOfRangeException(nameof(defaultCommissionRate), "Commission rate must be between 0 and 1.");

        MinPublishLevel = minPublishLevel;
        MinBookLevel = minBookLevel;
        MaxWeightKg = maxWeightKg;
        MaxValueAmount = maxValueAmount;
        RequiresEscrow = requiresEscrow;
        RequiresPreModeration = requiresPreModeration;
        DefaultCommissionRate = defaultCommissionRate;
    }
}
