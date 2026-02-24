using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency)
{
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency is required.");

        return Result.Success(new Money(amount, currency.ToUpperInvariant().Trim()));
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
