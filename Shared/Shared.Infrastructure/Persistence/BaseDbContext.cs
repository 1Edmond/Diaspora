using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Inbox;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence.Translations;
using Shared.Messaging.Events;
using Shared.Security.Persistence;
using System.Linq.Expressions;
using System.Text.Json;
using Wolverine;

public abstract class BaseDbContext(
        DbContextOptions options,
        IIntegrationEventMapper _integrationEventMapper,
        IDateTimeProvider _dateTimeProvider,
        ILogger<BaseDbContext> _logger,
        ICurrentUserService _currentUserService) : DbContext(options)
{

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new DataEncryptionKeyConfiguration());
        modelBuilder.ApplyConfiguration(new EncryptionMetadataConfiguration());

        var utcConverter = new ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {

            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        
        }

        modelBuilder.ApplyTranslatableEntities();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        var domainEvents = CollectDomainEvents();

        await SaveDomainEventsToOutboxAsync(domainEvents);

         var result = await base.SaveChangesAsync(cancellationToken);

        ClearDomainEvents();

        return result;
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries<Entity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = _dateTimeProvider.UtcNow;
                    entry.Entity.CreatedBy = _currentUserService.UserId ?? "System";
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = _dateTimeProvider.UtcNow;
                    entry.Entity.UpdatedBy = _currentUserService.UserId ?? "System";
                    break;
            }

            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.Delete(_currentUserService.UserId ?? "System");
            }
        }
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        return domainEvents;
    }

    protected virtual async Task SaveDomainEventsToOutboxAsync(List<IDomainEvent> domainEvents)
    {
        if (domainEvents.Count == 0)
            return;

        foreach (var domainEvent in domainEvents)
        {
            var integrationEvent = _integrationEventMapper.Map(domainEvent);

            if (integrationEvent == null)
                continue;

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = integrationEvent.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
                OccurredOnUtc = _dateTimeProvider.UtcNow,
                ProcessedOnUtc = null,
                Error = null,
                RetryCount = 0
            };

            var outboxAdded = await OutboxMessages.AddAsync(outboxMessage);

            if(_logger is not null && _logger.IsEnabled(LogLevel.Information))
            _logger?.LogInformation(
                "Domain event {EventType} saved to outbox with ID {OutboxId}",
                domainEvent.GetType().Name,
                outboxMessage.Id);
        }
    }

    private void ClearDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any());

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
    }


}