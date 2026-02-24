using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.ValueObjects;

public class EncryptedBytes : ValueObject
{
    public byte[] ClearValue { get; private set; }
    public byte[]? EncryptedValue { get; private set; }
    public bool IsEncrypted => EncryptedValue != null && EncryptedValue.Length > 0;

    private EncryptedBytes(byte[] clearValue, byte[]? encryptedValue = null)
    {
        ClearValue = clearValue ?? Array.Empty<byte>();
        EncryptedValue = encryptedValue;
    }

    public static EncryptedBytes FromClear(byte[] value)
    {
        return new EncryptedBytes(value ?? Array.Empty<byte>(), null);
    }

    public static EncryptedBytes FromEncrypted(byte[] encryptedValue, byte[] clearValue)
    {
        return new EncryptedBytes(clearValue, encryptedValue);
    }

    public static EncryptedBytes Empty => new(Array.Empty<byte>(), null);

    public EncryptedBytes WithEncryptedValue(byte[] encryptedValue)
    {
        return new EncryptedBytes(ClearValue, encryptedValue);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ClearValue;
    }
}