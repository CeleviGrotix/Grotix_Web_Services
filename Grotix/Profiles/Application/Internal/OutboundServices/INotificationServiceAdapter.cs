namespace GrotixBackend.Profiles.Application.Internal.OutboundServices;

/// <summary>Cliente hacia Firebase/notificaciones (informe Profile).</summary>
public interface INotificationServiceAdapter
{
    Task SendWelcomeEmailAsync(string email, CancellationToken cancellationToken = default);
}
