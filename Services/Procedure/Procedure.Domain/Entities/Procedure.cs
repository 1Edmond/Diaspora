using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class Procedure : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Cost { get; private set; } = null!;
    public ProcedureProfileType ProfileType { get; private set; }

    /// <summary>
    /// InternalProfileType.Id si ProfileType = Internal
    /// ExternalProfileType.Id si ProfileType = External
    /// </summary>
    public Guid ProfileTypeId { get; private set; }

    public int EstimatedDurationDays { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? LocationId { get; private set; }

    // Navigation EF Core
    public Location? Location { get; private set; }
    private readonly List<ProcedureDependency> _dependencies = [];
    public IReadOnlyList<ProcedureDependency> Dependencies => _dependencies.AsReadOnly();

    private readonly List<ProcedureRequiredDocument> _requiredDocuments = [];
    public IReadOnlyList<ProcedureRequiredDocument> RequiredDocuments => _requiredDocuments.AsReadOnly();

    private Procedure() { }

    private Procedure(
        Guid id,
        string title,
        string description,
        Money cost,
        ProcedureProfileType profileType,
        Guid profileTypeId,
        int estimatedDurationDays,
        Guid? locationId) : base(id)
    {
        Title = title;
        Description = description;
        Cost = cost;
        ProfileType = profileType;
        ProfileTypeId = profileTypeId;
        EstimatedDurationDays = estimatedDurationDays;
        LocationId = locationId;
        IsActive = true;
    }

    public static Result<Procedure> Create(
        string title,
        string description,
        Money cost,
        ProcedureProfileType profileType,
        Guid profileTypeId,
        int estimatedDurationDays,
        Guid? locationId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Procedure>("Procedure title is required.");

        if (profileTypeId == Guid.Empty)
            return Result.Failure<Procedure>("ProfileTypeId is required.");

        if (estimatedDurationDays < 0)
            return Result.Failure<Procedure>("Estimated duration cannot be negative.");

        return Result.Success(new Procedure(
            Guid.NewGuid(), title, description, cost,
            profileType, profileTypeId, estimatedDurationDays, locationId));
    }

    public void Update(
        string title,
        string description,
        Money cost,
        int estimatedDurationDays,
        Guid? locationId)
    {
        Title = title;
        Description = description;
        Cost = cost;
        EstimatedDurationDays = estimatedDurationDays;
        LocationId = locationId;
    }

    public Result AddDependency(Guid dependsOnProcedureId)
    {
        if (dependsOnProcedureId == Id)
            return Result.Failure("A procedure cannot depend on itself.");

        if (_dependencies.Any(d => d.DependsOnProcedureId == dependsOnProcedureId))
            return Result.Failure("This dependency already exists.");

        _dependencies.Add(new ProcedureDependency(Id, dependsOnProcedureId));
        return Result.Success();
    }

    public void RemoveDependency(Guid dependsOnProcedureId)
        => _dependencies.RemoveAll(d => d.DependsOnProcedureId == dependsOnProcedureId);

    public Result AddRequiredDocument(Guid documentTypeId)
    {
        if (_requiredDocuments.Any(d => d.DocumentTypeId == documentTypeId))
            return Result.Failure("This document type is already required.");

        _requiredDocuments.Add(new ProcedureRequiredDocument(Id, documentTypeId));
        return Result.Success();
    }

    public void RemoveRequiredDocument(Guid documentTypeId)
        => _requiredDocuments.RemoveAll(d => d.DocumentTypeId == documentTypeId);

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
