using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Shared.Logging.Enrichers;
using Microsoft.AspNetCore.Builder;
using Serilog.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Shared.Logging.Configurations;

public static class LoggingConfiguration
{
    public static IHostBuilder AddCoreLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceVersion = null)
    {
        var elasticsearchConnectionString = configuration.GetConnectionString("elasticsearch");
        ElasticsearchConfiguration? elasticConfig = null;
        if (!string.IsNullOrWhiteSpace(elasticsearchConnectionString))
        {
            try
            {
                elasticConfig = ElasticsearchConfiguration.FromConnectionString(elasticsearchConnectionString);
            }
            catch
            {
                elasticConfig = null;
            }
        }


        var loggingConfig = new ServiceLoggingConfiguration
        {
            ServiceName = configuration.GetSection("Messaging:ServiceName").Value!,
            ServiceVersion = serviceVersion ?? "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        };

        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            ConfigureSerilog(
                loggerConfiguration,
                elasticConfig,
                loggingConfig,
                services);
        });

        return hostBuilder;
    }

    private static void ConfigureSerilog(
        LoggerConfiguration loggerConfiguration,
        ElasticsearchConfiguration? elasticConfig,
        ServiceLoggingConfiguration loggingConfig,
        IServiceProvider services)
    {
        loggerConfiguration
            .MinimumLevel.Is(loggingConfig.MinimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.With(new ServiceContextEnricher(
                loggingConfig.ServiceName,
                loggingConfig.ServiceVersion,
                loggingConfig.Environment));


        var httpContextAccessor = services.GetService<IHttpContextAccessor>();
        if (httpContextAccessor != null)
        {
            loggerConfiguration
                .Enrich.With(new UserContextEnricher(httpContextAccessor))
                .Enrich.With(new CorrelationIdEnricher(httpContextAccessor));
        }

        if (loggingConfig.EnableConsoleLogging)
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");
        }

        if (loggingConfig.EnableFileLogging && !string.IsNullOrEmpty(loggingConfig.LogFilePath))
        {
            loggerConfiguration.WriteTo.File(
                loggingConfig.LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);
        }

        if (elasticConfig != null && !string.IsNullOrWhiteSpace(elasticConfig.Url))
        {
        var elasticOptions = new ElasticsearchSinkOptions(new Uri(elasticConfig.Url))
        {
            AutoRegisterTemplate = elasticConfig.AutoRegisterTemplate,
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
            IndexFormat = $"{elasticConfig.IndexPrefix}-{loggingConfig.ServiceName.ToLower()}-{{0:yyyy.MM.dd}}",
            NumberOfShards = elasticConfig.NumberOfShards,
            NumberOfReplicas = elasticConfig.NumberOfReplicas,
            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
            FailureCallback = (e, r) => Console.WriteLine($"Unable to submit event {e.MessageTemplate}"),

            TemplateName = "blood-donation-logs-template",

            BatchPostingLimit = 50,
            Period = TimeSpan.FromSeconds(2),

            ModifyConnectionSettings = conn =>
            {
                if (!string.IsNullOrEmpty(elasticConfig.Username))
                {
                    conn.BasicAuthentication(elasticConfig.Username, elasticConfig.Password);
                }
                return conn;
            }
        };

        loggerConfiguration.WriteTo.Elasticsearch(elasticOptions);
        }
    }
}