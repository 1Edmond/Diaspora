using Elasticsearch.Net.Specification.IndicesApi;
using JasperFx.Core;
using JasperFx.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Application;
using Shared.Application.Abstractions;
using Shared.Application.Commands;
using Shared.Application.Interfaces;
using Shared.Application.Middlewares;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Handlers;
using Shared.Infrastructure.Inbox;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Infrastructure.Services;
using Shared.Logging.Configurations;
using Shared.Messaging.Constants;
using Shared.Messaging.Events.Authorization;
using Shared.Messaging.Events.Identity;
using Shared.Security;
using Shared.Security.Abstractions;
using Shared.Security.Services;
using StackExchange.Redis;
using System.Reflection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.RabbitMQ;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IHostBuilder AddCoreInfrastructure(this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceVersion = null)
    {

        hostBuilder.AddCoreLogging(
            configuration,
            serviceVersion);

        return hostBuilder;
    }

    public static IApplicationBuilder UseCoreInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseRouting();
        app.UseSecurity();
        

        return app;
    }

    public static WolverineOptions SetCoreInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
         Action<WolverineOptions>? additionalConfiguration = null
        )
    {

        services.AddCoreApplication();
        services.AddSecurity(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IGuidProvider, GuidProvider>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddHostedService<OutboxProcessor>();
        services.AddSingleton<IResourceDeclarationService, ResourceDeclarationService>();

        var serviceName = configuration.GetValue<string>("Messaging:ServiceName") 
            ?? throw new InvalidOperationException("Messaging service name is not configured.");
        var rabbitMqConnectionString = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");


        var options = services.AddCoreMessaging(serviceName, rabbitMqConnectionString, additionalConfiguration);


        var redisConnectionString = configuration.GetConnectionString("redis");
        services.AddStackExchangeRedisCache(options => { 
            options.Configuration = redisConnectionString;
            options.InstanceName = serviceName;
        });
        services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString!));

        return options;
    }

    public static IServiceCollection AddPostgresDatabase<TContext>(
      this IServiceCollection services,
      string connectionString,
      Assembly? migrationsAssembly = null)
      where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                
                if (migrationsAssembly != null)
                {
                    npgsqlOptions.MigrationsAssembly(
                        migrationsAssembly.GetName().Name);
                }
                npgsqlOptions.CommandTimeout(30);
            });

            
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        services.AddScoped<DbContext>(sp =>
            sp.GetRequiredService<TContext>());

        var context = services.BuildServiceProvider().GetRequiredService<TContext>();
        context.Database.EnsureCreated();

        return services;
    }


    public static WolverineOptions AddCoreMessaging(
      this IServiceCollection services,
      string serviceName,
      string rabbitMqConnectionString,
      Action<WolverineOptions>? additionalConfiguration = null
      )
    {
        WolverineOptions options = null!;

        //if(connectionString != null)
        //{
        //    services.AddDbContextWithWolverineIntegration<WolverineDbContext>(options =>
        //    {
        //        options.UseNpgsql(connectionString, npgsqlOptions =>
        //        {
        //            npgsqlOptions.EnableRetryOnFailure(0);
        //        });
        //    });
        //}


        services.AddWolverine(context =>
        {
            context.Discovery.IncludeAssembly(typeof(WarmupCommandHandler).Assembly);
            //context.UseFluentValidation();
           
            

            options = ConfigureWolverineMessaging(
                context,
                serviceName,
                rabbitMqConnectionString,
                additionalConfiguration
                );
        });
        
        return options;
    }

    public static WolverineOptions ConfigureWolverineMessaging(
     WolverineOptions options,

     string serviceName,
     string rabbitMqConnectionString,
     Action<WolverineOptions>? additionalConfiguration = null
     )
    {
        options.UseRabbitMq(rabbitMqConnectionString).AutoProvision();

        options.ServiceName = serviceName;
        additionalConfiguration?.Invoke(options);


        options.Durability.ScheduledJobPollingTime = TimeSpan.FromMinutes(2);
        options.Durability.NodeReassignmentPollingTime = TimeSpan.FromMinutes(5);
        options.Durability.RecoveryBatchSize = 100;
        options.Durability.DurabilityMetricsEnabled = false;
        options.PublishMessage<ResourcesDeclarationIntegrationEvent>()
           .ToRabbitQueue(ExchangesConstants.IdentityEventsExchange)
           .UseDurableOutbox();
        options.OnException<Exception>()
            .RetryWithCooldown(
                50.Milliseconds(),
                100.Milliseconds(),
                250.Milliseconds(),
                500.Milliseconds(),
                1.Seconds()
            );
        return options;
    }

}

