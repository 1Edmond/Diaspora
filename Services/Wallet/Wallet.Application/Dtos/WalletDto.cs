using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Dtos;

public sealed record WalletDto(
    Guid Id,
    Guid ExternalProfileId,
    Guid UserId,
    string Currency,
    decimal Balance,
    bool IsActive,
    DateTime CreatedAt
);