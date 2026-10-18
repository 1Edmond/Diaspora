using Marketplace.Domain.Enums;
using System;

namespace Marketplace.Application.Requests;

public record UpdateProfileTypePaymentPolicyRequest(
    string? CountryCode,
    AllowedSettlementPolicy AllowedPolicy,
    DirectCashTiming DefaultCashTiming,
    bool DirectBookingFeeEnabled,
    decimal DirectBookingFeeFixedAmount,
    decimal DirectBookingFeeRate,
    string Currency,
    bool IsActive
);
