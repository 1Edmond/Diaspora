using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Entities;

namespace Wallet.Infrastructure.Persistences;

public sealed class WalletDbContext(
    DbContextOptions<WalletDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<WalletDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService)
{
    public DbSet<Domain.Entities.Wallet> Wallets => Set<Domain.Entities.Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<WalletParameter> WalletParameters => Set<WalletParameter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletDbContext).Assembly);
        modelBuilder.HasDefaultSchema("wallet");
    }
}