using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class ServiceParameter : AggregateRoot
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Clés connues
    public const string DependenciesBlocking = "dependencies_blocking";

    private ServiceParameter() { }

    private ServiceParameter(Guid id, string key, string value, string? description)
        : base(id)
    {
        Key = key;
        Value = value;
        Description = description;
    }

    public static ServiceParameter Create(string key, string value, string? description = null)
        => new(Guid.NewGuid(), key, value, description);

    public void UpdateValue(string value) => Value = value;

    public bool AsBoolean() =>
        bool.TryParse(Value, out var result) && result;
}

