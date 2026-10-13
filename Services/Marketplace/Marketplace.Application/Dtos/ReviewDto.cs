using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record ReviewDto(
    Guid Id,
    Guid ListingId,
    Guid ReviewerId,
    string ReviewerName,
    int Rating,
    string Comment,
    bool IsVerified,
    DateTime CreatedAt,
    string? ProviderReply,
    DateTime? ProviderRepliedAt
);