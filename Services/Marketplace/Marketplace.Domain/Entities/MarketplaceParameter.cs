using Shared.Domain.Primitives;
using System;

namespace Marketplace.Domain.Entities;

public sealed class MarketplaceParameter : Entity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private MarketplaceParameter() { }

    public MarketplaceParameter(string key, string value, string description) : base(Guid.NewGuid())
    {
        Key = key;
        Value = value;
        Description = description;
    }

    public void Update(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
