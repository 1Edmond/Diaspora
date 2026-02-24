using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace Notification.Infrastructure.Persistence;

public class NotificationDbContext : BaseDbContext
{
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options,
        IIntegrationEventMapper integrationEventMapper,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        ILogger<NotificationDbContext> logger
        )
         : base(options: options,
            _integrationEventMapper: integrationEventMapper,
            _dateTimeProvider: dateTimeProvider,
            _currentUserService: currentUserService,
            _logger: logger){ }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
