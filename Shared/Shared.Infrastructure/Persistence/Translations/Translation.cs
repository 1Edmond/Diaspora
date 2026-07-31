using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Translations;

public sealed class Translation<TOwner> : Entity where TOwner : class, ITranslatable
{
    public Guid OwnerId { get; private set; }
    public string CultureCode { get; private set; } = string.Empty;
    public Dictionary<string, string> Fields { get; private set; } = new();

    private Translation()
    {
    }

    private Translation(Guid ownerId, string cultureCode, Dictionary<string, string> fields) : base(Guid.NewGuid())
    {
        OwnerId = ownerId;
        CultureCode = NormalizeCulture(cultureCode);
        Fields = fields;
    }

    public static Translation<TOwner> Create(Guid ownerId, string cultureCode, Dictionary<string, string>? fields = null)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        if (string.IsNullOrWhiteSpace(cultureCode))
            throw new ArgumentException("CultureCode is required.", nameof(cultureCode));

        return new Translation<TOwner>(ownerId, cultureCode, fields ?? new Dictionary<string, string>());
    }

    public void SetField(string propertyName, string value)
    {
        Fields[propertyName] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public string? GetField(string propertyName) => Fields.GetValueOrDefault(propertyName);

    private static string NormalizeCulture(string cultureCode) => cultureCode.Trim().ToLowerInvariant();
}
