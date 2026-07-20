using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Domain.Entities;

public sealed class DocumentType : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public Guid ProfileTypeId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsRenewable { get; private set; }
    public int RenewalAlertDays { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; }

    private DocumentType() { }

    private DocumentType(
        Guid id,
        Guid profileTypeId,
        string name,
        string code,
        string? description,
        bool isRenewable,
        int renewalAlertDays,
        bool isRequired) : base(id)
    {
        Name = name;
        Code = code;
        ProfileTypeId = profileTypeId;
        Description = description;
        IsRenewable = isRenewable;
        RenewalAlertDays = renewalAlertDays;
        IsRequired = isRequired;
        IsActive = true;
    }

    public static Result<DocumentType> Create(
        Guid profileTypeId,
        string name,
        string code,
        string? description,
        bool isRenewable,
        int renewalAlertDays,
        bool isRequired)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<DocumentType>("Document type name is required.");

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<DocumentType>("Document type code is required.");

        if (isRenewable && renewalAlertDays <= 0)
            return Result.Failure<DocumentType>(
                "Renewal alert days must be greater than 0 for renewable documents.");

        return Result.Success(new DocumentType(
            Guid.NewGuid(),
            profileTypeId,
            name.Trim(),
            code.ToUpperInvariant().Trim(),
            description?.Trim(),
            isRenewable,
            renewalAlertDays,
            isRequired));
    }

    public void Update(
        string name,
        string? description,
        bool isRenewable,
        int renewalAlertDays,
        bool isRequired)
    {
        Name = name.Trim();
        Description = description?.Trim();
        IsRenewable = isRenewable;
        RenewalAlertDays = renewalAlertDays;
        IsRequired = isRequired;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
