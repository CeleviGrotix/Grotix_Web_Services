using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GrotixBackend.BuildingBlocks.RabbitMq;

/// <summary>Mantiene una conexión lazy a RabbitMQ; si el broker no está disponible, devuelve null sin tumbar la API.</summary>
public sealed class RabbitMqConnectionHolder(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionHolder> logger)
    : IDisposable
{
    private readonly object _gate = new();
    private IConnection? _connection;
    private bool _loggedUnavailable;

    public IConnection? TryGetConnection()
    {
        if (!options.Value.Enabled)
            return null;

        lock (_gate)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            if (_connection != null)
            {
                try { _connection.Dispose(); } catch { /* ignore */ }
                _connection = null;
            }

            var opt = options.Value;

            try
            {
                ConnectionFactory factory;
                if (!string.IsNullOrWhiteSpace(opt.Uri))
                {
                    factory = new ConnectionFactory
                    {
                        Uri = new Uri(opt.Uri),
                        AutomaticRecoveryEnabled = true
                    };
                }
                else
                {
                    factory = new ConnectionFactory
                    {
                        HostName = opt.HostName,
                        Port = opt.Port,
                        UserName = opt.UserName,
                        Password = opt.Password,
                        VirtualHost = string.IsNullOrEmpty(opt.VirtualHost) ? "/" : opt.VirtualHost,
                        AutomaticRecoveryEnabled = true
                    };
                }
                _connection = factory.CreateConnection();
                logger.LogInformation(
                    "RabbitMQ connected ({Host}:{Port}, vhost={VHost}).",
                    opt.HostName,
                    opt.Port,
                    opt.VirtualHost);
                return _connection;
            }
            catch (Exception ex)
            {
                if (!_loggedUnavailable)
                {
                    logger.LogWarning(
                        ex,
                        "RabbitMQ no disponible; la API sigue sin broker. Arranca RabbitMQ o pon RabbitMq:Enabled=false.");
                    _loggedUnavailable = true;
                }

                return null;
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            try { _connection?.Dispose(); } catch { /* ignore */ }
            _connection = null;
        }
    }
}
