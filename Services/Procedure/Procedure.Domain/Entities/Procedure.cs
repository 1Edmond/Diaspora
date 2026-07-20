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
    public Guid ProfileTypeId { get; private set; }
    public int EstimatedDurationDays { get; private set; }
    public bool IsActive { get; private set; }

    // N-à-N vers Location
    private readonly List<ProcedureLocation> _locations = new();
    public IReadOnlyCollection<ProcedureLocation> Locations => _locations.AsReadOnly();

    private readonly List<ProcedureDependency> _dependencies = new();
    public IReadOnlyCollection<ProcedureDependency> Dependencies => _dependencies.AsReadOnly();

    private readonly List<ProcedureRequiredDocument> _requiredDocuments = new();
    public IReadOnlyCollection<ProcedureRequiredDocument> RequiredDocuments => _requiredDocuments.AsReadOnly();

    private Procedure() { }

    private Procedure(
        Guid id,
        string title,
        string description,
        Money cost,
        ProcedureProfileType profileType,
        Guid profileTypeId,
        int estimatedDurationDays) : base(id)
    {
        Title = title;
        Description = description;
        Cost = cost;
        ProfileType = profileType;
        ProfileTypeId = profileTypeId;
        EstimatedDurationDays = estimatedDurationDays;
        IsActive = true;
    }

    public static Result<Procedure> Create(
        string title,
        string description,
        Money cost,
        ProcedureProfileType profileType,
        Guid profileTypeId,
        int estimatedDurationDays)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Procedure>("Title is required.");

        if (estimatedDurationDays < 0)
            return Result.Failure<Procedure>("Estimated duration cannot be negative.");

        return Result.Success(new Procedure(
            Guid.NewGuid(), title, description, cost,
            profileType, profileTypeId, estimatedDurationDays));
    }

    public void Update(
        string title,
        string description,
        Money cost,
        int estimatedDurationDays)
    {
        Title = title;
        Description = description;
        Cost = cost;
        EstimatedDurationDays = estimatedDurationDays;
    }

    // ── Locations ──────────────────────────────────────────────────────────────

    public Result AddLocation(Guid locationId)
    {
        if (_locations.Any(l => l.LocationId == locationId))
            return Result.Failure("This location is already associated with the procedure.");

        _locations.Add(new ProcedureLocation(Id, locationId));
        return Result.Success();
    }

    public void RemoveLocation(Guid locationId)
        => _locations.RemoveAll(l => l.LocationId == locationId);

    public void ClearLocations()
        => _locations.Clear();

    // ── Dependencies ───────────────────────────────────────────────────────────

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

    public void ClearDependencies()
        => _dependencies.Clear();

   
    public void AddRequiredDocument(Guid documentTypeId)
    {
        if (!_requiredDocuments.Any(d => d.DocumentTypeId == documentTypeId))
            _requiredDocuments.Add(new ProcedureRequiredDocument(Id, documentTypeId));
    }

    public void ClearRequiredDocuments()
        => _requiredDocuments.Clear();

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}