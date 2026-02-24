using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IVaultTransitService
{
    Task<byte[]> EncryptAsync(byte[] plain, string keyName, CancellationToken ct);
    Task<byte[]> DecryptAsync(byte[] cipher, string keyName, CancellationToken ct);
}

