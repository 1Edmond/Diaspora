using System;

namespace Freelance.Application.Dtos;

public record JobCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
);