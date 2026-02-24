using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IVaultTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct);
}
