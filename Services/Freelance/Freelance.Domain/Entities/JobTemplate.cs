using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;

namespace Freelance.Domain.Entities;

public class JobTemplate : AggregateRoot
{
    public Guid EmployerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }
    public int Capacity { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentType PaymentType { get; private set; }
    public PaymentTiming PaymentTiming { get; private set; }

    public CheckInMethod CheckInMethod { get; private set; }
    public int? GeofenceRadiusMeters { get; private set; }
    public bool RequiresKycVerification { get; private set; }

    private readonly List<string> _requiredSkills = new();
    public IReadOnlyCollection<string> RequiredSkills => _requiredSkills.AsReadOnly();

    private readonly List<string> _requiredDocuments = new();
    public IReadOnlyCollection<string> RequiredDocuments => _requiredDocuments.AsReadOnly();

    public JobCategory Category { get; private set; } = null!;

    private JobTemplate() { }

    public static Result<JobTemplate> Create(
        Guid employerId, string name, string description, Guid categoryId,
        int capacity, decimal amount, string currency, PaymentType paymentType, PaymentTiming paymentTiming,
        CheckInMethod checkInMethod, int? geofenceRadiusMeters, bool requiresKycVerification,
        IEnumerable<string> requiredSkills, IEnumerable<string> requiredDocuments)
    {
        if (capacity <= 0)
            return Result.Failure<JobTemplate>("Capacity must be positive.");
        if (amount < 0)
            return Result.Failure<JobTemplate>("Amount cannot be negative.");
        if (checkInMethod == CheckInMethod.GeoLocation && geofenceRadiusMeters is null or <= 0)
            return Result.Failure<JobTemplate>("GeofenceRadiusMeters is required when CheckInMethod is GeoLocation.");

        var template = new JobTemplate
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            Name = name,
            Description = description,
            CategoryId = categoryId,
            Capacity = capacity,
            Amount = amount,
            Currency = currency,
            PaymentType = paymentType,
            PaymentTiming = paymentTiming,
            CheckInMethod = checkInMethod,
            GeofenceRadiusMeters = geofenceRadiusMeters,
            RequiresKycVerification = requiresKycVerification
        };

        template._requiredSkills.AddRange(requiredSkills.Distinct());
        template._requiredDocuments.AddRange(requiredDocuments.Distinct());

        return Result.Success(template);
    }

    public Result Update(
        string name, string description, Guid categoryId,
        int capacity, decimal amount, string currency, PaymentType paymentType, PaymentTiming paymentTiming,
        CheckInMethod checkInMethod, int? geofenceRadiusMeters, bool requiresKycVerification,
        IEnumerable<string> requiredSkills, IEnumerable<string> requiredDocuments)
    {
        if (capacity <= 0)
            return Result.Failure("Capacity must be positive.");
        if (amount < 0)
            return Result.Failure("Amount cannot be negative.");
        if (checkInMethod == CheckInMethod.GeoLocation && geofenceRadiusMeters is null or <= 0)
            return Result.Failure("GeofenceRadiusMeters is required when CheckInMethod is GeoLocation.");

        Name = name;
        Description = description;
        CategoryId = categoryId;
        Capacity = capacity;
        Amount = amount;
        Currency = currency;
        PaymentType = paymentType;
        PaymentTiming = paymentTiming;
        CheckInMethod = checkInMethod;
        GeofenceRadiusMeters = geofenceRadiusMeters;
        RequiresKycVerification = requiresKycVerification;

        _requiredSkills.Clear();
        _requiredSkills.AddRange(requiredSkills.Distinct());
        _requiredDocuments.Clear();
        _requiredDocuments.AddRange(requiredDocuments.Distinct());

        return Result.Success();
    }
}