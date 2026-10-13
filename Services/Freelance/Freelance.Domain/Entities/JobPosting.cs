using Freelance.Domain.Enums;
using Freelance.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freelance.Domain.Entities;

public class JobPosting : AggregateRoot, ISoftDeletable
{
    public Guid EmployerId { get; private set; }
    public Guid? TemplateId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public int Capacity { get; private set; }
    public int AcceptedCount { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentType PaymentType { get; private set; }
    public PaymentTiming PaymentTiming { get; private set; }

    public CheckInMethod CheckInMethod { get; private set; }
    public int? GeofenceRadiusMeters { get; private set; }
    public bool RequiresKycVerification { get; private set; }

    public DateTime EventStartAt { get; private set; }
    public DateTime EventEndAt { get; private set; }

    public string? City { get; private set; }
    public string? Country { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public bool IsRemote { get; private set; }

    private readonly List<string> _requiredSkills = new();
    public IReadOnlyCollection<string> RequiredSkills => _requiredSkills.AsReadOnly();

    private readonly List<string> _requiredDocuments = new();
    public IReadOnlyCollection<string> RequiredDocuments => _requiredDocuments.AsReadOnly();

    public DateTime? RegistrationDeadline { get; private set; }
    public JobPostingStatus Status { get; private set; }

    public JobCategory Category { get; private set; } = null!;

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    private JobPosting() { }

    public static Result<JobPosting> Create(
        Guid employerId, Guid? templateId, Guid categoryId, string title, string description,
        int capacity, decimal amount, string currency, PaymentType paymentType, PaymentTiming paymentTiming,
        CheckInMethod checkInMethod, int? geofenceRadiusMeters, bool requiresKycVerification,
        DateTime eventStartAt, DateTime eventEndAt,
        string? city, string? country, double? latitude, double? longitude, bool isRemote,
        IEnumerable<string> requiredSkills, IEnumerable<string> requiredDocuments,
        DateTime? registrationDeadline)
    {
        if (capacity <= 0)
            return Result.Failure<JobPosting>("Capacity must be positive.");
        if (amount < 0)
            return Result.Failure<JobPosting>("Amount cannot be negative.");
        if (eventEndAt <= eventStartAt)
            return Result.Failure<JobPosting>("EventEndAt must be after EventStartAt.");
        if (checkInMethod == CheckInMethod.GeoLocation && geofenceRadiusMeters is null or <= 0)
            return Result.Failure<JobPosting>("GeofenceRadiusMeters is required when CheckInMethod is GeoLocation.");

        var posting = new JobPosting
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            TemplateId = templateId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
            Capacity = capacity,
            AcceptedCount = 0,
            Amount = amount,
            Currency = currency,
            PaymentType = paymentType,
            PaymentTiming = paymentTiming,
            CheckInMethod = checkInMethod,
            GeofenceRadiusMeters = geofenceRadiusMeters,
            RequiresKycVerification = requiresKycVerification,
            EventStartAt = eventStartAt,
            EventEndAt = eventEndAt,
            City = city,
            Country = country,
            Latitude = latitude,
            Longitude = longitude,
            IsRemote = isRemote,
            RegistrationDeadline = registrationDeadline,
            Status = JobPostingStatus.Draft,
            IsDeleted = false
        };

        posting._requiredSkills.AddRange(requiredSkills.Distinct());
        posting._requiredDocuments.AddRange(requiredDocuments.Distinct());

        return Result.Success(posting);
    }

    public Result Update(
        string title, string description, Guid categoryId,
        int capacity, decimal amount, string currency, PaymentType paymentType, PaymentTiming paymentTiming,
        CheckInMethod checkInMethod, int? geofenceRadiusMeters, bool requiresKycVerification,
        DateTime eventStartAt, DateTime eventEndAt,
        string? city, string? country, double? latitude, double? longitude, bool isRemote,
        IEnumerable<string> requiredSkills, IEnumerable<string> requiredDocuments,
        DateTime? registrationDeadline)
    {
        if (Status != JobPostingStatus.Draft)
            return Result.Failure("Only a Draft job posting can be fully updated.");

        if (capacity <= 0)
            return Result.Failure("Capacity must be positive.");
        if (amount < 0)
            return Result.Failure("Amount cannot be negative.");
        if (eventEndAt <= eventStartAt)
            return Result.Failure("EventEndAt must be after EventStartAt.");
        if (checkInMethod == CheckInMethod.GeoLocation && geofenceRadiusMeters is null or <= 0)
            return Result.Failure("GeofenceRadiusMeters is required when CheckInMethod is GeoLocation.");

        Title = title;
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
        EventStartAt = eventStartAt;
        EventEndAt = eventEndAt;
        City = city;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
        IsRemote = isRemote;
        RegistrationDeadline = registrationDeadline;

        _requiredSkills.Clear();
        _requiredSkills.AddRange(requiredSkills.Distinct());
        _requiredDocuments.Clear();
        _requiredDocuments.AddRange(requiredDocuments.Distinct());

        return Result.Success();
    }

    public Result Publish()
    {
        if (Status != JobPostingStatus.Draft)
            return Result.Failure("Only a Draft job posting can be published.");
        if (EventStartAt <= DateTime.UtcNow)
            return Result.Failure("EventStartAt must be in the future to publish.");

        Status = JobPostingStatus.Open;

        AddDomainEvent(new JobPostingPublishedEvent(
            Id, EmployerId, CategoryId, Title, City, Country, _requiredSkills.ToArray()));

        return Result.Success();
    }

    public Result CloseRegistration()
    {
        if (Status != JobPostingStatus.Open)
            return Result.Failure("Only an Open job posting can have its registration closed.");

        Status = JobPostingStatus.RegistrationClosed;
        return Result.Success();
    }

    public Result Start()
    {
        if (Status is not (JobPostingStatus.Open or JobPostingStatus.RegistrationClosed))
            return Result.Failure("Job posting must be Open or RegistrationClosed to start.");

        Status = JobPostingStatus.InProgress;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != JobPostingStatus.InProgress)
            return Result.Failure("Only an InProgress job posting can be completed.");

        Status = JobPostingStatus.Completed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is JobPostingStatus.Completed or JobPostingStatus.Cancelled)
            return Result.Failure("This job posting can no longer be cancelled.");

        Status = JobPostingStatus.Cancelled;
        return Result.Success();
    }

    public Result NotifyApplicationAccepted()
    {
        if (AcceptedCount >= Capacity)
            return Result.Failure("Capacity already reached.");

        AcceptedCount++;

        if (AcceptedCount >= Capacity && Status == JobPostingStatus.Open)
            Status = JobPostingStatus.RegistrationClosed;

        return Result.Success();
    }

    public void NotifyAcceptedApplicationReversed()
    {
        if (AcceptedCount > 0)
            AcceptedCount--;

        if (Status == JobPostingStatus.RegistrationClosed && AcceptedCount < Capacity)
            Status = JobPostingStatus.Open;
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Delete(string deletedBy) => SoftDelete(deletedBy);

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    public void NotifyMatchedWorker(Guid workerId)
    {
        AddDomainEvent(new JobMatchFoundEvent(Id, workerId, EmployerId, CategoryId, Title));
    }
}