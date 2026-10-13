using System;

namespace Shared.Application.Dtos;

public record TranslationDto(
    string CultureCode,
    Dictionary<string, string> Fields,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);