using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;

namespace Marketplace.Application.Commands;

public record UpdateCategoryPolicyCommand(
    Guid CategoryId,
    VerificationLevel MinPublishLevel,
    VerificationLevel MinBookLevel,
    decimal? MaxWeightKg,
    decimal? MaxValueAmount,
    bool RequiresEscrow,
    bool RequiresPreModeration,
    decimal DefaultCommissionRate
) : ICommand<Result>;
