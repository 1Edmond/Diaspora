using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Interfaces;
using Wallet.Domain.Entities;
using Wallet.Infrastructure.Persistences;

namespace Wallet.Infrastructure.Repositories;

public sealed class WalletParameterRepository(WalletDbContext context)
    : IWalletParameterRepository
{
    public async Task<WalletParameter?> GetByKeyAsync(
        string key, CancellationToken cancellationToken = default)
        => await context.WalletParameters
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);

    public async Task<IEnumerable<WalletParameter>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await context.WalletParameters
            .OrderBy(p => p.Key)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetDecimalAsync(
        string key, decimal defaultValue = 0m,
        CancellationToken cancellationToken = default)
    {
        var param = await GetByKeyAsync(key, cancellationToken);
        return param?.AsDecimal(defaultValue) ?? defaultValue;
    }

    public async Task<string> GetStringAsync(
        string key, string defaultValue = "",
        CancellationToken cancellationToken = default)
    {
        var param = await GetByKeyAsync(key, cancellationToken);
        return param?.AsString() ?? defaultValue;
    }
}
