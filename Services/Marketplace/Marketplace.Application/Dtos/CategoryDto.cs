using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder,
    int ListingCount
);