using System;
using System.Collections.Generic;

namespace Freelance.Application.Dtos;

public record WorkerJobPreferenceDto(
    Guid Id,
    Guid WorkerId,
    IReadOnlyCollection<Guid> PreferredCategoryIds,
    string? PreferredCity,
    string? PreferredCountry,
    double? PreferredLatitude,
    double? PreferredLongitude,
    int? PreferredRadiusKm,
    bool OnlyRemote,
    bool EmailNotificationsEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);