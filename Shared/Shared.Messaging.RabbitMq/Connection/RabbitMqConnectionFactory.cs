using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMq.Connection;

/// <summary>
/// Factory that creates and manages a singleton RabbitMQ connection.
/// Thread-safe: IConnection is designed to be shared across threads.
/// </summary>
public sealed class RabbitMqConnectionFactory : IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RabbitMqConnectionFactory(
        RabbitMqSettings settings,
        ILogger<RabbitMqConnectionFactory> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_settings.ConnectionString),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                ClientProvidedName = _settings.ServiceName
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            _logger.LogInformation(
                "RabbitMQ connection established for service {ServiceName}",
                _settings.ServiceName);

            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _semaphore.Dispose();
    }
}

/// <summary>
/// Configuration settings for RabbitMQ connection.
/// </summary>
public sealed class RabbitMqSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}
