using IdentityService.Application.Abstractions;
using IdentityService.Application.Mappings;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Outbox;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace IdentityService.Infrastructure.Persistence;

public class IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IIntegrationEventMapper integrationEventMapper,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        ILogger<IdentityDbContext> logger) 
        : BaseDbContext (options: options,
            _integrationEventMapper : integrationEventMapper,
            _dateTimeProvider: dateTimeProvider,
            _currentUserService: currentUserService,
            _logger: logger), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ResourcePermission> ResourcePermissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<UserPermission> UserPermissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        modelBuilder.HasDefaultSchema("identity");
    }

  }