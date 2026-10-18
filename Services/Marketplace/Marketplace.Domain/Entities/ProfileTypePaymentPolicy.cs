using Marketplace.Domain.Enums;
using Shared.Domain.Primitives;
using System;

namespace Marketplace.Domain.Entities;

public class ProfileTypePaymentPolicy : AggregateRoot
{
    public Guid ProfileTypeId { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public AllowedSettlementPolicy AllowedPolicy { get; private set; } = AllowedSettlementPolicy.Both;
    public DirectCashTiming DefaultCashTiming { get; private set; } = DirectCashTiming.AtPickup;
    public bool DirectBookingFeeEnabled { get; private set; } = false;
    public decimal DirectBookingFeeFixedAmount { get; private set; } = 0m;
    public decimal DirectBookingFeeRate { get; private set; } = 0m;
    public string Currency { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private ProfileTypePaymentPolicy() { }

    public static ProfileTypePaymentPolicy Create(
        Guid profileTypeId,
        string countryCode,
        AllowedSettlementPolicy allowedPolicy = AllowedSettlementPolicy.Both,
        DirectCashTiming defaultCashTiming = DirectCashTiming.AtPickup,
        bool directBookingFeeEnabled = false,
        decimal directBookingFeeFixedAmount = 0m,
        decimal directBookingFeeRate = 0m,
        string currency = "EUR",
        bool isActive = true)
    {
        if (profileTypeId == Guid.Empty)
            throw new ArgumentException("ProfileTypeId is required.", nameof(profileTypeId));

        if (directBookingFeeRate < 0 || directBookingFeeRate > 1)
            throw new ArgumentOutOfRangeException(nameof(directBookingFeeRate), "Booking fee rate must be between 0 and 1.");

        if (directBookingFeeFixedAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(directBookingFeeFixedAmount), "Fixed fee amount cannot be negative.");

        return new ProfileTypePaymentPolicy
        {
            Id = Guid.NewGuid(),
            ProfileTypeId = profileTypeId,
            CountryCode = (countryCode ?? string.Empty).Trim().ToUpperInvariant(),
            AllowedPolicy = allowedPolicy,
            DefaultCashTiming = defaultCashTiming,
            DirectBookingFeeEnabled = directBookingFeeEnabled,
            DirectBookingFeeFixedAmount = directBookingFeeFixedAmount,
            DirectBookingFeeRate = directBookingFeeRate,
            Currency = (currency ?? string.Empty).Trim().ToUpperInvariant(),
            IsActive = isActive
        };
    }

    public void Update(
        AllowedSettlementPolicy allowedPolicy,
        DirectCashTiming defaultCashTiming,
        bool directBookingFeeEnabled,
        decimal directBookingFeeFixedAmount,
        decimal directBookingFeeRate,
        string currency,
        bool isActive)
    {
        if (directBookingFeeRate < 0 || directBookingFeeRate > 1)
            throw new ArgumentOutOfRangeException(nameof(directBookingFeeRate), "Booking fee rate must be between 0 and 1.");

        if (directBookingFeeFixedAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(directBookingFeeFixedAmount), "Fixed fee amount cannot be negative.");

        AllowedPolicy = allowedPolicy;
        DefaultCashTiming = defaultCashTiming;
        DirectBookingFeeEnabled = directBookingFeeEnabled;
        DirectBookingFeeFixedAmount = directBookingFeeFixedAmount;
        DirectBookingFeeRate = directBookingFeeRate;
        Currency = (currency ?? string.Empty).Trim().ToUpperInvariant();
        IsActive = isActive;
    }

    public decimal CalculateDirectPlatformFee(decimal agreedPrice)
    {
        if (!DirectBookingFeeEnabled || agreedPrice <= 0m)
            return 0m;

        decimal fee = DirectBookingFeeFixedAmount;
        if (DirectBookingFeeRate > 0m)
        {
            fee += Math.Round(agreedPrice * DirectBookingFeeRate, 2, MidpointRounding.AwayFromZero);
        }

        return fee;
    }
}
