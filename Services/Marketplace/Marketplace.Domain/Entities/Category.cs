using Shared.Domain.Attributes;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Entities;

public class Category : AggregateRoot, ITranslatable
{
    [Translatable]
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    [Translatable]
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    public CategoryPolicy? Policy { get; private set; }

    private readonly List<ServiceListing> _listings = new();
    public IReadOnlyCollection<ServiceListing> Listings => _listings.AsReadOnly();

    public void SetPolicy(CategoryPolicy policy) => Policy = policy;

    private Category() { }

    public static Category Create(string name, string? description, string? iconUrl, int displayOrder)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = GenerateSlug(name),
            Description = description,
            IconUrl = iconUrl,
            IsActive = true,
            DisplayOrder = displayOrder
        };
        return category;
    }

    public void Update(string name, string? description, string? iconUrl, int displayOrder)
    {
        Name = name;
        Slug = GenerateSlug(name);
        Description = description;
        IconUrl = iconUrl;
        DisplayOrder = displayOrder;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static string GenerateSlug(string name)
        => name.ToLower().Replace(" ", "-").Replace("'", "");
}
