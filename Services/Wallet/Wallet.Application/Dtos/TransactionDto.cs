using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Enums;

namespace Wallet.Application.Dtos;

public sealed record TransactionDto(
    Guid Id,
    Guid WalletId,
    TransactionType Type,
    TransactionDirection Direction,
    decimal Amount,
    decimal Fee,
    decimal BalanceAfter,
    string Currency,
    string Description,
    Guid? ReferenceId,
    string? ReferenceType,
    TransactionStatus Status,
    DateTime CreatedAt
);