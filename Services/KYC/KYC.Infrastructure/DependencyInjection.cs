using JasperFx.Resources;
using KYC.Application;
using KYC.Application.Abstractions;
using KYC.Application.Interfaces;
using KYC.Application.Handlers.IntegrationEvents;
using KYC.Infrastructure.Persistence;
using KYC.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Constants;
using Shared.Domain.Models;
using Shared.Infrastructure;
using Shared.Messaging.Constants;
using Shared.Security;
using System.Reflection;
using KYC.Infrastructure.Consumers;
using KYC.Infrastructure.Repositories;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace KYC.Infrastructure;

public static class DependencyInjection
{

    public static async Task AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, Assembly? migrationsAssembly)
    {

        services.AddApplicationDependyInjections();
        var connectionString = configuration.GetConnectionString("KycDb")
            ?? throw new InvalidOperationException("KYC database connection string is not configured.");
        
        
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IProfileDirectoryRepository, ProfileDirectoryRepository>();
        services.AddScoped<ICountryKycRequirementRepository, CountryKycRequirementRepository>();
        services.AddScoped<ProfileDirectoryConsumer>();

        services.SetCoreInfrastructureServices(configuration, options =>
        {
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

            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine_kyc");

            // Outbox : publication des 6 événements KYC + KycArrivalProofApproved vers kyc.events
            options.PublishMessage<Shared.Messaging.Events.Kyc.KYCVerificationCreatedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            options.PublishMessage<Shared.Messaging.Events.Kyc.KYCDocumentUploadedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            options.PublishMessage<Shared.Messaging.Events.Kyc.KYCVerificationSubmittedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            options.PublishMessage<Shared.Messaging.Events.Kyc.KYCMoreInfoRequestedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            options.PublishMessage<Shared.Messaging.Events.Kyc.KYCVerificationApprovedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            options.PublishMessage<Shared.Messaging.Events.Kyc.KYCVerificationRejectedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            options.PublishMessage<Shared.Messaging.Events.Kyc.KycArrivalProofApprovedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.KycEventsExchange);

            // Inbound : événements Profile pour alimenter la projection locale ProfileDirectory
            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.ProfileEventsExchange)
                .ToQueue("kyc-service.profile-events");

            options.ListenToRabbitQueue("kyc-service.profile-events")
                .PreFetchCount(20)
                .MaximumParallelMessages(3)
                .ListenerCount(2)
                .UseDurableInbox();

            // Handlers gèrent leur propre persistance via IUnitOfWork.
            options.Policies.UseDurableOutboxOnAllSendingEndpoints();

            options.Discovery.IncludeAssembly(typeof(KYC.Application.Handlers.Commands.ApproveKYCCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(Consumers.ProfileDirectoryConsumer).Assembly);
            if (migrationsAssembly != null)
                options.Discovery.IncludeAssembly(migrationsAssembly);
        });

        services.AddPostgresDatabase<KYCDbContext>(connectionString, migrationsAssembly);
        services.AddScoped<IKYCDbContext, KYCDbContext>();

    }

    public static void BuildKYCInfrastructure(this IHostBuilder hostBuilder, IConfiguration configuration,
        string? serviceVersion = null)
    {
        hostBuilder.AddCoreInfrastructure(configuration, serviceVersion);
    }

    public static async Task UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseCoreInfrastructure();

        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KYCDbContext>();

        await db.Database.EnsureCreatedAsync();
    }

    public static async Task PublishKYCResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceDeclarationService>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly("KYCService", typeof(T).Assembly);
        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("🚀 Published {Count} permissions",
            declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }


}
