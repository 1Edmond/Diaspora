using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Procedure.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Persistences;

public sealed class ProcedureDbContext(
    DbContextOptions<ProcedureDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcedureDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService)
{
    public DbSet<Domain.Entities.Procedure> Procedures => Set<Domain.Entities.Procedure>();
    public DbSet<UserProcedure> UserProcedures => Set<UserProcedure>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ProcedureDependency> ProcedureDependencies => Set<ProcedureDependency>();
    public DbSet<ProcedureDocument> ProcedureRequiredDocuments => Set<ProcedureDocument>();
    public DbSet<ServiceParameter> ServiceParameters => Set<ServiceParameter>();

    public DbSet<ProcedureLocation> ProcedureLocations => Set<ProcedureLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("procedure");
    }
}
