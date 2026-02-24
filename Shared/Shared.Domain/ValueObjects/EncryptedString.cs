using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.ValueObjects;

public class EncryptedString : ValueObject
{
    public string ClearValue { get;  set; }

    public string? EncryptedValue { get; private set; }

    public bool IsEncrypted => !string.IsNullOrEmpty(EncryptedValue);

    private EncryptedString(){    }
    private EncryptedString(string clearValue, string? encryptedValue = null)
    {
        ClearValue = clearValue ?? string.Empty;
        EncryptedValue = encryptedValue;
    }

    public static EncryptedString FromClear(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Empty;

        return new EncryptedString(value, null);
    }

    public static EncryptedString FromEncrypted(string encryptedValue, string clearValue)
    {
        return new EncryptedString(clearValue, encryptedValue);
    }

    public static EncryptedString Empty => new(string.Empty, null);

    public EncryptedString WithEncryptedValue(string encryptedValue)
    {
        return new EncryptedString(ClearValue, encryptedValue);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ClearValue;
    }

    public override string ToString() => ClearValue;

    public static implicit operator string(EncryptedString encryptedString)
        => encryptedString.ClearValue;
}
