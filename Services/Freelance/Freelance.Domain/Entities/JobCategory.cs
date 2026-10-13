using Shared.Domain.Primitives;
using System;

namespace Freelance.Domain.Entities;

public class JobCategory : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    private JobCategory() { }

    public static JobCategory Create(string name, string? description = null, bool isActive = true, int displayOrder = 0)
    {
        return new JobCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = isActive,
            DisplayOrder = displayOrder
        };
    }

    public void Update(string name, string? description, bool isActive, int displayOrder)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }
}