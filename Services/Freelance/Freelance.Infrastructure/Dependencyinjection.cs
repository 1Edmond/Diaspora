using Freelance.Application.Abstractions;
using Freelance.Application.Handlers.Commands;
using Freelance.Application.Interfaces;
using Freelance.Application.Mappings;
using Freelance.Domain.Entities;
using Freelance.Infrastructure.Consumers;
using Freelance.Infrastructure.Extensions;
using Freelance.Infrastructure.Persistence;
using Freelance.Infrastructure.Repositories;
using Freelance.Infrastructure.Seeders;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Infrastructure;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using System;
using System.Reflection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Runtime;

namespace Freelance.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        var connectionString = configuration.GetConnectionString("FreelanceDb")
            ?? throw new InvalidOperationException("Freelance database connection string is not configured.");

        services.AddScoped<IIntegrationEventMapper, FreelanceIntegrationEventMapper>();

        services.AddScoped<IJobCategoryRepository, JobCategoryRepository>();
        services.AddScoped<IJobTemplateRepository, JobTemplateRepository>();
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IJobCheckInRepository, JobCheckInRepository>();
        services.AddScoped<IJobReviewRepository, JobReviewRepository>();
        services.AddScoped<IFreelanceReputationRepository, FreelanceReputationRepository>();
        services.AddScoped<IWorkerJobPreferenceRepository, WorkerJobPreferenceRepository>();
        services.AddScoped<IWorkerVerificationStatusRepository, WorkerVerificationStatusRepository>();
        services.AddSingleton<JobCategorySeeder>();

        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureFreelanceDomainQueues();

            var optimizedConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = 60,
                CommandTimeout = 60,
                MaxPoolSize = 15,
                MinPoolSize = 0,
                Pooling = true,
                ConnectionIdleLifetime = 300,
                ConnectionPruningInterval = 10
            }.ToString();

            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine_freelance");

            options.ListenToRabbitQueue(ExchangesConstants.FreelanceEventsExchange)
                .PreFetchCount(15)
                .UseDurableInbox();

            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.ProfileEventsExchange)
                 .ToQueue("freelance-service.profile-events");

            // Queue entrante : événements Profiles
            options.ListenToRabbitQueue("freelance-service.profile-events")
              .PreFetchCount(20)
              .MaximumParallelMessages(3)
              .ListenerCount(2)
              .UseDurableInbox();

            // Handlers gèrent leur propre persistance via IUnitOfWork.
            options.Discovery.IncludeAssembly(typeof(CreateJobPostingCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(ExternalProfileActivatedConsumer).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });

        services.AddPostgresDatabase<FreelanceDbContext>(connectionString, migrationsAssembly);
        services.AddScoped<IApplicationDbContext, FreelanceDbContext>();
    }

    public static void BuildInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceVersion = null)
    {
        hostBuilder.AddCoreInfrastructure(configuration, serviceVersion);
    }

    public static async Task UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseCoreInfrastructure();

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FreelanceDbContext>();

        await context.Database.MigrateAsync();

        var seeder = new JobCategorySeeder(
            context,
            scope.ServiceProvider.GetRequiredService<ILogger<JobCategorySeeder>>()
        );
        await seeder.SeedAsync();
    }

    public static async Task PublishFreelanceResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly("FreelanceService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Registered {Count} permissions",
                declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}