using Marketplace.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Commands;

public record CreatePaymentPolicyCommand(
    Guid ProfileTypeId,
    string? CountryCode,
    AllowedSettlementPolicy AllowedPolicy,
    DirectCashTiming DefaultCashTiming,
    bool DirectBookingFeeEnabled,
    decimal DirectBookingFeeFixedAmount,
    decimal DirectBookingFeeRate,
    string Currency,
    bool IsActive
) : ICommand<Result<Guid>>;
