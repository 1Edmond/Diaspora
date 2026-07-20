using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Dtos;
using Wallet.Domain.Entities;

namespace Wallet.Application.Extensions;

public static class WalletMappingExtensions
{
    public static WalletDto ToDto(this Domain.Entities.Wallet w) =>
        new(w.Id, w.ExternalProfileId, w.UserId,
            w.Currency, w.Balance, w.IsActive, w.CreatedAt);

    public static TransactionDto ToDto(this Transaction t) =>
        new(t.Id, t.WalletId, t.Type, t.Direction,
            t.Amount, t.Fee, t.BalanceAfter, t.Currency,
            t.Description, t.ReferenceId, t.ReferenceType,
            t.Status, t.CreatedAt);
}