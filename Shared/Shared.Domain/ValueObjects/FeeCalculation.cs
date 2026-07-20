using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace Shared.Domain.ValueObjects;

public sealed class FeeCalculation : ValueObject
{
    public decimal FeeAmount { get; private set; }
    public decimal TotalAmount { get; private set; }  // montant + frais
    public string Currency { get; private set; } = string.Empty;

    private FeeCalculation() { }

    private FeeCalculation(decimal feeAmount, decimal totalAmount, string currency)
    {
        FeeAmount = feeAmount;
        TotalAmount = totalAmount;
        Currency = currency;
    }

    public static FeeCalculation Calculate(
        decimal amount,
        string currency,
        FeeType feeType,
        decimal feeValue,
        decimal? feeMin = null,
        decimal? feeMax = null)
    {
        decimal fee = feeType switch
        {
            FeeType.Percentage => amount * feeValue / 100m,
            FeeType.Fixed => feeValue,
            _ => 0m
        };

        if (feeMin.HasValue && fee < feeMin.Value) fee = feeMin.Value;
        if (feeMax.HasValue && fee > feeMax.Value) fee = feeMax.Value;

        fee = Math.Round(fee, 2);

        return new FeeCalculation(fee, amount + fee, currency);
    }

    public static FeeCalculation Zero(string currency)
        => new(0m, 0m, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FeeAmount;
        yield return TotalAmount;
        yield return Currency;
    }
}
