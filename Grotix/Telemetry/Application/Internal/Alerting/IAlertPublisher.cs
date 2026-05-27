using GrotixBackend.Contracts.Integration.Telemetry;

namespace GrotixBackend.Telemetry.Application.Internal.Alerting;

public interface IAlertPublisher
{
    void Publish(AlertTriggeredIntegrationEvent alert);
}
