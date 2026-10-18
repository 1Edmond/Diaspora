using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record CategoryPolicyDto(
    VerificationLevel MinPublishLevel,
    VerificationLevel MinBookLevel,
    decimal? MaxWeightKg,
    decimal? MaxValueAmount,
    bool RequiresEscrow,
    bool RequiresPreModeration,
    decimal DefaultCommissionRate
);

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder,
    int ListingCount,
    CategoryPolicyDto? Policy = null,
    List<Guid>? AllowedProfileTypeIds = null
);