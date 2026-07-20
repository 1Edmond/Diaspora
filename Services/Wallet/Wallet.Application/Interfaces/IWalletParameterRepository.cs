using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Entities;

namespace Wallet.Application.Interfaces;

public interface IWalletParameterRepository
{
    Task<WalletParameter?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletParameter>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<decimal> GetDecimalAsync(
        string key,
        decimal defaultValue = 0m,
        CancellationToken cancellationToken = default);

    Task<string> GetStringAsync(
        string key,
        string defaultValue = "",
        CancellationToken cancellationToken = default);
}
