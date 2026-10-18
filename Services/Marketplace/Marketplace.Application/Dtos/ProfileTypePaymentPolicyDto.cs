using Marketplace.Domain.Enums;
using System;

namespace Marketplace.Application.Dtos;

public record ProfileTypePaymentPolicyDto(
    Guid Id,
    Guid ProfileTypeId,
    string CountryCode,
    AllowedSettlementPolicy AllowedPolicy,
    DirectCashTiming DefaultCashTiming,
    bool DirectBookingFeeEnabled,
    decimal DirectBookingFeeFixedAmount,
    decimal DirectBookingFeeRate,
    string Currency,
    bool IsActive
);
