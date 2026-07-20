using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Domain.Entities;

public sealed class WalletParameter : Entity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public const string TransferFeeType = "transfer_fee_type";
    public const string TransferFeeValue = "transfer_fee_value";
    public const string TransferFeeMin = "transfer_fee_min";
    public const string TransferFeeMax = "transfer_fee_max";

    private WalletParameter() { }

    private WalletParameter(Guid id, string key, string value, string description)
        : base(id)
    {
        Key = key;
        Value = value;
        Description = description;
    }

    public void UpdateValue(string newValue) => Value = newValue;

    public decimal AsDecimal(decimal defaultValue = 0m)
        => decimal.TryParse(Value,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var d) ? d : defaultValue;

    public string AsString() => Value;
}
